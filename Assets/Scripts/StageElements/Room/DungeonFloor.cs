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
    private SpawnRoom spawnRoom;
    [SerializeField]
    private BattleRoom finalBattleRoom;
    private Room playerRoom;
    private readonly object roomTrackingLock = new object();
    [SerializeField]
    private bool startDungeonOnAwake = false;
    [SerializeField]
    private bool isHostileFloor = true;

    [Header("Enemy Management")]
    [SerializeField]
    private LootTable dungeonFloorLootTable;
    private DungeonFloorLayout dungeonFloorMap;
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
    private const int MAX_NUM_INGREDIENTS_PER_FLOOR = 9;
    private int curSpawnedIngredients = 0;
    private HashSet<EnemyStatus> activeEnemies = new HashSet<EnemyStatus>();
    private Coroutine runningEnemySpawner = null;
    private readonly object enemyTrackingLock = new object();

    [Header("Enemy Loot Probability")]
    [SerializeField]
    [Min(1)]
    private int probabilityNumerator = 3;
    [SerializeField]
    [Min(1)]
    private int probabilityDenominator = 5;
    [SerializeField]
    [Min(1)]
    private int probabilityVariance = 1;
    private ConditionalProbCalculator enemyLootCondProb;

    [Header("Prize Management")]
    [SerializeField]
    private PrizePool prizePool;
    [SerializeField]
    private DungeonFloorEntrance[] nonHostileEntrances;
    [SerializeField]
    private ItemChest startingReward = null;
    private TwitchInventory curPlayerInventory;
    private PlayerStatus curPlayerStatus;

    // Only update the map if this dungeon is currently active
    private bool currentlyActive = false;

    
    // On start, listen to all events associated to rooms
    private void Start() {
        // Process battle room if battle room is null
        if (finalBattleRoom == null && isHostileFloor) {
            Debug.LogError("BATTLE ROOM IN DUNGEON FOUND TO BE NULL IN A DUNGEON FLOOR THAT'S SUPPOSED TO SPAWN IN ENEMIES");
        }

        if (probabilityNumerator > probabilityDenominator) {
            Debug.LogError("PROB DENOMINATOR IS GREATER THAN PROB NUMERATOR");
        }

        enemyLootCondProb = new ConditionalProbCalculator(probabilityNumerator, probabilityDenominator, probabilityVariance);

        if (isHostileFloor) {
            dungeonFloorMap = new DungeonFloorLayout(dungeonRooms, finalBattleRoom);
            finalBattleRoom.battleRoomStartEvent.AddListener(onFinalBattleRoomStart);
            finalBattleRoom.dungeonExitEvent.AddListener(exitDungeon);
            finalBattleRoom.battleRoomEndEvent.AddListener(onFinalBattleRoomEnd);
        }

        if (startDungeonOnAwake) {
            PlayerStatus player = FindObjectOfType<PlayerStatus>();
            startDungeon(player, null);
        }
    }
    
    
    // Main function to start the dungeon
    public void startDungeon(PlayerStatus playerStatus, EndReward endReward) {
        if (!currentlyActive) {
            // Process rooms
            foreach (Room room in dungeonRooms) {
                if (room == null) {
                    Debug.LogError("ROOM IN DUNGEON ROOM HAVE BEEN FOUND TO BE NULL");
                }

                room.playerEnterRoomEvent.AddListener( delegate { onPlayerRoomUpdate(room); } );
                room.enemyRoomEvent.AddListener(onEnemyRoomUpdate);
                room.enemyDeathEvent.AddListener(onEnemyRoomUpdate);
            }

            // Set up
            currentlyActive = true;
            MapUI.mainMapUI.recenter(dungeonRooms);

            // Have enemies spawn
            if (isHostileFloor && maxEnemies > 0) {
                runningEnemySpawner = StartCoroutine(enemySpawningSequence());
            }

            // Set up rewards in the entrances of the next floor and the rewards of this floor
            curPlayerInventory = playerStatus.transform.parent.GetComponent<TwitchInventory>();
            curPlayerStatus = playerStatus;
            if (startingReward == null) {
                List<EndReward> rewards = prizePool.getDistinctEndRewards(nonHostileEntrances.Length, curPlayerInventory, playerStatus);

                for (int e = 0; e < nonHostileEntrances.Length; e++) {
                    nonHostileEntrances[e].setProjectedEndPrize(rewards[e]);
                }

            } else {
                foreach (LobAction reward in endReward.rewards) {
                    startingReward.addItem(reward);
                }
            }

            // Spawn in player
            if (spawnRoom != null) {
                spawnRoom.spawnInPlayer(playerStatus);
            }
        }
    }


    // Main function to exit the dungeon
    public void exitDungeon() {
        if (currentlyActive) {
            currentlyActive = false;

            // Destroy all enemies nearby
            lock (enemyTrackingLock) {
                foreach (EnemyStatus activeEnemy in activeEnemies) {
                    activeEnemy.deactivate();
                }

                activeEnemies.Clear();
            }
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

            if (currentlyActive) {
                MapUI.mainMapUI.render(dungeonRooms);
            }
        }
    }


    private void onEnemyRoomUpdate() {
        lock (roomTrackingLock) {
            if (currentlyActive) {
                MapUI.mainMapUI.render(dungeonRooms);
            }
        }
    }


    private void onEnemyDeath(EnemyStatus corpse) {
        if (currentlyActive) {
            lock (enemyTrackingLock) {
                activeEnemies.Remove(corpse);
            }
        }

        lock (roomTrackingLock) {
            if (currentlyActive) {
                MapUI.mainMapUI.render(dungeonRooms);
            }
        }
    }


    // Main event handler function for when the final battle room starts
    private void onFinalBattleRoomStart() {
        if (runningEnemySpawner != null) {
            StopCoroutine(runningEnemySpawner);
        }
        runningEnemySpawner = null;
    }


    // Main event handler for when final battle room ends: set up next rewards
    private void onFinalBattleRoomEnd() {
        finalBattleRoom.setUpNextFloorRewards(prizePool, curPlayerInventory, curPlayerStatus);
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
        LootTable givenLoot = dungeonFloorLootTable;

        // Determine if they will drop loot
        bool willDropLoot = curSpawnedIngredients < MAX_NUM_INGREDIENTS_PER_FLOOR && enemyLootCondProb.rolledHit();
        if (willDropLoot) {
            curSpawnedIngredients++;
        }

        lock (enemyTrackingLock) {
            EnemyStatus enemyInstance = spawnRoom.spawnEnemy(curEnemy, givenLoot, dungeonFloorMap, willDropLoot);
            activeEnemies.Add(enemyInstance);
            enemyInstance.deathEvent.AddListener(delegate { onEnemyDeath(enemyInstance); });
        }
    }


    // Main enemy spawning sequence that continuously spawns enemies until the player finishes the floor
    //  Will only spawn enemies when max amount of TRACKED enemies are hit
    private IEnumerator enemySpawningSequence() {
        yield return new WaitForSeconds(0.1f);

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
