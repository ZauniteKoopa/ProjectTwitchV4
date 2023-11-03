using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonFog : DeployableHitbox
{
    // Base stats
    [SerializeField]
    [Min(0.1f)]
    private float fogTickDuration = 1.0f;
    [SerializeField]
    [Min(1)]
    private int maxFogTicks = 3;
    [SerializeField]
    [Min(0.01f)]
    private float initialDamageDuration = 0.1f;
    [SerializeField]
    [Min(0.1f)]
    private float initialDamage = 8f;
    [SerializeField]
    [Min(0)]
    private int initialNumStacks = 1;
    [SerializeField]
    [Range(0.01f, 1f)]
    private float poisonFogSpeedReduction = 0.6f;
    [SerializeField]
    [Range(0.01f, 1f)]
    private float poisonFogStickySpeedReduction = 0.4f;
    [SerializeField]
    private ResourceBar optionalPoisonFogDurationBar = null;
    private PoisonVial poison;
    private bool inInitialStage = true;
    private float curFogSpeedModifier = 1f;

    // Hashsets for enemy management
    private HashSet<EnemyStatus> enemyHit = new HashSet<EnemyStatus>();
    private HashSet<EnemyStatus> inPoisonRange = new HashSet<EnemyStatus>();


    // Main abstract protected function to override that represents what this hitbox does in its lifespan
    //  Pre: none
    //  Post: hitbox will stay for a duration, doing whatever it wants. by the end of it, it should kill itself
    protected override IEnumerator lifespan(PoisonVial p) {
        poison = p;
        curFogSpeedModifier = getFogSpeedModifier();
        GetComponent<MeshRenderer>().material.color = p.getColor();

        if (optionalPoisonFogDurationBar != null) {
            StartCoroutine(displayDuration());
        }

        // Initial damage
        yield return new WaitForSeconds(initialDamageDuration);
        inInitialStage = false;

        // Fog
        for (int t = 0; t < maxFogTicks; t++) {
            yield return new WaitForSeconds(fogTickDuration);

            foreach (EnemyStatus enemy in inPoisonRange) {
                enemy.poisonDamage(0f, false, poison, 1, false);
            }
        }

        // Cleanup
        foreach (EnemyStatus enemy in inPoisonRange) {
            enemy.revertSpeedModifier(curFogSpeedModifier);
        }

        destroyDeployable();
    }


    // Main function to handle the resource bar, if it exists
    private IEnumerator displayDuration() {
        Debug.Assert(optionalPoisonFogDurationBar != null);

        float timer = 0f;
        float maxDuration = initialDamageDuration + (fogTickDuration * maxFogTicks);
        optionalPoisonFogDurationBar.setFill(1f, 1f);

        while (timer < maxDuration) {
            yield return 0;

            timer += Time.deltaTime;
            optionalPoisonFogDurationBar.setFill(maxDuration - timer, maxDuration);
        }
    }


    // On trigger enter
    private void OnTriggerEnter(Collider collider) {
        EnemyStatus enemyTgt = collider.GetComponent<EnemyStatus>();

        if (enemyTgt != null) {
            // If initial stage, apply damage
            if (inInitialStage && !enemyHit.Contains(enemyTgt)) {
                enemyHit.Add(enemyTgt);
                enemyTgt.poisonDamage(initialDamage, false, poison, initialNumStacks);
            }

            // add to enemies that are in range
            inPoisonRange.Add(enemyTgt);
            enemyTgt.applySpeedModifier(curFogSpeedModifier);
        }
    }


    // On trigger exit
    private void OnTriggerExit(Collider collider) {
        EnemyStatus enemyTgt = collider.GetComponent<EnemyStatus>();

        if (enemyTgt != null) {

            // add to enemies that are in range
            inPoisonRange.Remove(enemyTgt);
            enemyTgt.revertSpeedModifier(curFogSpeedModifier);
        }
    }


    // Main function to get the speed modifier
    private float getFogSpeedModifier() {
        return (poison != null && poison.sideEffect.getType() == PoisonVialStat.STICKINESS) ? poisonFogStickySpeedReduction : poisonFogSpeedReduction;
    }
}
