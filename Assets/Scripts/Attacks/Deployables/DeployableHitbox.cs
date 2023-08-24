using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class DeployableHitbox : MonoBehaviour
{
    public Transform source;
    public UnityEvent deployableDestroyedEvent;
    private bool destroyed = false;


    // Main function to deploy the hitbox at its currrent location
    public void deploy(PoisonVial poison) {
        gameObject.SetActive(true);
        StartCoroutine(lifespan(poison));
    }


    // Main event handler for when the deployable is destroyed
    public void OnDestroy() {
        deployableDestroyedEvent.Invoke();
    }

    // Main function to destroy deployable
    public void destroyDeployable() {
        if (!destroyed) {
            destroyed = true;
            StartCoroutine(destroyDeployableSequence());
        }
    }

    // Main abstract protected function to override that represents what this hitbox does in its lifespan
    //  Pre: none
    //  Post: hitbox will stay for a duration, doing whatever it wants. by the end of it, it should kill itself
    protected abstract IEnumerator lifespan(PoisonVial poison);


    // Main private IEnumerator to destroy deployables
    private IEnumerator destroyDeployableSequence() {
        destroyed = true;
        transform.Translate(Vector3.down * 5000f);
        yield return new WaitForSeconds(0.01f);
        Object.Destroy(gameObject);
    }
}
