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

    public UnityEvent battleRoomStartEvent;
    public UnityEvent battleRoomEndEvent;
    
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

        foreach (EnemyWave wave in enemyWaves) {
            wave.waveFinishedEvent.AddListener(onEnemyWaveFinish);
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

        if (curEnemyWave >= enemyWaves.Length) {
            enemyWaveLock.unlock();
        } else {
            enemyWaves[curEnemyWave].activate(enemyLootTable, lootChance);
        }
    }


}
