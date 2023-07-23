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
    private NavMeshAgent navMeshAgent;
    private IUnitStatus unitStatus;

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
}
