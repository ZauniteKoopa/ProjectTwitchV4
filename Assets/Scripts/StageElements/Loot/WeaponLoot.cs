using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponLoot : PrizeLoot
{
    // Abstract function on what to do with the player if player collected
    //  Pre: player != null
    //  Post: returns a boolean that checks if the activation is successful (and thus the loot destroys itself)
    protected override bool activate(PlayerStatus player, TwitchInventory inv) {
        inv.GetComponent<TwitchAttackModule>().obtainWeapon();
        inv.refreshAmbush();
        return true;
    }
}
