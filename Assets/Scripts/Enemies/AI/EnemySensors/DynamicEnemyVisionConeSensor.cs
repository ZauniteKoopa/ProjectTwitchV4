using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DynamicEnemyVisionConeSensor : EnemyVisionSensor
{
    // Way to passively sense
    [Header("Passive Sensing")]
    [SerializeField]
    private FieldOfVision fieldOfVision = null;

    // Way to stay connected with nearby enemies
    [Header("Events")]
    public UnityEvent enemyAttackedEvent;
    private Dictionary<DynamicEnemyVisionConeSensor, UnityAction[]> nearbyEnemySensorDelegates = new Dictionary<DynamicEnemyVisionConeSensor, UnityAction[]>();

    [Header("Reaction Time")]
    [SerializeField]
    [Min(0f)]
    private float enemyAttackedReactionTime = 0.2f;
    [SerializeField]
    [Min(0f)]
    private float enemyAllyNoticesPlayerReactionTime = 0.5f;
    private Coroutine runningReaction = null;
    private Coroutine runningPlayerReaction = null;


    // On awake, error check and get body
    protected override void initialize() {
        base.initialize();

        if (fieldOfVision == null) {
            Debug.LogError("Field of vision is not connected to this sensor", transform);
        }

        enemyStatus.enemyNoticesDamageEvent.AddListener(onAttackedByPlayer);
    }


    // Main function to manage passive sensing each frame
    protected override void managePassiveSensing() {
        // If player is just in vision
        PlayerStatus seenPlayer = fieldOfVision.getSeenPlayer();

        if (seenPlayer != null && seenPlayer.canSeePlayer(enemyStatus)) {
            brain.onSensedPlayer(seenPlayer.transform);
            fieldOfVision.showVision(false);
        }

        // If another enemy just has it and isn't currently reacting
        seenPlayer = getPlayerSeenByAllies();
        if (seenPlayer != null && seenPlayer.canSeePlayer(enemyStatus) && runningPlayerReaction == null) {
            runningPlayerReaction = StartCoroutine(reactToOtherEnemyFindingPlayer(seenPlayer, enemyAllyNoticesPlayerReactionTime));
        }
    }


    // Main action of actually forgetting the player in terms of sensing
    protected override void forgetPlayer() {
        base.forgetPlayer();
        fieldOfVision.showVision(true);
    }


    // Main event handler function for when this enemy is attacked by the player: look at the player if this happens
    private void onAttackedByPlayer() {
        if (nearbyTarget != null) {
            brain.lookAt(nearbyTarget.transform.position - transform.position);
            enemyAttackedEvent.Invoke();
        }
    }


    // -----------
    //  Reacting to events concerning to other enemies
    // -----------


    // If enemy enters zone, keep track of enemy
    protected override void onTriggerEnterExt(Collider collider) {
        DynamicEnemyVisionConeSensor otherSensor = collider.GetComponentInChildren<DynamicEnemyVisionConeSensor>();

        if (otherSensor != null && !nearbyEnemySensorDelegates.ContainsKey(otherSensor)) {

            // Set up delegates
            UnityAction[] delegates = new UnityAction[2];
            delegates[0] = delegate { onOtherEnemyAttacked(otherSensor); };
            delegates[1] = delegate { onOtherEnemyDeath(otherSensor); };

            // Connect delegates
            otherSensor.enemyAttackedEvent.AddListener(delegates[0]);
            otherSensor.enemyStatus.deathEvent.AddListener(delegates[1]);
            nearbyEnemySensorDelegates.Add(otherSensor, delegates);
        }
    }


    // If enemy exits zone, lose track of enemy
    protected override void onTriggerExitExt(Collider collider) {
        DynamicEnemyVisionConeSensor otherSensor = collider.GetComponentInChildren<DynamicEnemyVisionConeSensor>();

        if (otherSensor != null && nearbyEnemySensorDelegates.ContainsKey(otherSensor)) {
            // Disconnect from delegates
            otherSensor.enemyAttackedEvent.RemoveListener(nearbyEnemySensorDelegates[otherSensor][0]);
            otherSensor.enemyStatus.deathEvent.RemoveListener(nearbyEnemySensorDelegates[otherSensor][1]);

            // Remove
            nearbyEnemySensorDelegates.Remove(otherSensor);
        }
    }


    // If enemy dies, lose track of enemy
    private void onOtherEnemyDeath(DynamicEnemyVisionConeSensor enemySensor) {
        if (nearbyEnemySensorDelegates.ContainsKey(enemySensor)) {
            // Disconnect from delegates
            enemySensor.enemyAttackedEvent.RemoveListener(nearbyEnemySensorDelegates[enemySensor][0]);
            enemySensor.enemyStatus.deathEvent.RemoveListener(nearbyEnemySensorDelegates[enemySensor][1]);

            // Remove
            nearbyEnemySensorDelegates.Remove(enemySensor);
        }
    }


    // If enemy is attacked, look at enemy
    private void onOtherEnemyAttacked(DynamicEnemyVisionConeSensor enemySensor) {
        if (runningReaction != null) {
            StopCoroutine(runningReaction);
        }

        runningReaction = StartCoroutine(reactToStimulus(enemySensor.transform.position - transform.position, enemyAttackedReactionTime));
    }


    // Private delayed sequence method to react to an enemy ally being attacked
    private IEnumerator reactToStimulus(Vector3 lookDirection, float reactionTime) {
        yield return new WaitForSeconds(reactionTime);
        brain.lookAt(lookDirection);
        runningReaction = null;
    }


    // Private delayed sequence method to react to an enemy finding the player
    private IEnumerator reactToOtherEnemyFindingPlayer(PlayerStatus seenPlayer, float reactionTime) {
        yield return new WaitForSeconds(reactionTime);

        // Check conditions again
        seenPlayer = getPlayerSeenByAllies();
        if (seenPlayer != null && seenPlayer.canSeePlayer(enemyStatus) && !brain.inAggroState()) {
            brain.onSensedPlayer(seenPlayer.transform);
            fieldOfVision.showVision(false);
        }

        runningPlayerReaction = null;
    }


    // Private helper function to see if you can actually see the unit
    //  Post: returns whether or not player is in sight range of enemy AND no objects are blocking
    protected override bool canSeePlayerInRange() {
        return base.canSeePlayerInRange() || getPlayerSeenByAllies() != null;
    }


    // Main function to check if any of the enemy allies found the player
    //  Pre: return the player if enemy allies have found him and are attacking him. return null otherwise
    private PlayerStatus getPlayerSeenByAllies() {
        foreach(KeyValuePair<DynamicEnemyVisionConeSensor, UnityAction[]> otherEnemy in nearbyEnemySensorDelegates) {
            if (otherEnemy.Key.brain.inAggroState()) {
                return otherEnemy.Key.nearbyTarget;
            }
        }

        return null;
    }
}
