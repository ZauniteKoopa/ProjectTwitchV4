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
    protected bool visitedByPlayer = false;
    public bool playerInside = false;
    private Dictionary<EnemyStatus, UnityAction> enemyDeathDelegates = new Dictionary<EnemyStatus, UnityAction>();

    // What doors are open in this room (assuming 0 rotation)
    [Header("Room Openings")]
    public bool westOpen;
    public bool eastOpen;
    public bool northOpen;
    public bool southOpen;
    public bool adjacentToBattleRoom = false;

    
    // Map coordinates
    [Header("Map coordinates")]
    [Range(0, 5)]
    public int mapRow = 0;
    [Range(0, 5)]
    public int mapCol = 0;

    [Header("Camera and Spawn Parameters")]
    [SerializeField]
    public float roomWidth = 18f;
    [SerializeField]
    public float roomLength = 18f;
    [Min(0f)]
    public float cameraExpandUp = 0f;
    [Min(0f)]
    public float cameraExpandDown = 0f;
    [Min(0f)]
    public float cameraExpandLeft = 0f;
    [Min(0f)]
    public float cameraExpandRight = 0f;
    public static readonly float WALL_OFFSET = 2.5f;


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
        // Invoke event. THE PARENT SHOULD HANDLE THE playerInside boolean
        if (!playerInside) {
            PlayerCameraController.startCameraRoomSequence(this, hasCameraTransition());
        }
        
        // Mark this as visited by the player if not visited yet
        if (!visitedByPlayer) {
            visitedByPlayer = true;
        }

        playerEnterRoomEvent.Invoke();
    }


    // Main function to check if you have a camera transition
    protected virtual bool hasCameraTransition() {
        return true;
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
    public virtual EnemyStatus spawnEnemy(EnemyStatus enemyTemplate, LootTable lootTable, DungeonFloorLayout dungeonNav, bool willDropLoot) {
        // Set up for spawn point creation
        float emptySpaceLength = roomLength - WALL_OFFSET;
        float emptySpaceWidth = roomWidth - WALL_OFFSET;
        Vector3 actualSpawnPos = transform.position;

        // keep trying to find a point until you found a valid one
        do {
            // Get the projected point
            Vector3 projectedSpawnPos = new Vector3(Random.Range(-emptySpaceWidth / 2f, emptySpaceWidth / 2f), 0f, Random.Range(-emptySpaceLength / 2f, emptySpaceLength / 2f));
            projectedSpawnPos += transform.position;

            // Adjust via navmesh
            NavMeshHit hitInfo;
            NavMesh.SamplePosition(projectedSpawnPos, out hitInfo, 10f, NavMesh.AllAreas);
            actualSpawnPos = hitInfo.position;

        } while (!inRoomBounds(actualSpawnPos));

        // Spawn in position and set properties
        EnemyStatus curEnemy = Object.Instantiate(enemyTemplate, actualSpawnPos, Quaternion.identity);
        curEnemy.lootTable = lootTable;
        curEnemy.willDropLoot = willDropLoot;

        DungeonPatrolPointBranch dynamicDungeonAI = curEnemy.GetComponent<DungeonPatrolPointBranch>();
        if (dynamicDungeonAI != null) {
            dynamicDungeonAI.setAssignedLayout(dungeonNav);
        }

        curEnemy.spawnIn();

        return curEnemy;
    }


    // Main function to check if enemy is within bounds or not
    private bool inRoomBounds(Vector3 position) {
        bool inXBounds = (transform.position.x - roomWidth) < position.x && position.x < (transform.position.x + roomWidth);
        bool inZBounds = (transform.position.z - roomLength) < position.z && position.z < (transform.position.z + roomLength);

        return inXBounds && inZBounds;
    }
}
