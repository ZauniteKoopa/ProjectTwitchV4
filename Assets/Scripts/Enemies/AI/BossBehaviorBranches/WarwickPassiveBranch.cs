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
    private bool tracking = false;
    private IUnitStatus bloodiedTarget = null;
    
    
    // Main function to do additional initialization for branch
    //  Pre: none
    //  Post: sets branch up
    protected override void initialize(BossEnemyStatus bossEnemyStatus) {

    }


    // Main function to check whether or not the enemy is currently too focused on something to be distracted from specific task
    public override bool canBeDistracted() {
        return false;
    }


    // Main function to execute the branch
    //  Pre: tgt is the player, cannot equal null
    //  Post: executes aggressive branch
    public override IEnumerator execute(Transform tgt, BossEnemyStatus enemyStatus) {
        // Initially wait, puzzled
        Debug.Log("WHERE DID YOU GO?");
        yield return AI_NavLibrary.waitForFrames(initialPassiveFrames);

        // Run the navigation sequence and the track timer sequence in parallel
        Debug.Log("TRACKING");
        runningTrackingNavSequence = StartCoroutine(trackTowardsPlayer(tgt));
        tracking = true;
        float trackingTimer = 0f;
        while (trackingTimer < trackingDuration && bloodiedTarget == null) {
            yield return 0;
            trackingTimer += Time.deltaTime;
        }

        // Once tracking is done, either chase after bloodied enemy or chase after tracked player
        StopCoroutine(runningTrackingNavSequence);
        runningTrackingNavSequence =  null;
        tracking = false;

        // Blood frenzy case
        if (bloodiedTarget != null) {
            Debug.Log("BLOOD SPILLED! CAN'T CONTROL");
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
                bloodiedTarget.stun(true);
                bloodiedTarget.damage(99999f, true);
                enemyStats.healPercent(bloodFrenzyTargetHealPercent);
            }

        // Find twitch case
        } else {
            Debug.Log("FOUND YOU");
            yield return AI_NavLibrary.waitForFrames(numDiscoveryFrames);
            turnAggressiveEvent.Invoke();
        }

        bloodiedTarget = null;
    }


    // Main sequence to continuously move towards player
    private IEnumerator trackTowardsPlayer(Transform tgt) {
        while (true) {
            yield return AI_NavLibrary.goToPosition(
                tgt.position,
                navMeshAgent,
                enemyStats,
                pathExpiration: pathRefreshTime,
                speedModifier: trackingSpeedReduction
            );
        }
    }


    // Main function to reset the branch when the overall tree gets overriden / switch branches
    public override void reset() {
        if (runningTrackingNavSequence != null) {
            StopCoroutine(runningTrackingNavSequence);
            runningTrackingNavSequence = null;
        }
    }


    // Main event handler for when a unit nearby gets damaged
    private void onUnitDamagedNearby(IUnitStatus damagedUnit) {
        if (tracking) {
            bloodiedTarget = damagedUnit;
        }
    }
}
