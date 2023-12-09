using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleDeployable : DeployableHitbox
{
    private HashSet<IUnitStatus> hit = new HashSet<IUnitStatus>();
    private bool dealsInitialDamage = true;

    [SerializeField]
    [Min(0.01f)]
    private float hitboxDuration = 0.1f;
    [SerializeField]
    [Range(0f, 1f)]
    private float initialDamageDurationPercent = 1f;
    [SerializeField]
    [Min(0f)]
    private float initialDamage = 0f;


    // Main abstract protected function to override that represents what this hitbox does in its lifespan
    //  Pre: none
    //  Post: hitbox will stay for a duration, doing whatever it wants. by the end of it, it should kill itself
    protected override IEnumerator lifespan(PoisonVial poison) {
        yield return waitForInitialDamage();
        yield return new WaitForSeconds(hitboxDuration);

        destroyDeployable();
    }


    // Main protected helper function to wait for initial damage
    protected IEnumerator waitForInitialDamage() {
        if (initialDamageDurationPercent > 0.001f) {
            yield return new WaitForSeconds(hitboxDuration * initialDamageDurationPercent);
        }

        dealsInitialDamage = false;
    }


    // Main function to damage unit
    protected virtual void damageTarget(IUnitStatus tgt) {
        tgt.damage(initialDamage, false);
    }


    // Main function to handle trigger event
    protected virtual void onHitboxTriggered(IUnitStatus target) {}


    // On Collision, damage the target and put them within the hit list if they weren't hit before
    private void OnTriggerEnter(Collider collider) {
        IUnitStatus target = collider.GetComponent<IUnitStatus>();

        if (target != null) {
            if (dealsInitialDamage && !hit.Contains(target)) {
                hit.Add(target);
                damageTarget(target);
            }

            onHitboxTriggered(target);
        }
    }
}
