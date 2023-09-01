using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingLoot : PrizeLoot {
    [SerializeField]
    [Range(0.001f, 1f)]
    private float healPercentage;

    // Abstract function on what to do with the player if player collected
    //  Pre: player != null
    //  Post: returns a boolean that checks if the activation is successful (and thus the loot destroys itself)
    protected override bool activate(PlayerStatus player, TwitchInventory inv) {
        player.healPercentage(healPercentage);
        return true;
    }
}
