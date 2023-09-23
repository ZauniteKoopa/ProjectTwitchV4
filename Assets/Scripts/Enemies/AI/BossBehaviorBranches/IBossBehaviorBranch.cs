using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class IBossBehaviorBranch : MonoBehaviour
{
    // Variables
    protected NavMeshAgent navMeshAgent;
    protected IUnitStatus enemyStats;


    // Main variables for most aggroBranches
    private void Awake() {
        // Get reference variables and error check
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyStats = GetComponent<IUnitStatus>();

        if (navMeshAgent == null){
            Debug.LogWarning("No nav mesh agent connected to this unit: " + transform, transform);
        }

        if (enemyStats == null){
            Debug.LogError("No ITwitchUnitStatus connected to this unit: " + transform, transform);
        }

        // Do any branch specific initialization
        initialize();
    }


    // Main function to do additional initialization for branch
    //  Pre: none
    //  Post: sets branch up
    protected abstract void initialize();


    // Main function to execute the branch
    //  Pre: tgt is the player, cannot equal null
    //  Post: executes aggressive branch
    public abstract IEnumerator execute(Transform tgt, BossEnemyStatus enemyStatus);


    // Main function to reset the branch when the overall tree gets overriden / switch branches
    public abstract void reset();


    // Main function to do a hard reset: a reset in which the enemy respawns from scratch
    //  Pre: enemy respawns from scratch
    //  Post: resets as if nothing happened to this branch (by default, just reset)
    public virtual void hardReset() {
        reset();
    }
}
