using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGoal : PrizeLoot
{
    [SerializeField]
    private string destinationScene;

    // Abstract function on what to do with the player if player collected
    //  Pre: player != null
    //  Post: returns a boolean that checks if the activation is successful (and thus the loot destroys itself)
    protected override bool activate(PlayerStatus player, TwitchInventory inv) {
        SceneManager.LoadScene(destinationScene);
        return false;
    }
}
