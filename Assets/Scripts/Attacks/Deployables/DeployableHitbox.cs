using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DeployableHitbox : MonoBehaviour
{
    public Transform source;


    // Main function to deploy the hitbox at its currrent location
    public void deploy(PoisonVial poison) {
        gameObject.SetActive(true);
        StartCoroutine(lifespan(poison));
    }

    // Main abstract protected function to override that represents what this hitbox does in its lifespan
    //  Pre: none
    //  Post: hitbox will stay for a duration, doing whatever it wants. by the end of it, it should kill itself
    protected abstract IEnumerator lifespan(PoisonVial poison);
}
