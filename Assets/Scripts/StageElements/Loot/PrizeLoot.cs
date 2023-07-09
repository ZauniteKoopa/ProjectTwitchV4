using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PrizeLoot : MonoBehaviour
{
    private bool collected = false;

    // On trigger enter, if a player enters it, collect it
    private void OnTriggerEnter(Collider collider) {
        PlayerStatus player = collider.GetComponent<PlayerStatus>();

        if (player != null && !collected) {
            collected = true;
            collect(player);
            Object.Destroy(gameObject);
        }
    }


    // Abstract function on what to do with the player if player collected
    //  Pre: player != null
    protected abstract void collect(PlayerStatus player);
}
