using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContaminateManager : MonoBehaviour
{
    [SerializeField]
    private LobAction contaminateVisualEffect = null;
    [SerializeField]
    private float contaminateEffectTime = 0.1f;
    [SerializeField]
    private PoisonedUnitSensor poisonedSensor = null;
    private HashSet<EnemyStatus> inRange = new HashSet<EnemyStatus>();

    private MeshRenderer render = null;


    // On awake setup
    private void Awake() {
        render = GetComponent<MeshRenderer>();
    }


    // On update, check if contaminate vision field needs to be rendered
    private void Update() {
        if (poisonedSensor != null && render != null) {
            render.enabled = poisonedSensor.poisonedUnitsNearby();
        }
    }
    
    
    // Main function to call contaminate
    public void contaminateAll() {
        StartCoroutine(contaminateAllSequence());
    }


    // Main sequence to contaminate
    private IEnumerator contaminateAllSequence() {
        // Get copy of targets so that they aren't effected by remove
        HashSet<EnemyStatus> lockedTargets = new HashSet<EnemyStatus>(inRange);

        // For each locked target, just contaminate them
        foreach (EnemyStatus tgt in lockedTargets) {
            LobAction curEffect = Object.Instantiate(contaminateVisualEffect, transform.position, Quaternion.identity);
            curEffect.lobWithTime(transform.position, tgt.transform.position, contaminateEffectTime, null);
        }

        yield return new WaitForSeconds(contaminateEffectTime);

        // For each locked target, just contaminate them
        foreach (EnemyStatus tgt in lockedTargets) {
            tgt.contaminate();
        }
    }


    // Main on trigger enter
    private void OnTriggerEnter(Collider collider) {
        EnemyStatus tgt = collider.GetComponent<EnemyStatus>();

        if (tgt != null) {
            inRange.Add(tgt);
            tgt.deathEvent.AddListener(delegate { onEnemyDeath(tgt); });
        }
    }


    // Main on trigger exit
    private void OnTriggerExit(Collider collider) {
        EnemyStatus tgt = collider.GetComponent<EnemyStatus>();

        if (tgt != null) {
            inRange.Remove(tgt);
            tgt.deathEvent.RemoveListener(delegate { onEnemyDeath(tgt); });
        }
    }


    // Main event handler
    private void onEnemyDeath(EnemyStatus enemy) {
        inRange.Remove(enemy);
        enemy.deathEvent.RemoveListener(delegate { onEnemyDeath(enemy); });
    }


    // Main function to check if you can contaminate or not
    public bool contaminateTargetsFound() {
        foreach(EnemyStatus tgt in inRange) {
            if (tgt.isPoisoned()) {
                return true;
            }
        }

        return false;
    }
}
