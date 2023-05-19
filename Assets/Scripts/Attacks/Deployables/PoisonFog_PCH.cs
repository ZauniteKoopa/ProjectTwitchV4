using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonFog_PCH : PostContaminateHitbox
{
    // Base stats
    [SerializeField]
    [Min(0.1f)]
    private float fogTickDuration = 1.0f;

    // Hashsets for enemy management
    private HashSet<EnemyStatus> inPoisonRange = new HashSet<EnemyStatus>();


    // Main abstract protected function to override that represents what this hitbox does in its lifespan
    //  Pre: none
    //  Post: hitbox will stay for a duration, doing whatever it wants. by the end of it, it should kill itself
    protected override IEnumerator lifespan(PoisonVial p) {
        StartCoroutine(infectionLoop());
        yield return base.lifespan(p);
    }


    // Main function to run poison loop
    private IEnumerator infectionLoop() {
        while (true) {
            yield return new WaitForSeconds(fogTickDuration);

            foreach (EnemyStatus enemy in inPoisonRange) {
                enemy.poisonDamage(0f, false, curPoison, 1, false);
            }
        }
    }


    // On trigger enter
    protected override void onHitboxTriggered(IUnitStatus target) {
        EnemyStatus enemyTgt = target as EnemyStatus;

        if (enemyTgt != null) {
            // add to enemies that are in range
            inPoisonRange.Add(enemyTgt);
        }
    }


    // On trigger exit
    private void OnTriggerExit(Collider collider) {
        EnemyStatus enemyTgt = collider.GetComponent<EnemyStatus>();

        if (enemyTgt != null) {

            // add to enemies that are in range
            inPoisonRange.Remove(enemyTgt);
        }
    }
}
