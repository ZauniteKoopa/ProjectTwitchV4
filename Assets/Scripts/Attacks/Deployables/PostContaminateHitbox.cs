using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PostContaminateHitbox : DeployableHitbox
{
    private HashSet<IUnitStatus> hit = new HashSet<IUnitStatus>();
    private float initialDamage = 0f;
    protected PoisonVial curPoison;
    private bool hasKilled = false;
    private bool dealsInitialDamage = true;

    [SerializeField]
    [Min(0.01f)]
    private float hitboxDuration = 0.1f;
    [SerializeField]
    [Range(0f, 1f)]
    private float initialDamageDurationPercent = 1f;
    [SerializeField]
    [Range(0.01f, 1f)]
    private float scaleIncreaseOnDeath = 0.5f;
    [SerializeField]
    private float durationIncreaseOnDeath = 0f;
    [SerializeField]
    [Range(0.01f, 1f)]
    private float contaminateDamageRatio = 0.25f;

    private float curDuration = 0f;


    // Main function to set up post contaminate hitbox
    public virtual void setUp(float contaminateDamage, PoisonVial poison, bool wasKilled) {
        // If killed make it bigger
        if (wasKilled) {
            transform.localScale *= (1f + scaleIncreaseOnDeath);
        }

        dealsInitialDamage = (initialDamageDurationPercent > 0.001f);
        initialDamage = contaminateDamageRatio * contaminateDamage;
        curDuration = (wasKilled) ? hitboxDuration + durationIncreaseOnDeath : hitboxDuration;
        curPoison = poison;
        deploy(poison);
    }
    
    
    // Main abstract protected function to override that represents what this hitbox does in its lifespan
    //  Pre: none
    //  Post: hitbox will stay for a duration, doing whatever it wants. by the end of it, it should kill itself
    protected override IEnumerator lifespan(PoisonVial poison) {
        yield return waitForInitialDamage();
        yield return new WaitForSeconds(getLingeringDuration());
        destroyDeployable();
    }



    // Main protected helper function to wait for initial damage
    protected IEnumerator waitForInitialDamage() {
        if (initialDamageDurationPercent > 0.001f) {
            yield return new WaitForSeconds(curDuration * initialDamageDurationPercent);
        }

        dealsInitialDamage = false;
    }


    // Main function to get the passive duration without the initial damage
    protected float getLingeringDuration() {
        return curDuration * (1f - initialDamageDurationPercent);
    }



    // Main function to damage unit
    protected virtual void damageTarget(IUnitStatus tgt) {
        if (tgt.damage(initialDamage, false) && !hasKilled) {
            hasKilled = true;
            curPoison.contaminateExecuteEvent.Invoke();
        }
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
