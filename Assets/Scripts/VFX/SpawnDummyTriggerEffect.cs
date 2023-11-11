using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnDummyTriggerEffect : IStaticEffect
{
    // Test enemy dummy
    [SerializeField]
    private EnemyStatus enemyTriggerDummy;
    [SerializeField]
    private TopDownMovementController3D playerController;
    [SerializeField]
    private Transform playerStagingPoint;


    // Main function to activate static visual effect
    public override void executeEffect() {
        enemyTriggerDummy.spawnIn();
    }


    // Public event handler function for when it ends
    public void onAnimationTriggerEnd() {
        effectEndEvent.Invoke();
    }


    // Public event handler function for when Animatioon trigger has started: move player in appropriate position and disable controls
    public void onAnimationTriggerStart() {
        playerController.transform.position = playerStagingPoint.position;
    }
}
