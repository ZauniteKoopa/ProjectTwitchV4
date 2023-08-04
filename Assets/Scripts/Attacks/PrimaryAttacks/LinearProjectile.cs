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

    [SerializeField]
    [Range(0, 20)]
    private int hitStopFrames = 0;
    private bool firstHit = true;

    [SerializeField]
    [Range(0f, 1.5f)]
    private float cameraShakeMagnitude = 0f;

    [Header("Backstab Damage")]
    [SerializeField]
    private bool canBackstab = false;
    [SerializeField]
    [Min(1f)]
    private float backStabMultipier = 2f;
    [SerializeField]
    [Range(0f, 90f)]
    private float backstabAngleThreshold = 50f;

    private const float DAMAGE_RANGE_BONUS = 1.25f;


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
            maxDistance = range + DAMAGE_RANGE_BONUS;
        }
    }


    // Main collision handler
    private void OnTriggerEnter(Collider collider) {
        IUnitStatus target = collider.GetComponent<IUnitStatus>();
        if (target != null) {
            damageTarget(target);

            // If this was the first hit enemy, trigger hit stop
            if (firstHit) {
                firstHit = false;
                PlayerCameraController.hitStop(hitStopFrames);
                PlayerCameraController.shakeCamera(hitStopFrames, cameraShakeMagnitude);
            }
            onHitEnemy();

        } else {
            Object.Destroy(gameObject);
            
        }
    }


    // Main protected helper function to damage a target
    protected virtual void damageTarget(IUnitStatus tgt) {
        Debug.Assert(tgt != null);

        tgt.damage(getBackstabDamage(tgt, projectileDamage), false);
    }


    // Main protected helper function on when the projectile hits an enemy
    protected virtual void onHitEnemy() {
        Object.Destroy(gameObject);
    }

    
    // Main protected helper function to get modified backstab damage if backstab applies
    protected float getBackstabDamage(IUnitStatus tgt, float damage) {
        if (canBackstab && Vector3.Angle(transform.forward, tgt.transform.forward) <= backstabAngleThreshold) {
            damage *= backStabMultipier;
        }

        return damage;
    }
}
