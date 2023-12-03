using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.AI;

public class EnemyComponentBehaviorTree : IEnemyBehavior
{
    // Variables to control the tree
    private readonly object treeLock = new object();
    private Transform playerTgt = null;

    [Header("Enemy Branch Components")]
    [SerializeField]
    private IEnemyAggroBranch aggressiveBranch;
    [SerializeField]
    private IEnemyPassiveBranch passiveBranch;
    [SerializeField]
    [Min(0.1f)]
    private float passiveLookAtDuration = 1.5f;
    private NavMeshAgent navMeshAgent;
    private IUnitStatus unitStatus;

    [Header("Other Enemy Damaged Behavior")]
    [SerializeField]
    [Min(0.1f)]
    private float enemyDamagedLookAtDuration = 1f;
    [SerializeField]
    [Min(0.1f)]
    private float enemyDamagedInvestigateDuration = 3f;
    [SerializeField]
    [Range(0.01f, 1f)]
    private float investigateEnemyDamagedSpeedReduction = 0.5f;
    [SerializeField]
    [Range(0.01f, 0.2f)]
    private float investigationPathRefreshTime = 0.1f;
    [SerializeField]
    [Min(0.1f)]
    private float enemyDamagedConfusedDuration = 1f;
    private bool reactingToEnemyDamaged = false;
    private Vector3 suspectedPlayerPosition;


    private Coroutine currentBehaviorSequence = null;

    
    // On start, start the behavior tree sequence
    private void Start() {
        // Error check if branches are connected
        if (passiveBranch == null) {
            Debug.LogError("ERROR, passive branch not connected to this behavior tree: " + transform, transform);
        }

        if (aggressiveBranch == null) {
            Debug.LogError("ERROR, aggressive branch not connected to this behavior tree: " + transform, transform);
        }

        // Execute behav tree for the first time
        navMeshAgent = GetComponent<NavMeshAgent>();
        unitStatus = GetComponent<IUnitStatus>();

        if (unitStatus == null || navMeshAgent == null) {
            Debug.LogError("No unit status and nav mesh agent found for this enemy", transform);
        }

        // unitStatus.enemyResetEvent.AddListener(reset);
        unitStatus.unitDeathEvent.AddListener(onDeath);
        unitStatus.stunnedStartEvent.AddListener(onStunStart);
        unitStatus.stunnedEndEvent.AddListener(onStunEnd);

        if (currentBehaviorSequence != null) {
            StopCoroutine(currentBehaviorSequence);
        }
        currentBehaviorSequence = StartCoroutine(behaviorTreeSequence());
    }

    
    // The main behavior tree sequence
    private IEnumerator behaviorTreeSequence() {
        if (!inAggroState()) {
            passiveBranchActiveEvent.Invoke();
        } else {
            aggressiveBranchActiveEvent.Invoke();
        }

        while (true) {
            // Test to see if unit is aggressive (they are aggressive IFF a playerTgt is found)
            if (playerTgt == null) {
                yield return passiveBranch.execute();
            } else {
                yield return aggressiveBranch.execute(playerTgt);
            }
        }
    }


    // Main event handler function for when an enemy sensed a player
    //  Pre: player != null, enemy saw player
    public override void onSensedPlayer(Transform player) {
        playerTgt = player;

        if (unitStatus.canMove()) {
            lock (treeLock) {
                passiveBranch.reset();

                if (currentBehaviorSequence != null) {
                    StopCoroutine(currentBehaviorSequence);
                    reactingToEnemyDamaged = false;
                }

                if (unitStatus.isAlive()) {
                    currentBehaviorSequence = StartCoroutine(behaviorTreeSequence());
                }
            }
        }

        navMeshAgent.isStopped = true;
    }


    // Main event handler function for when an enemy lost sight of a player
    //  Pre: enemy lost sight of player and gave up chasing
    public override void onLostPlayer() {
        playerTgt = null;

        if (unitStatus.canMove()) {
            lock (treeLock) {
                aggressiveBranch.reset();

                StopCoroutine(currentBehaviorSequence);

                if (unitStatus.isAlive()) {
                    currentBehaviorSequence = StartCoroutine(behaviorTreeSequence());
                }
            }
        }

        navMeshAgent.isStopped = true;
    }


    // Main function to look at a specific direction
    //  Pre: lookDirection is the look direction that the enemy will be looking at (ONLY IN PASSIVE BRANCH)
    //  Post: player will stop all coroutines to look at something for a specified number of seconds before going back to work
    public override void lookAt(Vector3 lookAtDirection) {
        if (!inAggroState() && !reactingToEnemyDamaged) {
            StopCoroutine(currentBehaviorSequence);
            reactingToEnemyDamaged = false;

            if (unitStatus.isAlive()) {
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


    // Main function to react to other enemy being attacked
    //  Pre: lookDirection is the direction to look at (most likely direction to the other enemy), player transform is the transform of the player
    public override void reactToOtherEnemyDamaged(Vector3 lookAtDirection, Transform playerTransform) {
        suspectedPlayerPosition = playerTransform.position;

        if (!inAggroState() && !reactingToEnemyDamaged) {
            StopCoroutine(currentBehaviorSequence);

            if (unitStatus.isAlive()) {
                lookAtDirection = Vector3.ProjectOnPlane(lookAtDirection, Vector3.up).normalized;
                currentBehaviorSequence = StartCoroutine(enemyDamagedReactionSequence(lookAtDirection));
            }
        }
    }


    // Main private helper sequence to react to enemy being attack
    private IEnumerator enemyDamagedReactionSequence(Vector3 lookAtDirection) {
        navMeshAgent.isStopped = true;
        reactingToEnemyDamaged = true;
        
        yield return 0;

        // Look at enemy for a predetermined amount of seconds
        transform.forward = lookAtDirection;
        yield return new WaitForSeconds(enemyDamagedLookAtDuration);

        // Move towards player's for a predetermined amount of time
        float timer = 0f;
        while (timer < enemyDamagedInvestigateDuration) {
            yield return AI_NavLibrary.goToPosition(
                suspectedPlayerPosition,
                navMeshAgent,
                unitStatus,
                pathExpiration: investigationPathRefreshTime,
                speedModifier: investigateEnemyDamagedSpeedReduction
            );

            timer += investigationPathRefreshTime;
        }

        // Stop for a bit, vonfused. When confused, you can still react to other enemies damaged
        reactingToEnemyDamaged = false;
        navMeshAgent.isStopped = true;
        yield return new WaitForSeconds(enemyDamagedConfusedDuration);

        yield return behaviorTreeSequence();
    }


    // Main function for event handlers for stun start
    public void onStunStart() {
        lock (treeLock) {
            StopAllCoroutines();

            if (playerTgt == null) {
                passiveBranch.reset();
            } else {
                aggressiveBranch.reset();
            }
        }

        navMeshAgent.isStopped = true;
    }


    // Main function for handling when this enemy stun ended
    public void onStunEnd() {
        lock (treeLock) {
            if (unitStatus.isAlive()) {
                currentBehaviorSequence = StartCoroutine(behaviorTreeSequence());
            }
        }
    }


    // Main function to handle enemy reset
    public override void reset() {
        lock (treeLock) {
            playerTgt = null;

            aggressiveBranch.hardReset();
            passiveBranch.hardReset();
            reactingToEnemyDamaged = false;

            StopCoroutine(currentBehaviorSequence);
            if (unitStatus.isAlive()) {
                currentBehaviorSequence = StartCoroutine(behaviorTreeSequence());
            }
        }

        behaviorResetEvent.Invoke();
    }


    // Main function to handle death
    public override void onDeath(IUnitStatus status) {
        lock (treeLock) {
            aggressiveBranch.hardReset();
            passiveBranch.hardReset();
            StopAllCoroutines();
        }
    }


    // Main function to access whether or not you're in the passive state or aggressive state
    public override bool inAggroState() {
        return playerTgt != null;
    }
}
