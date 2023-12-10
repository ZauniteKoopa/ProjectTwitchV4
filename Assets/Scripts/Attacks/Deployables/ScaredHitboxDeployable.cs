using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaredHitboxDeployable : SimpleDeployable {
    [SerializeField]
    [Min(0.01f)]
    private float stunDuration = 1f;
    private HashSet<IUnitStatus> hit = new HashSet<IUnitStatus>();
    
    // Main function to handle trigger event if they enter it
    protected override void onHitboxTriggered(IUnitStatus target) {
        EnemyStatus enemyTgt = target as EnemyStatus;

        if (enemyTgt != null && !hit.Contains(target)) {
            Vector3 rawDirVector = enemyTgt.transform.position - transform.position;
            enemyTgt.transform.forward = Vector3.ProjectOnPlane(rawDirVector, Vector3.up).normalized;

            enemyTgt.setTimedStunModifier(stunDuration);
            hit.Add(target);
        }

    }
}
