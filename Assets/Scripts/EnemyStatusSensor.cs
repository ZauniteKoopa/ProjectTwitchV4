using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatusSensor : MonoBehaviour
{
    protected HashSet<EnemyStatus> inRange = new HashSet<EnemyStatus>();


    // Main on trigger enter
    private void OnTriggerEnter(Collider collider) {
        EnemyStatus tgt = collider.GetComponent<EnemyStatus>();

        if (tgt != null) {
            inRange.Add(tgt);
            tgt.deathEvent.AddListener(delegate { onEnemyDeath(tgt); });
        }
    }


    // Main on trigger exit
    private void OnTriggerExit(Collider collider) {
        EnemyStatus tgt = collider.GetComponent<EnemyStatus>();

        if (tgt != null) {
            inRange.Remove(tgt);
            tgt.deathEvent.RemoveListener(delegate { onEnemyDeath(tgt); });
        }
    }


    // Main event handler for enemy death
    private void onEnemyDeath(EnemyStatus enemy) {
        inRange.Remove(enemy);
        enemy.deathEvent.RemoveListener(delegate { onEnemyDeath(enemy); });
    }


    // Main function to check if enemy is found within range
    public bool isFoundWithinRange(EnemyStatus tgt) {
        return inRange.Contains(tgt);
    }
}
