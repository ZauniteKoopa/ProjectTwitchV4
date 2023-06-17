using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleDeployable : DeployableHitbox
{
    // Main abstract protected function to override that represents what this hitbox does in its lifespan
    //  Pre: none
    //  Post: hitbox will stay for a duration, doing whatever it wants. by the end of it, it should kill itself
    protected override IEnumerator lifespan(PoisonVial poison) {
        yield return 0;
    }
}
