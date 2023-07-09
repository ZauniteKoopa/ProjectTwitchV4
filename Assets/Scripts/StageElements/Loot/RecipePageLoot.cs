using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecipePageLoot : PrizeLoot
{
    // Abstract function on what to do with the player if player collected
    //  Pre: player != null
    protected override void collect(PlayerStatus player) {
        TwitchInventory playerInv = player.transform.parent.GetComponent<TwitchInventory>();
        playerInv.addRandomRecipe();
    }
}
