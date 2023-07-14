using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnRoom : Room
{
    [SerializeField]
    private Transform spawnPoint;


    // On awake, disable spawn point
    private void Awake() {
        spawnPoint.gameObject.SetActive(false);
    }


    // Main function to spawn in player at position
    //  Pre: spawnedInPlayer
    //  Post: player spawns in the map
    public void spawnInPlayer(PlayerStatus player) {
        player.transform.parent.position = spawnPoint.position;
    }
}
