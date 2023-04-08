using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonFog : DeployableHitbox
{
    [SerializeField]
    private float fogDuration = 3.0f;

    // Main abstract protected function to override that represents what this hitbox does in its lifespan
    //  Pre: none
    //  Post: hitbox will stay for a duration, doing whatever it wants. by the end of it, it should kill itself
    protected override IEnumerator lifespan() {
        yield return new WaitForSeconds(fogDuration);
        Object.Destroy(gameObject);
    }
}
