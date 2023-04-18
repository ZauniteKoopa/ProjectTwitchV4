using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// MAKE THIS EVENT DRIVEN IN THE FUTURE
public class PoisonedUnitSensor : MonoBehaviour
{
    private HashSet<EnemyStatus> inRange = new HashSet<EnemyStatus>();


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


    // Main event handler
    private void onEnemyDeath(EnemyStatus enemy) {
        inRange.Remove(enemy);
        enemy.deathEvent.RemoveListener(delegate { onEnemyDeath(enemy); });
    }


    // Main function to check if you can contaminate or not
    public bool poisonedUnitsNearby() {
        foreach(EnemyStatus tgt in inRange) {
            if (tgt.isPoisoned()) {
                return true;
            }
        }

        return false;
    }
}
