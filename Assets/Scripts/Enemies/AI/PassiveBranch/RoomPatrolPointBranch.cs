using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RoomPatrolPointBranch : IEnemyPassiveBranch
{

    private IUnitStatus enemyStats;

    private NavMeshAgent navMeshAgent;
    private bool wasReset = true;
    private Vector3 roomPosition;
    private float roomWidth;
    private float roomLength;

    [SerializeField]
    [Min(0.1f)]
    private float stopDuration = 1.0f;
    [SerializeField]
    [Min(0.1f)]
    private float firstConfusedDuration = 2.0f;
    [SerializeField]
    [Range(0.1f, 1f)]
    private float passiveMovementSpeedReduction = 0.75f;


    // On awake, set patrolPointLocations immediately and get NavMeshAgent
    private void Awake() {
        // Initialize variables
        navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        enemyStats = GetComponent<IUnitStatus>();
        // enemyAudio = GetComponent<EnemyAudioManager>();

        if (navMeshAgent == null){
            Debug.LogError("No nav mesh agent connected to this unit: " + transform, transform);
        }

        if (enemyStats == null){
            Debug.LogError("No IUnitStatus connected to this unit: " + transform, transform);
        }

        // if (enemyAudio == null) {
        //     Debug.LogError("No EnemyAudioManager connected to this unit: " + transform, transform);
        // }
    }



    // Main function to run the branch
    public override IEnumerator execute() {
        // If this is the first time running branch or branch was reset, find nearest patrol point to start
        if (wasReset) {
            wasReset = false;
            navMeshAgent.isStopped = true;

            yield return new WaitForSeconds(firstConfusedDuration);
        }

        // Main loop of branch. Actually move if nav was set
        while (true) {
            // Set destination
            Vector3 destPos = getRandomPatrolPoint();
            yield return AI_NavLibrary.goToPosition(destPos, navMeshAgent, enemyStats, speedModifier: passiveMovementSpeedReduction);

            // Wait for stop duration
            yield return new WaitForSeconds(stopDuration);
        }
    }


    // Main function to reset the branch when the overall tree gets overriden / switch branches
    public override void reset() {
        StopAllCoroutines();
        wasReset = true;
        navMeshAgent.isStopped = true;
        // enemyAudio.setFootstepsActive(false);
    }


    // Main function to get random patrol point
    private Vector3 getRandomPatrolPoint() {
        Vector3 patrolPoint = new Vector3(
            Random.Range(-roomWidth / 2f, roomWidth / 2f),
            0f,
            Random.Range(-roomLength / 2f, roomLength / 2f)
        );

        patrolPoint += roomPosition;

        // Get nav mesh adjusted point 
        NavMeshHit hitInfo;
        NavMesh.SamplePosition(patrolPoint, out hitInfo, 10f, NavMesh.AllAreas);
        patrolPoint = hitInfo.position;

        return patrolPoint;
    }


    // Main function to assign IEnemyPassiveBranch layout
    public void setSpawnParameters(Vector3 center, float width, float length) {
        Debug.Assert(width > 0f && length > 0f);

        roomPosition = center;
        roomWidth = width;
        roomLength = length;
    }
}
