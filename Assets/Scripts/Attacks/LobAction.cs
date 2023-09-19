using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class LobAction : MonoBehaviour
{
    public DeployableHitbox deployable;
    private const float MIN_START_TIME = 0.25f;
    private const float MAX_LOB_HEIGHT = 2f;
    private const float LOB_COLLISION_LAYER_OFFSET = 0.7f;
    

    // Main function to lob the projectile from src to tgt
    //  Pre: The vector3s are positions within the game world, lobSpeed is the speed of the lobbing projectile
    //  Post: lobs the projectile
    public void lob(Vector3 src, Vector3 tgt, float lobSpeed, PoisonVial poison, Transform attacker) {
        Debug.Assert(lobSpeed > 0f);

        if (deployable != null) {
            deployable.source = attacker;
        }

        float lobTime = (Vector3.Distance(src, tgt) / lobSpeed) + MIN_START_TIME;
        StartCoroutine(lobSequence(src, tgt, lobTime, poison));
    }


    // Main function to lob the projectile from src to tgt
    //  Pre: The vector3s are positions within the game world, lobSpeed is the speed of the lobbing projectile
    //  Post: lobs the projectile
    public void lobWithTime(Vector3 src, Vector3 tgt, float lobTime, PoisonVial poison) {
        Debug.Assert(lobTime > 0f);
        StartCoroutine(lobSequence(src, tgt, lobTime, poison));
    }


    // Main function to lob projectile to a dynamic moving target
    //  Pre: The vector3s are positions within the game world, lobSpeed is the speed of the lobbing projectile
    //  Post: lobs the projectile
    public void dynamicLobWithTime(Vector3 src, Transform tgt, float lobTime, PoisonVial poison) {
        Debug.Assert(lobTime > 0f);
        StartCoroutine(dynamicLobSequence(src, tgt, lobTime, poison));
    }


    // Main function to lob a projectile around a source position
    public void lobAroundRadius(Vector3 src, float lobRadius, float lobSpeed, LayerMask lobCollisionMask) {
        lobAroundRadius(src, lobRadius, 0f, lobSpeed, lobCollisionMask);
    }



    // Main function to lob a projectile around a source position
    public void lobAroundRadius(Vector3 src, float maxLobRadius, float minLobRadius, float lobSpeed, LayerMask lobCollisionMask) {
        Debug.Assert(maxLobRadius > 0f && lobSpeed > 0f && minLobRadius > -0.0001);

        Vector3 lootDropDir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
        float curLootDistance = Random.Range(minLobRadius, maxLobRadius);
        Vector3 dest = src + (curLootDistance * lootDropDir);

        // Raycast in that direction to get the dest that considers collision
        RaycastHit hitInfo;
        if (Physics.Raycast(src, lootDropDir, out hitInfo, curLootDistance + 0.5f, lobCollisionMask)) {
            dest = hitInfo.point - (LOB_COLLISION_LAYER_OFFSET * lootDropDir);
        }

        // Fire lob action at that point
        lob(src, dest, lobSpeed, null, null);
    }


    // Main private IEnumerator to handle lobbing action
    //  Pre: src and tgt are positions within the gameworld, lobTime is the time it takes to do the entire sequence
    private IEnumerator lobSequence(Vector3 src, Vector3 tgt, float lobTime, PoisonVial poison) {
        Debug.Assert(lobTime > 0f);

        // Set up
        float timer = 0f;
        Vector3 midPoint = (src + tgt) / 2f;
        midPoint.y += MAX_LOB_HEIGHT;
        float halfTime = lobTime / 2f;

        // First arc
        while (timer < halfTime) {
            yield return 0;

            timer += Time.deltaTime;
            transform.position = Vector3.Slerp(src, midPoint, timer / halfTime);
        }

        // Second arc
        timer = 0f;
        while (timer < halfTime) {
            yield return 0;

            timer += Time.deltaTime;
            transform.position = Vector3.Slerp(midPoint, tgt, timer / halfTime);
        }

        // Finish
        transform.position = tgt;
        
        if (deployable != null) {
            deployable.transform.parent = null;
            deployable.deploy(poison);
        }

        Object.Destroy(gameObject);
    }


    // Main private IEnumerator to handle lobbing action
    //  Pre: src and tgt are positions within the gameworld, lobTime is the time it takes to do the entire sequence
    private IEnumerator dynamicLobSequence(Vector3 src, Transform tgt, float lobTime, PoisonVial poison) {
        Debug.Assert(lobTime > 0f);

        // Set up
        float timer = 0f;
        Vector3 midPoint = (src + tgt.position) / 2f;
        midPoint.y += MAX_LOB_HEIGHT;
        float halfTime = lobTime / 2f;

        // First arc
        while (timer < halfTime) {
            yield return 0;

            timer += Time.deltaTime;
            transform.position = Vector3.Slerp(src, midPoint, timer / halfTime);
        }

        // Second arc
        timer = 0f;
        while (timer < halfTime) {
            yield return 0;

            timer += Time.deltaTime;
            transform.position = Vector3.Slerp(midPoint, tgt.position, timer / halfTime);
        }

        // Finish
        transform.position = tgt.position;
        
        if (deployable != null) {
            deployable.transform.parent = null;
            deployable.deploy(poison);
        }

        Object.Destroy(gameObject);
    }
}
