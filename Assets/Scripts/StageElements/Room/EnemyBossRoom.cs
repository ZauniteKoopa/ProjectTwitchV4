using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBossRoom : Room
{
    [SerializeField]
    private BossEnemyStatus bossEnemy;
    private bool activated = false;

    [Header("Enemy Spawning")]
    [SerializeField]
    private LootTable minionLootTable;
    [SerializeField]
    private bool willSpawnMinions = false;
    [SerializeField]
    private EnemyStatus[] possibleMinions;
    [SerializeField]
    [Min(0)]
    private int enemySpawnStartPhase = 1;
    [SerializeField]
    [Min(0)]
    private int maxNumMinions = 4;
    [SerializeField]
    [Min(0)]
    private int initialNumMinions = 2;
    [SerializeField]
    [Min(0.01f)]
    private float timeBetweenMinionSpawns = 7.5f;
    private HashSet<EnemyStatus> activeMinions = new HashSet<EnemyStatus>();
    private Coroutine minionSpawningSequence = null;
    private readonly object minionTrackingLock = new object();

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


    [Header("Doors")]
    [SerializeField]
    private LockedDoor[] lockedDoors;
    private AbstractLock bossLock;


    // Main event handler for when player enters the room
    protected override void onPlayerEnter(PlayerStatus player) {
        base.onPlayerEnter(player);

        if (!activated) {
            activated = true;

            // Connect to boss events and boss spawnin
            bossEnemy.deathEvent.AddListener(onBossDeath);
            bossEnemy.enemyPhaseTransitionBeginEvent.AddListener(onBossTransitioned);
            bossEnemy.GetComponent<EnemyBossBehaviorTree>().spawnInBoss(player.transform);

            // Lock doors
            foreach (LockedDoor door in lockedDoors) {
                door.gameObject.SetActive(true);
            }
            bossLock = GetComponent<AbstractLock>();

            // Set up enemy spawning
            enemyLootCondProb = new ConditionalProbCalculator(probabilityNumerator, probabilityDenominator, probabilityVariance);
            if (willSpawnMinions && enemySpawnStartPhase == 0) {
                minionSpawningSequence = StartCoroutine(enemySpawningSequence());
            }
        }
    }


    // Main event handler for when boss dies
    private void onBossDeath() {
        // Unlock door of battle room
        bossLock.unlock();

        // Stop current enemy spawning sequence
        if (minionSpawningSequence != null) {
            StopCoroutine(minionSpawningSequence);
        }
    }


    // Main event handler function for when boss has transitioned
    private void onBossTransitioned() {
        int curPhase = bossEnemy.getCurrentPhase();

        if (willSpawnMinions && minionSpawningSequence == null && curPhase >= enemySpawnStartPhase) {
            minionSpawningSequence = StartCoroutine(enemySpawningSequence());
        }
    }


    protected override void onEnemyDeath(EnemyStatus corpse) {
        base.onEnemyDeath(corpse);
        lock (minionTrackingLock) {
            activeMinions.Remove(corpse);
        }
    }


    // Main enemy spawning sequence that continuously spawns enemies until the player finishes the floor
    //  Will only spawn enemies when max amount of TRACKED enemies are hit
    private IEnumerator enemySpawningSequence() {
        yield return 0;

        // Spawn initial enemies
        for (int e = 0; e < initialNumMinions; e++) {
            spawnEnemyHelper();
        }

        // Actual spawning sequence
        while (true) {
            // Only spawn enemies if numactiveMinions is less than allowed max
            if (activeMinions.Count < maxNumMinions) {
                yield return new WaitForSeconds(timeBetweenMinionSpawns);
                spawnEnemyHelper();
            }

            yield return 0;
        }
    }


    // Main private helper function to spawn enemies
    private void spawnEnemyHelper() {
        Debug.Assert(possibleMinions.Length > 0);

        EnemyStatus curEnemy = possibleMinions[Random.Range(0, possibleMinions.Length)];
        LootTable givenLoot = minionLootTable;
        bool willDropLoot = enemyLootCondProb.rolledHit();

        lock (minionTrackingLock) {
            EnemyStatus enemyInstance = spawnEnemy(curEnemy, givenLoot, null, willDropLoot);
            activeMinions.Add(enemyInstance);
            enemyInstance.deathEvent.AddListener(delegate { onEnemyDeath(enemyInstance); });
        }
    }


    // Main function to spawn an enemy inside this room (ASSUMES A SQUARE ROOM)
    public override EnemyStatus spawnEnemy(EnemyStatus enemyTemplate, LootTable lootTable, DungeonFloorLayout dungeonNav, bool willDropLoot) {
        // Get spawn position
        float emptySpaceLength = roomLength - WALL_OFFSET;
        float emptySpaceWidth = roomWidth - WALL_OFFSET;
        Vector3 spawnPos = new Vector3(Random.Range(-emptySpaceWidth / 2f, emptySpaceWidth / 2f), 0f, Random.Range(-emptySpaceLength / 2f, emptySpaceLength / 2f));
        spawnPos += transform.position;

        // Get nav mesh adjusted point 
        NavMeshHit hitInfo;
        NavMesh.SamplePosition(spawnPos, out hitInfo, 10f, UnityEngine.AI.NavMesh.AllAreas);
        spawnPos = hitInfo.position;

        // Spawn in position and set properties
        EnemyStatus curEnemy = Object.Instantiate(enemyTemplate, spawnPos, Quaternion.identity);
        curEnemy.lootTable = lootTable;
        curEnemy.willDropLoot = willDropLoot;
        curEnemy.spawnIn();

        return curEnemy;
    }

}
