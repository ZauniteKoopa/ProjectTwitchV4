using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class LinearProjectile : IPrimaryAttack
{
    [SerializeField]
    private float projectileSpeed = 30f;
    protected float projectileDamage = 0f;

    private float distanceTimer = 0f;
    [SerializeField]
    [Min(0.2f)]
    private float maxDistance = 6f;


    // Update is called once per frame
    void Update()
    {
        // Move projectile
        float distDelta = projectileSpeed * Time.deltaTime;
        transform.Translate(distDelta * Vector3.forward);

        // Check timeout
        distanceTimer += distDelta;
        if (distanceTimer >= maxDistance) {
            Object.Destroy(gameObject);
        }
    }


    // Main function to set up the projectile
    //  Pre: dir is the direction the projectile will move towards, dmg > 0
    //  Post: sets up primary attack 
    public override void setUp(Vector3 dir, float dmg, PoisonVial poison, float range = -1f) {
        Debug.Assert(dmg >= 0f);

        transform.forward = dir.normalized;
        projectileDamage = dmg;
        if (range > 0f) {
            maxDistance = range;
        }
    }


    // Main collision handler
    private void OnTriggerEnter(Collider collider) {
        IUnitStatus target = collider.GetComponent<IUnitStatus>();
        if (target != null) {
            damageTarget(target);
            onHitEnemy();

        } else {
            Object.Destroy(gameObject);
            
        }
    }


    // Main protected helper function to damage a target
    protected virtual void damageTarget(IUnitStatus tgt) {
        Debug.Assert(tgt != null);

        tgt.damage(projectileDamage, false);
    }


    // Main protected helper function on when the projectile hits an enemy
    protected virtual void onHitEnemy() {
        Object.Destroy(gameObject);
    }
}