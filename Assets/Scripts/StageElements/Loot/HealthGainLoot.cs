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
    protected override void collect(PlayerStatus player) {
        player.gainHealth(healthGain);
    }
}
