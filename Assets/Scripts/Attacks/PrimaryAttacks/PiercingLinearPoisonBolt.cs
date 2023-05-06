using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiercingLinearPoisonBolt : LinearPoisonBolt
{
    [SerializeField]
    [Range(0.01f, 1f)]
    private float minDamageRatio = 0.6f;
    [SerializeField]
    [Range(0.01f, 1f)]
    private float damageReductionPerEnemy = 0.1f;
    private float curDamageRatio = 1f;
    private float originalProjectileDamage = 0f;



    // Main function to set up the projectile
    //  Pre: dir is the direction the projectile will move towards, dmg > 0
    //  Post: sets up primary attack 
    public override void setUp(Vector3 dir, float dmg, PoisonVial poi) {
        Debug.Assert(dmg >= 0f);

        originalProjectileDamage = dmg;
        base.setUp(dir, dmg, poi);
    }
    
    
    // Main protected helper function to damage a target
    protected override void damageTarget(IUnitStatus tgt) {
        projectileDamage = originalProjectileDamage * curDamageRatio;
        base.damageTarget(tgt);
    }


    // Main protected helper function on when the projectile hits an enemy (HAPPENS AFTER DAMAGE TARGET)
    protected override void onHitEnemy() {
        curDamageRatio = Mathf.Max(curDamageRatio - damageReductionPerEnemy, minDamageRatio);
    }
}
