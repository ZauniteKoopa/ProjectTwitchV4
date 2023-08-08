using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearPoisonBolt : LinearProjectile
{
    private PoisonVial poison;
    [SerializeField]
    [Min(0)]
    private int appliedStacks = 1;

    private TrailRenderer trailRenderer;

    
    // Main function to set up the projectile
    //  Pre: dir is the direction the projectile will move towards, dmg > 0
    //  Post: sets up primary attack 
    public override void setUp(Vector3 dir, float dmg, PoisonVial poi, float range = -1f) {
        Debug.Assert(dmg >= 0f);

        base.setUp(dir, dmg, poi, range);
        poison = poi;

        trailRenderer = GetComponent<TrailRenderer>();
        if (trailRenderer == null) {
            Debug.LogWarning("No Trail Renderer found on this object");
        } else {
            trailRenderer.startColor = poi.getColor();
            trailRenderer.endColor = poi.getColor();
        }
    }


    // Main protected helper function to damage a target
    protected override void damageTarget(IUnitStatus tgt) {
        Debug.Assert(tgt != null);

        EnemyStatus enemyTarget = tgt as EnemyStatus;

        if (enemyTarget != null) {
            enemyTarget.poisonDamage(projectileDamage, false, poison, appliedStacks, isCrit: isBackstab(tgt));
        }
    }
}
