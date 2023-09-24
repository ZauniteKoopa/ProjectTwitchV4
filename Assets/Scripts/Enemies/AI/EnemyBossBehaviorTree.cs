using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBossBehaviorTree : IEnemyBehavior
{
    // Variables to control the tree
    private readonly object treeLock = new object();
    [SerializeField]
    private Transform playerTgt = null;

    [Header("Enemy Branch Components")]
    [SerializeField]
    private IBossBehaviorBranch aggroBranch;
    [SerializeField]
    private IBossBehaviorBranch scoutingBranch;
    [SerializeField]
    [Min(0.01f)]
    private float passiveLookAtDuration = 1.5f;
    private NavMeshAgent navMeshAgent;
    private BossEnemyStatus bossStatus;

    private Coroutine currentBehaviorSequence = null;
    private bool aggroState = false;



    // The main behavior tree sequence
    private IEnumerator behaviorTreeSequence() {
        Debug.Assert(playerTgt != null);

        while (true) {
            // Test to see if unit is aggressive (they are aggressive IFF a playerTgt is found)
            IBossBehaviorBranch curBranch = (aggroState) ? aggroBranch : scoutingBranch;
            yield return curBranch.execute(playerTgt, bossStatus);
        }
    }


    // Main function to initialize boss sequence
    public void spawnInBoss(Transform targetedPlayer) {
        bossStatus = GetComponent<BossEnemyStatus>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        bossStatus.stunnedStartEvent.AddListener(onStunStart);
        bossStatus.stunnedEndEvent.AddListener(onStunEnd);
        bossStatus.spawnInFinishEvent.AddListener(onSpawnInFinish);

        playerTgt = targetedPlayer;
        bossStatus.spawnIn();
    }


    // Main fucntion to set up when spawn in has been finished
    private void onSpawnInFinish() {
        aggroState = true;

        if (currentBehaviorSequence != null) {
            StopCoroutine(currentBehaviorSequence);
        }
        currentBehaviorSequence = StartCoroutine(behaviorTreeSequence());
    }


    // Main event handler function for when an enemy sensed a player
    //  Pre: player != null, enemy saw player
    public override void onSensedPlayer(Transform player) {
        if (!aggroState && scoutingBranch.canBeDistracted()) {
            aggroState = true;
            navMeshAgent.isStopped = true;

            if (bossStatus.canMove()) {
                lock (treeLock) {
                    scoutingBranch.reset();

                    if (currentBehaviorSequence != null) {
                        StopCoroutine(currentBehaviorSequence);
                    }

                    if (bossStatus.isAlive()) {
                        currentBehaviorSequence = StartCoroutine(behaviorTreeSequence());
                    }
                }
            }
        }
    }

    // Main event handler function for when an enemy lost sight of a player
    //  Pre: enemy lost sight of player and gave up chasing
    public override void onLostPlayer() {
        if (aggroState) {
            aggroState = false;
            navMeshAgent.isStopped = true;

            if (bossStatus.canMove()) {
                lock (treeLock) {
                    aggroBranch.reset();

                    if (currentBehaviorSequence != null) {
                        StopCoroutine(currentBehaviorSequence);
                    }

                    if (bossStatus.isAlive()) {
                        currentBehaviorSequence = StartCoroutine(behaviorTreeSequence());
                    }
                }
            }
        }
    }

    // Main function to handle reset
    public override void reset() {
        lock (treeLock) {
            playerTgt = null;

            aggroBranch.hardReset();
            scoutingBranch.hardReset();

            StopCoroutine(currentBehaviorSequence);
            if (bossStatus.isAlive()) {
                currentBehaviorSequence = StartCoroutine(behaviorTreeSequence());
            }
        }

        behaviorResetEvent.Invoke();
    }

    // Main function to handle death event of this unit
    public override void onDeath(IUnitStatus corpse) {
        lock (treeLock) {
            aggroBranch.hardReset();
            scoutingBranch.hardReset();
            StopAllCoroutines();
        }
    }

    // Main function to access whether or not you're in the passive state or aggressive state
    public override bool inAggroState() {
        return aggroState;
    }


    // Main function to look at a specific direction
    //  Pre: lookDirection is the look direction that the enemy will be looking at (ONLY IN PASSIVE BRANCH)
    //  Post: player will stop all coroutines to look at something for a specified number of seconds before going back to work
    public override void lookAt(Vector3 lookAtDirection) {
        if (!inAggroState() && scoutingBranch.canBeDistracted()) {
            if (currentBehaviorSequence != null) {
                StopCoroutine(currentBehaviorSequence);
            }

            if (bossStatus.isAlive()) {
                lookAtDirection = Vector3.ProjectOnPlane(lookAtDirection, Vector3.up).normalized;
                currentBehaviorSequence = StartCoroutine(lookAtSequence(lookAtDirection));
            }
        }
    }


    // Main private helper sequence to just look at an enemy for a predetermined number of seconds before going about typical behavior
    private IEnumerator lookAtSequence(Vector3 lookAtDirection) {
        navMeshAgent.isStopped = true;
        
        yield return 0;
        
        transform.forward = lookAtDirection;
        
        yield return new WaitForSeconds(passiveLookAtDuration);
        yield return behaviorTreeSequence();
    }


    // Main function for event handlers for stun start
    public void onStunStart() {
        lock (treeLock) {
            StopAllCoroutines();

            if (playerTgt == null) {
                scoutingBranch.reset();
            } else {
                aggroBranch.reset();
            }
        }

        navMeshAgent.isStopped = true;
    }


    // Main function for handling when this enemy stun ended
    public void onStunEnd() {
        lock (treeLock) {
            if (bossStatus.isAlive()) {
                currentBehaviorSequence = StartCoroutine(behaviorTreeSequence());
            }
        }
    }
}
