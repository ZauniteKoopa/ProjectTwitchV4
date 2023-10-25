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
    [SerializeField]
    [Min(0.1f)]
    private float proximityRange = 4f;
    private bool onAlert = false;
    [SerializeField]
    private float onAlertDuration = 0.2f;


    // Way to stay connected with nearby enemies
    [Header("Events")]
    public UnityEvent enemyAttackedEvent;
    public UnityEvent<IUnitStatus> otherEnemyAttackedEvent;
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
        fieldOfVision.changeObstacleMask(visionMask);
        brain.aggressiveBranchActiveEvent.AddListener(onBehaviorAggroBranchActivate);
        brain.passiveBranchActiveEvent.AddListener(onBehaviorPassiveBranchActivate);
    }


    // Main function to manage passive sensing each frame
    protected override void managePassiveSensing() {
        // Get data to make decisions
        PlayerStatus seenPlayerByVision = fieldOfVision.getSeenPlayer();
        PlayerStatus seenPlayerByAllies = getPlayerSeenByAllies();

        // If player's in proximity range
        if (playerTargetInProximityRange()) {
            brain.onSensedPlayer(nearbyTarget.transform);

        // If player is in vision cone
        } else if (seenPlayerByVision != null && seenPlayerByVision.canSeePlayer(enemyStatus)) {
            brain.onSensedPlayer(seenPlayerByVision.transform);

        // If allies nearby can see player
        } else if (seenPlayerByAllies != null && runningPlayerReaction == null) {
            runningPlayerReaction = StartCoroutine(reactToOtherEnemyFindingPlayer(seenPlayerByAllies, enemyAllyNoticesPlayerReactionTime));

        }
    }


    // Main action of actually forgetting the player in terms of sensing
    protected override void forgetPlayer() {
        base.forgetPlayer();
    }


    // Main event handler function for when this enemy is attacked by the player: look at the player if this happens
    private void onAttackedByPlayer() {
        if (nearbyTarget != null) {
            brain.lookAt(nearbyTarget.transform.position - transform.position);
            enemyAttackedEvent.Invoke();

            if (!brain.inAggroState() && !onAlert) {
                StartCoroutine(onAlertSequence());
            } 
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

        if (enemyStatus.isAlive()) {
            runningReaction = StartCoroutine(reactToStimulus(enemySensor.transform.position - transform.position, enemyAttackedReactionTime));
        }
        
        otherEnemyAttackedEvent.Invoke(enemySensor.transform.parent.GetComponent<IUnitStatus>());
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
        if (seenPlayer != null && !brain.inAggroState()) {
            brain.onSensedPlayer(seenPlayer.transform);
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
            if (otherEnemy.Key.brain.inAggroState() && otherEnemy.Key.nearbyTarget != null) {
                Vector3 targetPosition = otherEnemy.Key.nearbyTarget.transform.position;
                Vector3 rayDir = targetPosition - transform.position;
                float rayDist = rayDir.magnitude;
                bool seeEnemy = !Physics.Raycast(transform.position, rayDir, rayDist, getVisionMask());

                if (seeEnemy && otherEnemy.Key.nearbyTarget.canSeePlayer(enemyStatus)) {
                    return otherEnemy.Key.nearbyTarget;
                }
            }
        }

        return null;
    }


    // Main private helepr function to see if the nearby target is in proximity range
    private bool playerTargetInProximityRange() {
        // if no nearbyTarget, return false immediately
        if (nearbyTarget == null) {
            return false;
        }

        // Get data
        Vector3 targetPosition = nearbyTarget.transform.position;
        Vector3 rayDir = targetPosition - transform.position;
        float rayDist = rayDir.magnitude;

        // If player is not in proximity, return false immediately
        if (Vector3.Distance(targetPosition, transform.position) > proximityRange) {
            return false;
        }

        // Check if there's any barriers in between
        return !Physics.Raycast(transform.position, rayDir, rayDist, getVisionMask());
    }


    // Main function to handle onAlert period
    private IEnumerator onAlertSequence() {
        onAlert = true;
        fieldOfVision.changeObstacleMask(onAlertVisionMask);

        yield return new WaitForSeconds(onAlertDuration);

        onAlert = false;
        fieldOfVision.changeObstacleMask(visionMask);
    }


    // Main function to get vision mask
    private LayerMask getVisionMask() {
        return (onAlert || brain.inAggroState()) ? onAlertVisionMask : visionMask;
    }


    // Event handler to handle when the aggressive branch activates
    private void onBehaviorAggroBranchActivate() {
        fieldOfVision.showVision(false);
        fieldOfVision.changeObstacleMask(onAlertVisionMask);
    }


    // Event handler to handle when the passive branch activates
    private void onBehaviorPassiveBranchActivate() {
        fieldOfVision.showVision(true);
        fieldOfVision.changeObstacleMask(visionMask);
    }
}
