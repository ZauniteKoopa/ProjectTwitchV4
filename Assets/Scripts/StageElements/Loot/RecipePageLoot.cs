using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecipePageLoot : PrizeLoot
{
    [SerializeField]
    private RecipeSelectionScreen selectionScreenUI;


    // On awake, listen to event
    private void Awake() {
        selectionScreenUI.selectionEndEvent.AddListener(onSelectionSequenceEnd);
    }


    // Abstract function on what to do with the player if player collected
    //  Pre: player != null
    //  Post: returns a boolean that checks if the activation is successful (and thus the loot destroys itself)
    protected override bool activate(PlayerStatus player, TwitchInventory inv) {
        inv.startRecipeSelectionSequence(selectionScreenUI);
        return false;
    }


    // Main function to handle when the sequence ends
    private void onSelectionSequenceEnd() {
        destroyObj();
    }
}
