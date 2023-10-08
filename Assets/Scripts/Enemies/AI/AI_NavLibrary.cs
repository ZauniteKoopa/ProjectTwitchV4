using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyAttackAnimationState {
    ANTICIPATION = 0,
    ATTACK = 1,
    RECOIL = 2
}

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
        float speedModifier = 1f,
        System.Func<bool> interrupted = null
    ) {
        Debug.Assert(pathExpiration > 0f);

        bool pathFound = navMeshAgent.SetDestination(dest);
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = movingUnit.getMovementSpeed() * speedModifier;

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

            while (navMeshAgent.remainingDistance > 0.05f && timer < pathExpiration && (interrupted == null || !interrupted())) {
                yield return waitFrame;

                navMeshAgent.speed = movingUnit.getMovementSpeed() * speedModifier;
                timer += Time.fixedDeltaTime;
            }
        }

        navMeshAgent.isStopped = true;
    }


    // Private helper method to wait for a specified amount of frames
    //  Pre: numFrames > 0
    //  Post: wait a number amount of frames before moving on after this sequence
    public static IEnumerator waitForFrames(int numFrames, System.Func<bool> interrupted = null) {
        Debug.Assert(numFrames > 0);

        int f = 0;

        while (f < numFrames && (interrupted == null || !interrupted())) {
            yield return 0;
            if (Time.timeScale != 0f) {
                f++;
            }
        }
    }
}
