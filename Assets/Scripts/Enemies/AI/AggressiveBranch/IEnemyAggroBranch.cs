using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class IEnemyAggroBranch : MonoBehaviour
{
    // Variables
    protected NavMeshAgent navMeshAgent;
    protected IUnitStatus enemyStats;
    protected GeneralEnemyAudioManager audioManager;


    // Main variables for most aggroBranches
    private void Awake() {
        // Get reference variables and error check
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyStats = GetComponent<IUnitStatus>();
        audioManager = GetComponent<GeneralEnemyAudioManager>();

        if (navMeshAgent == null){
            Debug.LogError("No nav mesh agent connected to this unit: " + transform, transform);
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
    public abstract IEnumerator execute(Transform tgt);


    // Main function to reset the branch when the overall tree gets overriden / switch branches
    public abstract void reset();


    // Main function to do a hard reset: a reset in hich the enemy respawns from scratch
    //  Pre: enemy respawns from scratch
    //  Post: resets as if nothing happened to this branch (by default, just reset)
    public virtual void hardReset() {
        reset();
    }
}
