using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GastricLeakPuddle : PostContaminateHitbox
{
    [SerializeField]
    private float speedReductionFactor = 0.7f;
    
    // Main function to handle trigger event if they enter it
    protected override void onHitboxTriggered(IUnitStatus target) {
        EnemyStatus enemyTgt = target as EnemyStatus;

        if (enemyTgt != null) {
            enemyTgt.applySpeedModifier(speedReductionFactor);
        }

    }


    private void OnTriggerExit(Collider collider) {
        EnemyStatus tgt = collider.GetComponent<EnemyStatus>();

        if (tgt != null) {
            tgt.revertSpeedModifier(speedReductionFactor);
        }
    }
}
