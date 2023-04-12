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
    private float initialDamageDuration = 0.1f;
    [SerializeField]
    [Min(0.1f)]
    private float initialDamage = 8f;
    [SerializeField]
    [Range(0.01f, 1f)]
    private float poisonFogSpeedReduction = 0.6f;
    private PoisonVial poison;
    private bool inInitialStage = true;

    // Hashsets for enemy management
    private HashSet<EnemyStatus> enemyHit = new HashSet<EnemyStatus>();
    private HashSet<EnemyStatus> inPoisonRange = new HashSet<EnemyStatus>();


    // Main abstract protected function to override that represents what this hitbox does in its lifespan
    //  Pre: none
    //  Post: hitbox will stay for a duration, doing whatever it wants. by the end of it, it should kill itself
    protected override IEnumerator lifespan(PoisonVial p) {
        poison = p;

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
            enemy.revertSpeedModifier(1f / poisonFogSpeedReduction);
        }

        Object.Destroy(gameObject);
    }


    // On trigger enter
    private void OnTriggerEnter(Collider collider) {
        EnemyStatus enemyTgt = collider.GetComponent<EnemyStatus>();

        if (enemyTgt != null) {
            // If initial stage, apply damage
            if (inInitialStage && !enemyHit.Contains(enemyTgt)) {
                enemyHit.Add(enemyTgt);
                enemyTgt.poisonDamage(initialDamage, false, poison, 1);
            }

            // add to enemies that are in range
            inPoisonRange.Add(enemyTgt);
            enemyTgt.applySpeedModifier(poisonFogSpeedReduction);
        }
    }


    // On trigger exit
    private void OnTriggerExit(Collider collider) {
        EnemyStatus enemyTgt = collider.GetComponent<EnemyStatus>();

        if (enemyTgt != null) {

            // add to enemies that are in range
            inPoisonRange.Remove(enemyTgt);
            enemyTgt.revertSpeedModifier(1f / poisonFogSpeedReduction);
        }
    }
}
