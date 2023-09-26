using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

// Aim assist class that works only on the XZ plane
public class XZ_AimAssist : IAimAssist
{
    // Collections of enemies to consider when adjusting aim
    private readonly object targetLock = new object();
    private HashSet<Transform> nearbyTargets = new HashSet<Transform>();
    private Dictionary<EnemyStatus, UnityAction> inRangeEnemyDelegates = new Dictionary<EnemyStatus, UnityAction>();

    [SerializeField]
    [Min(0.1f)]
    private float enemyAimAssistRadius = 0.5f;
    [SerializeField]
    private LayerMask aimMask;
    [SerializeField]
    [Min(0.1f)]
    private float minTargetScale = 1.25f;


    // Main function to adjust the aim direction so that it can accurately hit an enemy: O(E) Time and O(E) space (E = enemies considered)
    //  Pre: aimDirection is the direction of the attack, playerPosition is the position of the player
    //  Post: returns adjusted aimDirection IFF there's an enemy in that general direction
    public override Vector3 adjustAim(Vector3 aimDirection, Vector3 playerPosition) {
        // Flatten aimDirection so that only the XZ plane is considered
        aimDirection.y = 0f;

        // Cull enemies that do not fit the conditions of adjusting aimDirection
        List<Transform> enemyCandidates = forwardCull(aimDirection, playerPosition);
        enemyCandidates = proximityCull(aimDirection, playerPosition, enemyCandidates);

        // If there are no enemies to consider, just return the aimDirection as is. Else, return normalized direction from player to closest enemy
        if (enemyCandidates.Count == 0) {
            return aimDirection;
        } else {
            // Calculate the viable enemy candidate with the smallest distance to the player
            float minDistance = Vector3.Distance(playerPosition, enemyCandidates[0].transform.position);
            Transform topCandidate = enemyCandidates[0];

            for (int i = 1; i < enemyCandidates.Count; i++) {
                float curDistance = Vector3.Distance(playerPosition, enemyCandidates[i].position);

                if (curDistance < minDistance) {
                    minDistance = curDistance;
                    topCandidate = enemyCandidates[i];
                }
            }

            // Return normalized, flatten version of that vector
            Vector3 adjustedAim = topCandidate.position - playerPosition;
            adjustedAim.y = 0;
            return adjustedAim.normalized;
        }
    }


    // Main function to cull all enemies so that only enemies in the direction of aimDirection are considered AND ray is not blocked. This is THE FIRST CULL: O(E) time, O(E) space
    //  Pre: aimDirection is the direction of the attack (aimDirection.y == 0f), playerPosition is the position of the player, excludedEnemy is the enemy to be excluded
    //  Post: returns a list of enemies that are in the direction of aimDirection relative to the player (Not opposite). If there are none, returns an empty list
    private List<Transform> forwardCull(Vector3 aimDirection, Vector3 playerPosition) {
        Debug.Assert(aimDirection.y == 0f);

        List<Transform> forwardCullResults = new List<Transform>();

        // Iterate over every enemy in range to see if they are in the direction of aimDirection (not opposite)
        lock (targetLock) {
            foreach (Transform target in nearbyTargets) {
                // Get distance vector
                Vector3 tgtPosition = target.position;
                Vector3 distanceVector = tgtPosition - playerPosition;
                distanceVector.y = 0f;

                // check if there's a ray collision, indicating that the aim is blocked. Also make sure that the one blocking isn't the target itself
                RaycastHit hitInfo;
                bool aimBlocked = Physics.Raycast(transform.position, distanceVector.normalized, out hitInfo, distanceVector.magnitude, aimMask);
                if (aimBlocked) {
                    aimBlocked = (hitInfo.collider.transform != target);
                }

                // Get the Cos of the angle between distance vector and aim direction. (If positive, in the direction of aim. Else, in the opposite direction of aim)
                float cosAngle = Vector3.Dot(distanceVector, aimDirection);
                if (cosAngle > 0f && !aimBlocked) {
                    forwardCullResults.Add(target);
                }
            }
        }

        Debug.Assert(forwardCullResults != null);
        return forwardCullResults;
    }


    // Main function to cull all enemies so that only enemies that are in proximity to the aim direction (hits a circle around them) are considered: O(E) time, O(E) space
    //  Pre: aimDirection is the direction of the attack (aimDirection.y == 0f), playerPosition is the position of the player, enemyCandidates is a non-null list, enemyAimAssistRadius >= 0f
    //  Post: returns a list of enemies that are still viable. If there are none, returns an empty list
    private List<Transform> proximityCull(Vector3 aimDirection, Vector3 playerPosition, List<Transform> enemyCandidates) {
        Debug.Assert(aimDirection.y == 0f && enemyCandidates != null && enemyAimAssistRadius >= 0f);

        List<Transform> proximityCullResults = new List<Transform>();

        // Go through all enemies that are still viable to cull
        foreach (Transform target in enemyCandidates) {
            // Get distance vector
            Vector3 tgtPosition = target.position;
            Vector3 distanceVector = tgtPosition - playerPosition;
            distanceVector.y = 0f;

            // Get radius vector perpendicular to distance vector on the XZ plane 
            Vector3 radiusVector = Vector3.Cross(distanceVector, Vector3.up);
            radiusVector = radiusVector.normalized;

            // Get distance vector from playerPosition to radius point = (enemyPosition + (radius * radiusVector))
            float radius = Mathf.Min(target.lossyScale.x, minTargetScale) + enemyAimAssistRadius;
            Vector3 radiusPoint = tgtPosition + (radius * radiusVector);
            Vector3 radiusDistanceVector = radiusPoint - playerPosition;

            // Compare the angles between (distanceVector and aimDirection) AND (distanceVector and radiusDistanceVector). If aimDirction has a smaller angle (Cosine is bigger), add to the list
            float aimCosine = (Vector3.Dot(distanceVector, aimDirection)) / (distanceVector.magnitude * aimDirection.magnitude);
            float radiusCosine = (Vector3.Dot(distanceVector, radiusDistanceVector)) / (distanceVector.magnitude * radiusDistanceVector.magnitude);

            if (aimCosine >= radiusCosine) {
                proximityCullResults.Add(target);
            }
        }

        Debug.Assert(proximityCullResults != null);
        return proximityCullResults;
    }


    // Main function to reset aim assist
    //  Pre: none
    //  Post: aim assist is perfectly reset
    public override void reset() {
        lock (targetLock) {
            nearbyTargets.Clear();
        }
    }

    
    // Main on trigger enter
    private void OnTriggerEnter(Collider collider) {
        EnemyStatus tgt = collider.GetComponent<EnemyStatus>();

        if (tgt != null) {
            UnityAction curDelegate;
            tgt.deathEvent.AddListener(curDelegate = delegate { onEnemyDeath(tgt); });
            inRangeEnemyDelegates.Add(tgt, curDelegate);

            nearbyTargets.Add(tgt.transform);
        }
    }


    // Main on trigger exit
    private void OnTriggerExit(Collider collider) {
        EnemyStatus tgt = collider.GetComponent<EnemyStatus>();

        if (tgt != null) {
            nearbyTargets.Remove(tgt.transform);
            tgt.deathEvent.RemoveListener(inRangeEnemyDelegates[tgt]);
            inRangeEnemyDelegates.Remove(tgt);
        }
    }


    // Main event handler
    private void onEnemyDeath(EnemyStatus enemy) {
        nearbyTargets.Remove(enemy.transform);
        enemy.deathEvent.RemoveListener(inRangeEnemyDelegates[enemy]);
        inRangeEnemyDelegates.Remove(enemy);
    }

}
