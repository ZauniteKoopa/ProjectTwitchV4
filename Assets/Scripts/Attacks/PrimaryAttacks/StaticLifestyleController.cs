using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticLifestyleController : IPrimaryAttack
{
    [SerializeField]
    private IPrimaryAttack mainAttack;
    [SerializeField]
    [Min(0.01f)]
    private float lifecycleDuration = 0.2f;
    private bool running = false;

    // Main function to set up the melee hitbox
    //  Pre: dir is the direction of the melee attack
    //  Post: sets up primary attack to expire
    public override void setUp(Vector3 dir, float dmg, PoisonVial poi) {
        Debug.Assert(dmg >= 0f);

        if (!running) {
            running = true;

            transform.forward = dir;
            mainAttack.setUp(dir, dmg, poi);
            StartCoroutine(lifecycle());
        }
    }
    
    
    // Main lifecycle
    private IEnumerator lifecycle() {
        yield return new WaitForSeconds(lifecycleDuration);
        Object.Destroy(gameObject);
    }
}
