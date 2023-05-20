using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StimulatingStenchZone : PostContaminateHitbox
{
    [SerializeField]
    [Min(0.01f)]
    private float speedBoostFactor = 2f;
    [SerializeField]
    [Min(0.01f)]
    private float attackBuff = 1.25f;
    
    // Main function to handle trigger event if they enter it
    protected override void onHitboxTriggered(IUnitStatus target) {
        PlayerStatus playerTarget = target as PlayerStatus;

        if (playerTarget != null) {
            playerTarget.applySpeedModifier(speedBoostFactor);
            playerTarget.applyAttackModifier(attackBuff);
            Debug.Log("apply");
        }

    }


    private void OnTriggerExit(Collider collider) {
        PlayerStatus tgt = collider.GetComponent<PlayerStatus>();

        if (tgt != null) {
            tgt.revertSpeedModifier(speedBoostFactor);
            tgt.revertAttackModifier(attackBuff);
            Debug.Log("revert");
        }
    }
}
