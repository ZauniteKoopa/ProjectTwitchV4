using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BattleRoom : Room
{
    [SerializeField]
    private EnemyWave[] enemyWaves;
    [SerializeField]
    private LockedDoor[] lockedDoors;
    [SerializeField]
    private LootTable enemyLootTable;
    [SerializeField]
    [Range(0f, 1f)]
    private float lootChance = 0.6f;
    [SerializeField]
    private DungeonFloorEntrance[] possibleDungeonFloorEntrances;
    [SerializeField]
    private ItemChest rewardChest;


    public UnityEvent battleRoomStartEvent;
    public UnityEvent battleRoomEndEvent;
    public UnityEvent dungeonExitEvent;
    
    private AbstractLock enemyWaveLock;
    private bool activated = false;
    private int curEnemyWave = 0;


    // On awake, set up
    private void Start() {
        if (enemyLootTable == null) {
            Debug.LogError("No enemy loot table attached to this battle room");
        }

        if (enemyWaves.Length <= 0) {
            Debug.LogError("No enemy waves found in this battle room!");
        }

        // Set up each enemy wave
        foreach (EnemyWave wave in enemyWaves) {
            wave.waveFinishedEvent.AddListener(onEnemyWaveFinish);
        }

        // Disable all dungeon floor entrances
        foreach (DungeonFloorEntrance entrance in possibleDungeonFloorEntrances) {
            entrance.playerEnterFloorEvent.AddListener(onDungeonExit);
            entrance.gameObject.SetActive(false);
        }

        // Disable chest
        if (rewardChest != null) {
            rewardChest.gameObject.SetActive(false);
        }

        enemyWaveLock = GetComponent<AbstractLock>();
    }
    
    
    // Main event handler for when player enters the room
    protected override void onPlayerEnter(PlayerStatus player) {
        base.onPlayerEnter(player);

        if (!activated) {
            activated = true;

            foreach (LockedDoor door in lockedDoors) {
                door.gameObject.SetActive(true);
            }

            battleRoomStartEvent.Invoke();
            curEnemyWave = 0;
            enemyWaves[0].activate(enemyLootTable, lootChance);
        }
    }


    // Main event handler for when current enemy wave finished
    private void onEnemyWaveFinish() {
        curEnemyWave++;

        // Case where you finish all enemy waves
        if (curEnemyWave >= enemyWaves.Length) {
            // Unlock door of battle room
            enemyWaveLock.unlock();

            // Activate entrances to the next floor
            foreach (DungeonFloorEntrance entrance in possibleDungeonFloorEntrances) {
                entrance.gameObject.SetActive(true);
            }

            // Activate rewards chest
            if (rewardChest != null && rewardChest.getNumPrizes() > 0) {
                rewardChest.gameObject.SetActive(true);
            }

        // Case where you still have enemy waves left to go
        } else {
            enemyWaves[curEnemyWave].activate(enemyLootTable, lootChance);
        }
    }


    // Main event handler for when player exits the dungeon
    public void onDungeonExit() {
        dungeonExitEvent.Invoke();
    }


    // Function to add prize to rewards chest
    public void addPrize(LobAction reward) {
        Debug.Assert(reward != null && rewardChest != null);

        rewardChest.addItem(reward);
    }


    // Main function to set up prizes for the battle room
    public void setUpNextFloorRewards(PrizePool prizePool) {
        List<EndReward> rewards = prizePool.getDistinctEndRewards(possibleDungeonFloorEntrances.Length);

        for (int e = 0; e < possibleDungeonFloorEntrances.Length; e++) {
            possibleDungeonFloorEntrances[e].setProjectedEndPrize(rewards[e]);
        }
    }


    // Main function to set up the actual rewards of the floor
    public void setBattleRoomRewards(EndReward endReward) {
        foreach (LobAction reward in endReward.rewards) {
            rewardChest.addItem(reward);
        }
    }

}
