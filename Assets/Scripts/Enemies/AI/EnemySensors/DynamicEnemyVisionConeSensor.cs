using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicEnemyVisionConeSensor : EnemyVisionSensor
{
    [SerializeField]
    private FieldOfVision fieldOfVision = null;


    // On awake, error check and get body
    protected override void initialize() {
        base.initialize();

        if (fieldOfVision == null) {
            Debug.LogError("Field of vision is not connected to this sensor", transform);
        }

        enemyStatus.enemyNoticesDamageEvent.AddListener(onAttackedByPlayer);
    }


    // Main function to manage passive sensing each frame
    protected override void managePassiveSensing() {
        PlayerStatus seenPlayer = fieldOfVision.getSeenPlayer();

        if (seenPlayer != null && seenPlayer.canSeePlayer(enemyStatus)) {
            brain.onSensedPlayer(seenPlayer.transform);
            fieldOfVision.showVision(false);
        }
    }


    // Main action of actually forgetting the player in terms of sensing
    protected override void forgetPlayer() {
        base.forgetPlayer();
        fieldOfVision.showVision(true);
    }


    // Main event handler function for when this enemy is attacked by the player: look at the player if this happens
    private void onAttackedByPlayer() {
        if (nearbyTarget != null) {
            brain.lookAt(nearbyTarget.transform.position - transform.position);
        }
    }
}
