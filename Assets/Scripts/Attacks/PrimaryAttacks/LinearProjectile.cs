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
            if (firstHit) {
                onProjectileCollision();
            }
            
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

        if (collider.gameObject.layer == LayerMask.NameToLayer("WallCollision") || collider.gameObject.layer == LayerMask.NameToLayer("PlayerProtection")){
            if (firstHit) {
                onProjectileCollision();
            }
            
            Object.Destroy(gameObject);
        }
    }


    // Main function to on Target hit
    public void onTargetHit(IUnitStatus target) {
        damageTarget(target);

        // If this was the first hit enemy, trigger hit stop
        if (firstHit) {
            onProjectileCollision();
            firstHit = false;
            PlayerCameraController.hitStop(hitStopFrames);
            PlayerCameraController.shakeCamera(hitStopFrames, cameraShakeMagnitude);
        }

        onHitEnemy();
    }



    // Main protected helper function to damage a target
    protected virtual void damageTarget(IUnitStatus tgt) {
        Debug.Assert(tgt != null);

        tgt.damage(projectileDamage, false, isCrit: isBackstab(tgt));
    }


    // Main protected helper function on when the projectile hits an enemy
    protected virtual void onHitEnemy() {
        Object.Destroy(gameObject);
    }

    
    // Main protected helper function to get modified backstab damage if backstab applies
    protected bool isBackstab(IUnitStatus tgt) {
        return (canBackstab && Vector3.Angle(transform.forward, tgt.transform.forward) <= backstabAngleThreshold);
    }


    // Main protected helper function for when you actually hit something
    protected virtual void onProjectileCollision() {}
}
