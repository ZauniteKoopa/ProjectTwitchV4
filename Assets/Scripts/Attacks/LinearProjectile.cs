using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class LinearProjectile : IPrimaryAttack
{
    [SerializeField]
    private float projectileSpeed = 30f;
    private float projectileDamage = 0f;

    private float timer = 0f;
    private const float TIMEOUT_DURATION = 10f;


    // Update is called once per frame
    void Update()
    {
        // Move projectile
        float distDelta = projectileSpeed * Time.deltaTime;
        transform.Translate(distDelta * Vector3.forward);

        // Check timeout
        timer += Time.deltaTime;
        if (timer >= TIMEOUT_DURATION) {
            Object.Destroy(gameObject);
        }
    }


    // Main function to set up the projectile
    //  Pre: dir is the direction the projectile will move towards, dmg > 0
    //  Post: sets up primary attack 
    public override void setUp(Vector3 dir, float dmg) {
        Debug.Assert(dmg >= 0f);

        transform.forward = dir.normalized;
        projectileDamage = dmg;
    }


    // Main collision handler
    private void OnTriggerEnter(Collider collider) {
        IUnitStatus target = collider.GetComponent<IUnitStatus>();
        if (target != null) {
            damageTarget(target);
        }

        Object.Destroy(gameObject);
    }


    // Main protected helper function to damage a target
    protected void damageTarget(IUnitStatus tgt) {
        Debug.Assert(tgt != null);

        Debug.Log("Damaging " + tgt.name + ": " + projectileDamage);
    }
}
