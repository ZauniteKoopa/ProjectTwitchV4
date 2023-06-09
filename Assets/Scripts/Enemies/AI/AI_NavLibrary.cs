using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI_NavLibrary
{
    // Main function to get unit to go to a specific location
    //  Pre: dest is the position on the nav mesh that the unit is trying to go to, pathExpiration is the time it takes for path to be stale (> 0f)
    //  Post: unit will move to the position gradually, getting out of sequence once reached position. Ends when path expires or hit player
    public static IEnumerator goToPosition(
        Vector3 dest,
        NavMeshAgent navMeshAgent,
        IUnitStatus movingUnit,
        float pathExpiration = float.MaxValue,
        float speedModifier = 1f
    ) {
        Debug.Assert(pathExpiration > 0f);

        bool pathFound = navMeshAgent.SetDestination(dest);
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = movingUnit.getMovementSpeed();

        // If path found, go to path
        if (pathFound) {
            // Variable to represent waiting a frame and set starting location
            WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();

            // Wait for path to finish calculating
            while (navMeshAgent.pathPending) {
                yield return waitFrame;
            }

            // Wait for unit to either hit the player or path expiration has hit
            float timer = 0f;
            navMeshAgent.speed = movingUnit.getMovementSpeed() * speedModifier;

            while (navMeshAgent.remainingDistance > 0.05f && timer < pathExpiration) {
                yield return waitFrame;

                navMeshAgent.speed = movingUnit.getMovementSpeed() * speedModifier;
                timer += Time.fixedDeltaTime;
            }
        }

        navMeshAgent.isStopped = true;
    }
}
