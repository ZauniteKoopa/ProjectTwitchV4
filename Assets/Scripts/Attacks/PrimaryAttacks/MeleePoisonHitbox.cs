using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleePoisonHitbox : MeleeHitbox
{
    private PoisonVial poison;
    [SerializeField]
    [Min(0)]
    private int appliedStacks = 1;

    
    // Main function to set up the projectile
    //  Pre: dir is the direction the projectile will move towards, dmg > 0
    //  Post: sets up primary attack 
    public override void setUp(Vector3 dir, float dmg, PoisonVial poi, float range = -1f) {
        Debug.Assert(dmg >= 0f);

        base.setUp(dir, dmg, poi, range);
        poison = poi;
    }


    // Main protected helper function to damage a target
    protected override void damageTarget(IUnitStatus tgt) {
        Debug.Assert(tgt != null);

        EnemyStatus enemyTarget = tgt as EnemyStatus;

        if (enemyTarget != null) {
            enemyTarget.poisonDamage(getBackstabDamage(tgt, curDamage), false, poison, appliedStacks);
        }
    }
}
