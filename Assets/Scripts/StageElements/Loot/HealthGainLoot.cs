using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthGainLoot : PrizeLoot
{ 
    [SerializeField]
    [Min(1f)]
    private float healthGain = 5f;

    // Abstract function on what to do with the player if player collected
    //  Pre: player != null
    //  Post: returns a boolean that checks if the activation is successful (and thus the loot destroys itself)
    protected override bool activate(PlayerStatus player, TwitchInventory inv) {
        player.gainHealth(healthGain);
        return true;
    }
}
