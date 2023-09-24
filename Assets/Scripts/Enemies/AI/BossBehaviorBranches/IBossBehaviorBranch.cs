using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public abstract class IBossBehaviorBranch : MonoBehaviour
{
    // Variables
    protected NavMeshAgent navMeshAgent;
    protected BossEnemyStatus enemyStats;
    public UnityEvent turnAggressiveEvent;


    // Main variables for most aggroBranches
    private void Awake() {
        // Get reference variables and error check
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyStats = GetComponent<BossEnemyStatus>();

        if (navMeshAgent == null){
            Debug.LogWarning("No nav mesh agent connected to this unit: " + transform, transform);
        }

        if (enemyStats == null){
            Debug.LogError("No ITwitchUnitStatus connected to this unit: " + transform, transform);
        }

        // Do any branch specific initialization
        initialize(enemyStats);
    }


    // Main function to do additional initialization for branch
    //  Pre: none
    //  Post: sets branch up
    protected abstract void initialize(BossEnemyStatus enemyStats);


    // Main function to execute the branch
    //  Pre: tgt is the player, cannot equal null
    //  Post: executes aggressive branch
    public abstract IEnumerator execute(Transform tgt, BossEnemyStatus enemyStatus);


    // Main function to reset the branch when the overall tree gets overriden / switch branches
    public abstract void reset();


    // Main function to check whether or not the enemy is currently too focused on something to be distracted from specific task
    public abstract bool canBeDistracted();


    // Main function to do a hard reset: a reset in which the enemy respawns from scratch
    //  Pre: enemy respawns from scratch
    //  Post: resets as if nothing happened to this branch (by default, just reset)
    public virtual void hardReset() {
        reset();
    }
}
