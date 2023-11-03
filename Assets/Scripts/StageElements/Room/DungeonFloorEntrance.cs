using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using TMPro;

public class DungeonFloorEntrance : PrizeLoot
{
    [SerializeField]
    private DungeonFloor dungeonFloor;
    [SerializeField]
    private TMP_Text prizeDisplay;
    private EndReward projectedEndPrize;

    public UnityEvent playerEnterFloorEvent;


    // Abstract function on what to do with the player if player collected
    //  Pre: player != null
    //  Post: returns a boolean that checks if the activation is successful (and thus the loot destroys itself)
    protected override bool activate(PlayerStatus player, TwitchInventory inv) {
        playerEnterFloorEvent.Invoke();
        dungeonFloor.startDungeon(player, projectedEndPrize);

        return false;
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
