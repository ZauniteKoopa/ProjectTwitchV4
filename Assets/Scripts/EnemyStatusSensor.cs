using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class EnemyStatusDelegate : UnityEvent<EnemyStatus> {}

public class EnemyStatusSensor : MonoBehaviour
{
    private Dictionary<EnemyStatus, UnityAction> inRangeEnemyDelegates = new Dictionary<EnemyStatus, UnityAction>();
    public EnemyStatusDelegate enemyEnterEvent;
    public EnemyStatusDelegate enemyExitEvent;


    // Main on trigger enter
    private void OnTriggerEnter(Collider collider) {
        EnemyStatus tgt = collider.GetComponent<EnemyStatus>();

        if (tgt != null && !inRangeEnemyDelegates.ContainsKey(tgt)) {
            UnityAction curDelegate;
            tgt.deathEvent.AddListener(curDelegate = delegate { onEnemyDeath(tgt); });
            inRangeEnemyDelegates.Add(tgt, curDelegate);

            enemyEnterEvent.Invoke(tgt);
        }
    }


    // Main on trigger exit
    private void OnTriggerExit(Collider collider) {
        EnemyStatus tgt = collider.GetComponent<EnemyStatus>();

        if (tgt != null && inRangeEnemyDelegates.ContainsKey(tgt)) {
            tgt.deathEvent.RemoveListener(inRangeEnemyDelegates[tgt]);
            inRangeEnemyDelegates.Remove(tgt);

            enemyExitEvent.Invoke(tgt);
        }
    }


    // Main event handler for enemy death
    private void onEnemyDeath(EnemyStatus enemy) {
        if (enemy != null && inRangeEnemyDelegates.ContainsKey(enemy)) {
            enemy.deathEvent.RemoveListener(inRangeEnemyDelegates[enemy]);
            inRangeEnemyDelegates.Remove(enemy);

            enemyExitEvent.Invoke(enemy);
        }
    }


    // Main function to check if enemy is found within range
    public bool isFoundWithinRange(EnemyStatus tgt) {
        return inRangeEnemyDelegates.ContainsKey(tgt);
    }
}
