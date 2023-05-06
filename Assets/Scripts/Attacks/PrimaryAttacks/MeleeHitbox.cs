using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeHitbox : IPrimaryAttack
{
    private HashSet<IUnitStatus> hit = new HashSet<IUnitStatus>();
    protected float curDamage = 0f;
    private bool running = false;


    // Main function to set up the melee hitbox
    //  Pre: dir is the direction of the melee attack
    //  Post: sets up primary attack to expire
    public override void setUp(Vector3 dir, float dmg, PoisonVial poi) {
        Debug.Assert(dmg >= 0f);

        if (!running) {
            running = true;
            curDamage = dmg;
        }
    }



    // Main function to damage unit
    protected virtual void damageTarget(IUnitStatus tgt) {
        tgt.damage(curDamage, false);
    }


    // On Collision, damage the target and put them within the hit list if they weren't hit before
    private void OnTriggerEnter(Collider collider) {
        IUnitStatus target = collider.GetComponent<IUnitStatus>();

        if (target != null && !hit.Contains(target)) {
            hit.Add(target);
            damageTarget(target);
        }
    }
}
