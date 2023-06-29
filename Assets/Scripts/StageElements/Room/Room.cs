using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Room : MonoBehaviour
{
    [Header("Room Events")]
    public UnityEvent playerEnterRoomEvent;
    public UnityEvent enemyRoomEvent;

    private int enemiesInRoom = 0;
    private bool visitedByPlayer = false;
    public bool playerInside = false;

    // What doors are open in this room (assuming 0 rotation)
    [Header("Room Openings")]
    public bool westOpen;
    public bool eastOpen;
    public bool northOpen;
    public bool southOpen;


    // Main event handler for on trigger enter
    private void OnTriggerEnter(Collider collider) {
        PlayerStatus player = collider.GetComponent<PlayerStatus>();
        EnemyStatus enemy = collider.GetComponent<EnemyStatus>();

        // Case in which a player enters
        if (player != null) {
            onPlayerEnter(player);

        // Case in which an enemy enters
        } else if (enemy != null) {
            onEnemyEnter(enemy);
        }
    }


    // Main event handler for on trigger enter
    private void OnTriggerExit(Collider collider) {
        EnemyStatus enemy = collider.GetComponent<EnemyStatus>();

        if (enemy != null) {
            onEnemyExit(enemy);
        }
    }


    // Protected function for when player enters a room
    protected virtual void onPlayerEnter(PlayerStatus player) {
        // Mark this as visited by the player if not visited yet
        if (!visitedByPlayer) {
            visitedByPlayer = true;
        }

        // Invoke event. THE PARENT SHOULD HANDLE THE playerInside boolean
        playerEnterRoomEvent.Invoke();
    }


    // Protected function for when an enemy enters a room
    protected virtual void onEnemyEnter(EnemyStatus enemy) {
        enemiesInRoom++;
        enemy.deathEvent.AddListener(delegate { onEnemyDeath(enemy); });
    }


    // Event handler function for when enemy exits a room
    protected virtual void onEnemyExit(EnemyStatus enemy) {
        enemiesInRoom--;
        enemy.deathEvent.RemoveListener(delegate { onEnemyDeath(enemy); });
    }


    // Event handler function for when an enemy dies within the room
    protected virtual void onEnemyDeath(EnemyStatus enemy) {
        enemiesInRoom--;
        enemy.deathEvent.RemoveListener(delegate { onEnemyDeath(enemy); });
    }


    // Main function to access how many enemies are in this room
    public int getNumEnemiesInside() {
        return enemiesInRoom;
    }


    // Main function to see if this room is revealed on map
    public bool revealedOnMap() {
        return visitedByPlayer;
    }
}
