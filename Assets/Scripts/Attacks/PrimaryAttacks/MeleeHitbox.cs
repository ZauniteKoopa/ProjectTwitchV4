using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeHitbox : IPrimaryAttack
{
    private HashSet<IUnitStatus> hit = new HashSet<IUnitStatus>();
    protected float curDamage = 0f;
    private bool running = false;

    [SerializeField]
    [Range(0, 20)]
    private int hitStopFrames = 0;
    private bool firstHit = true;

    [SerializeField]
    [Range(0f, 1.5f)]
    private float cameraShakeMagnitude = 0f;


    // Main function to set up the melee hitbox
    //  Pre: dir is the direction of the melee attack
    //  Post: sets up primary attack to expire
    public override void setUp(Vector3 dir, float dmg, PoisonVial poi, float range = -1f) {
        Debug.Assert(dmg >= 0f);

        if (!running) {
            running = true;
            curDamage = dmg;

            if (range > 0f) {
                Transform parent = transform.parent;
                float xzRatio = parent.localScale.x / parent.localScale.z;
                parent.localScale = new Vector3(xzRatio * range, parent.localScale.y, range);
            }
        }
    }



    // Main function to damage unit
    protected virtual void damageTarget(IUnitStatus tgt) {
        tgt.damage(curDamage, false);
    }


    // On Collision, damage the target and put them within the hit list if they weren't hit before
    private void OnTriggerEnter(Collider collider) {
        IUnitStatus target = collider.GetComponent<IUnitStatus>();

        // If target is valid and has not been hit yet, deal damage to target
        if (target != null && !hit.Contains(target)) {
            hit.Add(target);
            damageTarget(target);

            // If this was the first hit enemy, trigger hit stop
            if (firstHit) {
                firstHit = false;
                PlayerCameraController.hitStop(hitStopFrames);
                PlayerCameraController.shakeCamera(hitStopFrames, cameraShakeMagnitude);
            }
        }
    }
}
