using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnDummyTriggerEffect : IStaticEffect
{
    // Test enemy dummy
    [SerializeField]
    private EnemyStatus enemyTriggerDummy;


    // Main function to activate static visual effect
    public override void executeEffect() {
        enemyTriggerDummy.spawnIn();
    }


    // Public event handler function for when it ends
    public void onAnimationTriggerEnd() {
        effectEndEvent.Invoke();
    }
}
