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

    public UnityEvent battleRoomStartEvent;
    public UnityEvent battleRoomEndEvent;
    
    private AbstractLock enemyWaveLock;
    private bool activated = false;
    private int curEnemyWave = 0;


    // On awake, set up
    private void Start() {
        foreach (EnemyWave wave in enemyWaves) {
            wave.waveFinishedEvent.AddListener(onEnemyWaveFinish);
        }
    }
    
    
    // Main event handler for when player enters the room
    protected override void onPlayerEnter(PlayerStatus player) {
        base.onPlayerEnter(player);

        if (!activated) {
            activated = true;

            foreach (LockedDoor door in lockedDoors) {
                door.gameObject.SetActive(true);
            }

            curEnemyWave = 0;
            enemyWaves[0].activate();
        }
    }


    // Main event handler for when current enemy wave finished
    private void onEnemyWaveFinish() {
        curEnemyWave++;

        if (curEnemyWave >= enemyWaves.Length) {
            enemyWaveLock.unlock();
        } else {
            enemyWaves[curEnemyWave].activate();
        }
    }


}
