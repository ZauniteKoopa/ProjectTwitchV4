using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

public class Room : MonoBehaviour
{
    [Header("Room Events")]
    public UnityEvent playerEnterRoomEvent;
    public UnityEvent enemyRoomEvent;
    public UnityEvent enemyDeathEvent;

    private int enemiesInRoom = 0;
    private bool visitedByPlayer = false;
    public bool playerInside = false;
    private Dictionary<EnemyStatus, UnityAction> enemyDeathDelegates = new Dictionary<EnemyStatus, UnityAction>();

    // What doors are open in this room (assuming 0 rotation)
    [Header("Room Openings")]
    public bool westOpen;
    public bool eastOpen;
    public bool northOpen;
    public bool southOpen;

    
    // Map coordinates
    [Header("Map coordinates")]
    [Range(0, 5)]
    public int mapRow = 0;
    [Range(0, 5)]
    public int mapCol = 0;


    private static readonly float ROOM_SIZE = 15f;
    private static readonly float WALL_OFFSET = 1.5f;


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
        if (!enemyDeathDelegates.ContainsKey(enemy)) {
            UnityAction curDelegate;

            enemiesInRoom++;
            enemy.deathEvent.AddListener(curDelegate = delegate { onEnemyDeath(enemy); });
            enemyRoomEvent.Invoke();

            enemyDeathDelegates.Add(enemy, curDelegate);
        }
    }


    // Event handler function for when enemy exits a room
    protected virtual void onEnemyExit(EnemyStatus enemy) {
        if (enemyDeathDelegates.ContainsKey(enemy)) {
            enemiesInRoom--;
            enemy.deathEvent.RemoveListener(enemyDeathDelegates[enemy]);
            enemyDeathDelegates.Remove(enemy);
            enemyRoomEvent.Invoke();
        }
    }


    // Event handler function for when an enemy dies within the room
    protected virtual void onEnemyDeath(EnemyStatus enemy) {

        if (enemyDeathDelegates.ContainsKey(enemy)) {
            enemiesInRoom--;
            enemy.deathEvent.RemoveListener(enemyDeathDelegates[enemy]);
            enemyDeathDelegates.Remove(enemy);
            enemyRoomEvent.Invoke();
        }
    }


    // Main function to access how many enemies are in this room
    public int getNumEnemiesInside() {
        return enemiesInRoom;
    }


    // Main function to see if this room is revealed on map
    public bool revealedOnMap() {
        return visitedByPlayer;
    }


    // Main function to spawn an enemy inside this room (ASSUMES A SQUARE ROOM)
    public EnemyStatus spawnEnemy(EnemyStatus enemyTemplate, LootTable lootTable) {
        // Get spawn position
        float emptySpaceLength = ROOM_SIZE - WALL_OFFSET;
        Vector3 spawnPos = new Vector3(Random.Range(-emptySpaceLength / 2f, emptySpaceLength / 2f), 0f, Random.Range(-emptySpaceLength / 2f, emptySpaceLength / 2f));
        spawnPos += transform.position;

        // Get nav mesh adjusted point 
        NavMeshHit hitInfo;
        NavMesh.SamplePosition(spawnPos, out hitInfo, 10f, NavMesh.AllAreas);
        spawnPos = hitInfo.position;

        // Spawn in position 
        EnemyStatus curEnemy = Object.Instantiate(enemyTemplate, spawnPos, Quaternion.identity);
        curEnemy.lootTable = lootTable;
        curEnemy.spawnIn();

        return curEnemy;
    }
}
