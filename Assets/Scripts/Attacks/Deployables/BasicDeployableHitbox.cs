using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicDeployableHitbox : DeployableHitbox
{
    [SerializeField]
    [Min(1)]
    private int numActiveFrames = 8;
    [SerializeField]
    private float hitboxDamage = 6f;
    [SerializeField]
    [Min(0)]
    private int addedStacks = 2;
    [SerializeField]
    [Range(0f, 1.5f)]
    private float cameraShakeMagnitude = 0f;
    [SerializeField]
    [Range(0, 20)]
    private int shakeFrames = 0;

    private HashSet<EnemyStatus> hit = new HashSet<EnemyStatus>();
    private PoisonVial curPoison;



    // Main abstract protected function to override that represents what this hitbox does in its lifespan
    //  Pre: none
    //  Post: hitbox will stay for a duration, doing whatever it wants. by the end of it, it should kill itself
    protected override IEnumerator lifespan(PoisonVial poison) {
        curPoison = poison;
        PlayerCameraController.shakeCamera(shakeFrames, cameraShakeMagnitude);

        for (int f = 0; f < numActiveFrames; f++) {
            yield return 0;
        }

        GetComponent<Collider>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;
        yield return new WaitForSeconds(1.0f);
    }


    // Main function to handle onTriggerEnter
    private void OnTriggerEnter(Collider collider) {
        EnemyStatus curTarget = collider.GetComponent<EnemyStatus>();

        if (curTarget != null && !hit.Contains(curTarget)) {
            hit.Add(curTarget);
            curTarget.poisonDamage(hitboxDamage, false, curPoison, addedStacks);
        }
    }

}
