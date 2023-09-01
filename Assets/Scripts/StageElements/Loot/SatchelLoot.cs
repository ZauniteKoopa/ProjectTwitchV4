using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SatchelLoot : PrizeLoot
{
    [SerializeField]
    [Range(1, 4)]
    private int ingredientSlotIncrease = 1;

    // Abstract function on what to do with the player if player collected
    //  Pre: player != null
    //  Post: returns a boolean that checks if the activation is successful (and thus the loot destroys itself)
    protected override bool activate(PlayerStatus player, TwitchInventory inv) {
        inv.addIngredientSlot(ingredientSlotIncrease);
        return true;
    }
}
