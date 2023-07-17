using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using TMPro;

public class DungeonFloorEntrance : MonoBehaviour
{
    [SerializeField]
    private DungeonFloor dungeonFloor;
    [SerializeField]
    private TMP_Text prizeDisplay;
    [SerializeField]
    private float entranceHeal = 5f;
    private PlayerStatus playerTgt;
    private EndReward projectedEndPrize;

    public UnityEvent playerEnterFloorEvent;


    // On trigger enter, if player enters, activate the dungeon floor with that player
    private void OnTriggerEnter(Collider collider) {
        PlayerStatus player = collider.GetComponent<PlayerStatus>();

        if (player != null) {
            playerTgt = player;
        }
    }


    // On trigger enter, if player enters, activate the dungeon floor with that player
    private void OnTriggerExit(Collider collider) {
        PlayerStatus player = collider.GetComponent<PlayerStatus>();

        if (player != null) {
            playerTgt = null;
        }
    }


    // Main input handler for activating dungeon
    public void onInteractPress(InputAction.CallbackContext context) {
        if (context.started && playerTgt != null) {
            playerEnterFloorEvent.Invoke();
            playerTgt.heal(entranceHeal);
            dungeonFloor.startDungeon(playerTgt, projectedEndPrize);
        }
    }


    // Main function to set the dungeon that's connected to this entrance
    public void setDungeonFloor(DungeonFloor floor) {
        dungeonFloor = floor;
    }


    // Main function to set up the entrance with the specific EndPrize
    public void setProjectedEndPrize(EndReward endPrize) {
        projectedEndPrize = endPrize;
        prizeDisplay.text = endPrize.rewardName;
    }

}
