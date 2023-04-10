using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class LobAction : MonoBehaviour
{
    [SerializeField]
    private DeployableHitbox deployable;
    private const float MIN_START_TIME = 0.25f;
    private const float MAX_LOB_HEIGHT = 3f;
    

    // Main function to lob the projectile from src to tgt
    //  Pre: The vector3s are positions within the game world, lobSpeed is the speed of the lobbing projectile
    //  Post: lobs the projectile
    public void lob(Vector3 src, Vector3 tgt, float lobSpeed, PoisonVial poison) {
        Debug.Assert(lobSpeed > 0f);

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
        
        if (deployable != null && poison != null) {
            deployable.transform.parent = null;
            deployable.deploy(poison);
        }

        Object.Destroy(gameObject);
    }
}
