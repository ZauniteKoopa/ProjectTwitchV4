using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;


// MAKE THIS EVENT DRIVEN IN THE FUTURE
public class PoisonedUnitSensor : MonoBehaviour
{
    private Dictionary<EnemyStatus, UnityAction> inRangeEnemyDelegates = new Dictionary<EnemyStatus, UnityAction>();


    // Main on trigger enter
    private void OnTriggerEnter(Collider collider) {
        EnemyStatus tgt = collider.GetComponent<EnemyStatus>();

        if (tgt != null && !inRangeEnemyDelegates.ContainsKey(tgt)) {
            UnityAction curDelegate;
            tgt.deathEvent.AddListener(curDelegate = delegate { onEnemyDeath(tgt); });
            inRangeEnemyDelegates.Add(tgt, curDelegate);
        }
    }


    // Main on trigger exit
    private void OnTriggerExit(Collider collider) {
        EnemyStatus tgt = collider.GetComponent<EnemyStatus>();

        if (tgt != null && inRangeEnemyDelegates.ContainsKey(tgt)) {
            tgt.deathEvent.RemoveListener(inRangeEnemyDelegates[tgt]);
            inRangeEnemyDelegates.Remove(tgt);
        }
    }


    // Main event handler
    private void onEnemyDeath(EnemyStatus enemy) {
        if (enemy != null && inRangeEnemyDelegates.ContainsKey(enemy)) {
            enemy.deathEvent.RemoveListener(inRangeEnemyDelegates[enemy]);
            inRangeEnemyDelegates.Remove(enemy);
        }
    }


    // Main function to check if you can contaminate or not
    public bool poisonedUnitsNearby() {
        var inRange = inRangeEnemyDelegates.Keys.ToArray();

        foreach(EnemyStatus tgt in inRange) {
            if (tgt.isPoisoned()) {
                return true;
            }
        }

        return false;
    }
}
