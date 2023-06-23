using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PassivePatrolPointBranch : IEnemyPassiveBranch
{
    private IUnitStatus enemyStats;

    private int patrolPointIndex = 0;
    private List<Vector3> patrolPointLocations;
    private NavMeshAgent navMeshAgent;
    private bool wasReset = true;

    [SerializeField]
    private Transform[] patrolPoints;
    [SerializeField]
    private float stopDuration = 1.0f;
    [SerializeField]
    private float firstConfusedDuration = 2.0f;
    [SerializeField]
    private float passiveMovementSpeedReduction = 0.75f;

    // Audio
    // private EnemyAudioManager enemyAudio;


    // On awake, set patrolPointLocations immediately and get NavMeshAgent
    private void Awake() {
        if (patrolPoints.Length <= 0) {
            Debug.LogError("NO PATROL POINTS FOUND FOR THIS UNIT");
        }

        // Initialize variables
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyStats = GetComponent<IUnitStatus>();
        // enemyAudio = GetComponent<EnemyAudioManager>();
        patrolPointLocations = new List<Vector3>();
        float yPos = transform.position.y;

        if (navMeshAgent == null){
            Debug.LogError("No nav mesh agent connected to this unit: " + transform, transform);
        }

        if (enemyStats == null){
            Debug.LogError("No IUnitStatus connected to this unit: " + transform, transform);
        }

        // if (enemyAudio == null) {
        //     Debug.LogError("No EnemyAudioManager connected to this unit: " + transform, transform);
        // }

        // record each patrol point location
        foreach (Transform patrolPoint in patrolPoints) {
            patrolPointLocations.Add(new Vector3(patrolPoint.position.x, yPos, patrolPoint.position.z));
            patrolPoint.gameObject.SetActive(false);
        }
    }


    // Main function to run the branch
    public override IEnumerator execute() {
        // If this is the first time running branch or branch was reset, find nearest patrol point to start
        if (wasReset) {
            wasReset = false;
            patrolPointIndex = getNearestPatrolPoint();
            navMeshAgent.isStopped = true;

            yield return new WaitForSeconds(firstConfusedDuration);

        } else {
            patrolPointIndex = 0;
        }

        // Go through the entire path in chronological order
        while (patrolPointIndex < patrolPointLocations.Count) {
            // Set destination
            Vector3 destPos = patrolPointLocations[patrolPointIndex];
            yield return AI_NavLibrary.goToPosition(destPos, navMeshAgent, enemyStats, speedModifier: passiveMovementSpeedReduction);

            // Wait for stop duration
            yield return new WaitForSeconds(stopDuration);

            // Increment patrol point index and start over
            patrolPointIndex++;
        }
    }


    // Main function to reset the branch when the overall tree gets overriden / switch branches
    public override void reset() {
        StopAllCoroutines();
        wasReset = true;
        navMeshAgent.isStopped = true;
        // enemyAudio.setFootstepsActive(false);
    }


    // Main helper function to get the index of the nearest patrol point location
    //  Returns the index of the nearest patrol point
    private int getNearestPatrolPoint() {
        float minDistance = Vector3.Distance(transform.position, patrolPointLocations[0]);
        int closestIndex = 0;

        for (int i = 1; i < patrolPointLocations.Count; i++) {
            float curDistance = Vector3.Distance(transform.position, patrolPointLocations[i]);

            if (minDistance > curDistance) {
                minDistance = curDistance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }


    // Main function to set passive patrol point branch, must be set before enemy is awake
    public void setPatrolPoints(Transform[] newPatrolPoints) {
        Debug.Assert(newPatrolPoints.Length > 0);

        patrolPoints = newPatrolPoints;
    }
}
