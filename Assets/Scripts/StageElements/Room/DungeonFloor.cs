using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DungeonFloor : MonoBehaviour
{
    [Header("Dungeon Tracking")]
    [SerializeField]
    private Room[] dungeonRooms;
    [SerializeField]
    private BattleRoom finalBattleRoom;
    private Room playerRoom;
    private readonly object roomTrackingLock = new object();

    [Header("Enemy Management")]
    [SerializeField]
    private LootTable dungeonFloorLootTable;
    [SerializeField]
    [Range(0f, 1f)]
    private float lootChance = 0.6f;
    [SerializeField]
    [Min(0)]
    private int numStartingEnemies = 0;
    [SerializeField]
    [Min(0)]
    private int maxEnemies = 3;
    [SerializeField]
    [Range(3f, 8f)]
    private float timeBetweenEnemySpawns = 4f;
    [SerializeField]
    [Range(0f, 2f)]
    private float spawnTimeVariance = 1.0f;
    [SerializeField]
    private EnemyStatus[] possibleEnemies = null;
    private HashSet<EnemyStatus> activeEnemies = new HashSet<EnemyStatus>();
    private Coroutine runningEnemySpawner = null;
    private readonly object enemyTrackingLock = new object();

    // Only update the map if this dungeon is currently active
    private bool currentlyActive = false;

    
    // On start, listen to all events associated to rooms
    private void Awake() {
        // Process rooms
        foreach (Room room in dungeonRooms) {
            if (room == null) {
                Debug.LogError("ROOM IN DUNGEON ROOM HAVE BEEN FOUND TO BE NULL");
            }

            if (room != finalBattleRoom) {
                room.playerEnterRoomEvent.AddListener( delegate { onPlayerRoomUpdate(room); } );
                room.enemyRoomEvent.AddListener(onEnemyRoomUpdate);
            }
        }

        // Process battle room
        if (finalBattleRoom == null) {
            Debug.LogError("BATTLE ROOM IN DUNGEON FOUND TO BE NULL");
        }

        finalBattleRoom.battleRoomStartEvent.AddListener(onFinalBattleRoomStart);

        // Start the dungeon
        startDungeon();
    }
    
    
    // Main function to start the dungeon
    public void startDungeon() {
        if (!currentlyActive) {
            currentlyActive = true;
            runningEnemySpawner = StartCoroutine(enemySpawningSequence());
        }
    }


    // Main function to exit the dungeon
    public void exitDungeon() {
        if (currentlyActive) {
            currentlyActive = false;
        }
    }


    // Main function to keep track of where the player is in the map
    private void onPlayerRoomUpdate(Room enteredPlayerRoom) {
        lock (roomTrackingLock) {
            if (playerRoom != null) {
                playerRoom.playerInside = false;
            }

            enteredPlayerRoom.playerInside = true;
            playerRoom = enteredPlayerRoom;

            // UPDATE MAP UI HERE
        }
    }


    private void onEnemyRoomUpdate() {
        lock (roomTrackingLock) {
            // UPDATE MAP UI HERE
        }
    }


    private void onEnemyDeath(EnemyStatus corpse) {
        lock (enemyTrackingLock) {
            activeEnemies.Remove(corpse);
        }

        lock (roomTrackingLock) {
            // UPDATE MAP UI HERE
        }
    }


    // Main event handler function for when the final battle room starts
    private void onFinalBattleRoomStart() {
        StopCoroutine(runningEnemySpawner);
        runningEnemySpawner = null;
    }


    // Main private helper function to get a room to spawn an enemy in
    //  Pre: dungeon must have at least 3 rooms (a room with a player in it, final battle room, a room in which enemies can spawn)
    private Room getRandomSpawnRoom() {
        Debug.Assert(dungeonRooms.Length > 2);
        Room curSpawnRoom = dungeonRooms[Random.Range(0, dungeonRooms.Length)];
        Debug.Assert(curSpawnRoom != null);

        while (curSpawnRoom.playerInside || (curSpawnRoom is BattleRoom)) {
            curSpawnRoom = dungeonRooms[Random.Range(0, dungeonRooms.Length)];
            Debug.Assert(curSpawnRoom != null);
        }

        return curSpawnRoom;
    }


    // Main private helper function to spawn enemies
    private void spawnEnemy() {
        Debug.Assert(possibleEnemies.Length > 0);

        Room spawnRoom = getRandomSpawnRoom();
        EnemyStatus curEnemy = possibleEnemies[Random.Range(0, possibleEnemies.Length)];
        LootTable givenLoot = (Random.Range(0f, 1f) < lootChance) ? dungeonFloorLootTable : null;

        lock (enemyTrackingLock) {
            EnemyStatus enemyInstance = spawnRoom.spawnEnemy(curEnemy, givenLoot);
            activeEnemies.Add(enemyInstance);
            enemyInstance.deathEvent.AddListener(delegate { onEnemyDeath(enemyInstance); });
        }
    }


    // Main enemy spawning sequence that continuously spawns enemies until the player finishes the floor
    //  Will only spawn enemies when max amount of TRACKED enemies are hit
    private IEnumerator enemySpawningSequence() {
        yield return 0;

        // Spawn initial enemies
        for (int e = 0; e < numStartingEnemies; e++) {
            spawnEnemy();
        }

        // Actual spawning sequence
        while (true) {
            // Only spawn enemies if numActiveEnemies is less than allowed max
            if (activeEnemies.Count < maxEnemies) {
                yield return new WaitForSeconds(Random.Range(timeBetweenEnemySpawns - spawnTimeVariance, timeBetweenEnemySpawns + spawnTimeVariance));
                spawnEnemy();
            }

            yield return 0;
        }
    }
}
