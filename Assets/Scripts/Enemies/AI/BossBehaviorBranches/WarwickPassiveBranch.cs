using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarwickPassiveBranch : IBossBehaviorBranch
{
    [Header("Navigation")]
    [SerializeField]
    [Range(0.01f, 0.5f)]
    private float pathRefreshTime = 0.15f;
    private Coroutine runningTrackingNavSequence;
    private bool passiveBranchActive = false;

    [Header("Tracking Variables")]
    [SerializeField]
    [Min(0.01f)]
    private float trackingDuration = 10f;
    [SerializeField]
    [Range(0.01f, 1f)]
    private float trackingSpeedReduction = 0.65f;
    [SerializeField]
    [Min(1)]
    private int numDiscoveryFrames = 30;
    [SerializeField]
    [Min(1)]
    private int bloodFrenzyFrames = 90;
    [SerializeField]
    [Min(1)]
    private int initialPassiveFrames = 20;
    [SerializeField]
    [Min(0.01f)]
    private float minBloodTargetKillRange = 2f;
    [SerializeField]
    [Range(0.01f, 0.2f)]
    private float bloodFrenzyTargetHealPercent = 0.05f;
    [SerializeField]
    private WarwickBloodMark huntingMark;
    private bool tracking = false;
    private IUnitStatus bloodiedTarget = null;
    private bool connectedToPlayer = false;
    
    
    // Main function to do additional initialization for branch
    //  Pre: none
    //  Post: sets branch up
    protected override void initialize(BossEnemyStatus bossEnemyStatus) {

    }


    // Main function to check whether or not the enemy is currently too focused on something to be distracted from specific task
    public override bool canBeDistracted() {
        return bloodiedTarget == null;
    }


    // Main function to execute the branch
    //  Pre: tgt is the player, cannot equal null
    //  Post: executes aggressive branch
    public override IEnumerator execute(Transform tgt, BossEnemyStatus enemyStatus) {
        // Set up
        if (!connectedToPlayer) {
            PlayerStatus playerTgt = tgt.GetComponent<PlayerStatus>();
            playerTgt.playerHurtEvent.AddListener(delegate { onPlayerDamagedNearby(playerTgt); });
            connectedToPlayer = true;
        }
        passiveBranchActive = true;

        // Initially wait, puzzled
        tracking = true;
        yield return AI_NavLibrary.waitForFrames(initialPassiveFrames);

        // Run the navigation sequence and the track timer sequence in parallel
        yield return track(tgt);
        tracking = false;
        huntingMark.setTrackingProgress(1f, 1f);

        // Blood frenzy case
        if (bloodiedTarget != null && bloodiedTarget is EnemyStatus) {
            huntingMark.setTarget(bloodiedTarget.transform);
            bloodiedTarget.stun(true);

            yield return AI_NavLibrary.waitForFrames(bloodFrenzyFrames);

            // Keep moving until you're close enough to the target
            while (Vector3.ProjectOnPlane(bloodiedTarget.transform.position - transform.position, Vector3.up).magnitude >= minBloodTargetKillRange) {
                yield return AI_NavLibrary.goToPosition(
                    bloodiedTarget.transform.position,
                    navMeshAgent,
                    enemyStats,
                    pathExpiration: pathRefreshTime,
                    interrupted: () => Vector3.ProjectOnPlane(bloodiedTarget.transform.position - transform.position, Vector3.up).magnitude <= minBloodTargetKillRange
                );
            }

            if (bloodiedTarget.isAlive()) {
                bloodiedTarget.damage(99999f, true);
                enemyStats.healPercent(bloodFrenzyTargetHealPercent);
            }

            huntingMark.setActive(false);

        // Find twitch case
        } else {
            yield return AI_NavLibrary.waitForFrames(numDiscoveryFrames);
            yield return trackTowardsPlayer(tgt, 1.0f);
        }

        bloodiedTarget = null;
    }


    // Main sequence to track
    private IEnumerator track(Transform tgt) {
        huntingMark.setTarget(tgt);
        huntingMark.setTrackingProgress(0f, 1f);
        huntingMark.setActive(true);

        runningTrackingNavSequence = StartCoroutine(trackTowardsPlayer(tgt, trackingSpeedReduction));
        float trackingTimer = 0f;

        while (trackingTimer < trackingDuration && bloodiedTarget == null) {
            yield return 0;

            trackingTimer += Time.deltaTime;
            huntingMark.setTrackingProgress(trackingTimer, trackingDuration);
        }

        // Once tracking is done, either chase after bloodied enemy or chase after tracked player
        if (runningTrackingNavSequence != null) {
            StopCoroutine(runningTrackingNavSequence);
        }
        runningTrackingNavSequence = null;
        navMeshAgent.isStopped = true;
    }


    // Main sequence to continuously move towards player
    private IEnumerator trackTowardsPlayer(Transform tgt, float speedReduction) {
        while (passiveBranchActive) {
            yield return AI_NavLibrary.goToPosition(
                tgt.position,
                navMeshAgent,
                enemyStats,
                pathExpiration: pathRefreshTime,
                speedModifier: speedReduction
            );
        }
    }


    // Main function to reset the branch when the overall tree gets overriden / switch branches
    public override void reset() {
        if (runningTrackingNavSequence != null) {
            StopCoroutine(runningTrackingNavSequence);
            runningTrackingNavSequence = null;
        }

        passiveBranchActive = false;
        tracking = false;
        bloodiedTarget = null;
        huntingMark.setActive(false);
    }


    // Main event handler for when a unit nearby gets damaged
    public void onUnitDamagedNearby(IUnitStatus damagedUnit) {
        if (tracking && damagedUnit.isAlive()) {
            bloodiedTarget = damagedUnit;

            Vector3 rawDirVector = (damagedUnit.transform.position - transform.position).normalized;
            transform.forward = Vector3.ProjectOnPlane(rawDirVector, Vector3.up);
        }
    }


    // Main event handler for when player is damagewd
    public void onPlayerDamagedNearby(IUnitStatus damagedUnit) {
        if (tracking && damagedUnit.isAlive()) {
            bloodiedTarget = damagedUnit;

            Vector3 rawDirVector = (damagedUnit.transform.position - transform.position).normalized;
            transform.forward = Vector3.ProjectOnPlane(rawDirVector, Vector3.up);
        }
    }
}
