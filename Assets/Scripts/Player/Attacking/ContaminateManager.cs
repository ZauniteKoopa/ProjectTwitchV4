using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class ContaminateManager : MonoBehaviour
{
    [SerializeField]
    private LobAction contaminateVisualEffect = null;
    [SerializeField]
    private float contaminateEffectTime = 0.1f;
    [SerializeField]
    private PoisonedUnitSensor poisonedSensor = null;
    private Dictionary<EnemyStatus, UnityAction> inRangeEnemyDelegates = new Dictionary<EnemyStatus, UnityAction>();
    [SerializeField]
    [Range(0, 20)]
    private int contaminateTimeStopFrames = 20;
    [SerializeField]
    [Range(0f, 1.5f)]
    private float cameraShakeMagnitude = 0f;

    private MeshRenderer render = null;
    public UnityEvent nearbyEnemyDeathEvent;


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
    public void contaminateAll(float attackModifier) {
        StartCoroutine(contaminateAllSequence(attackModifier));
    }


    // Main sequence to contaminate
    private IEnumerator contaminateAllSequence(float attackModifier) {
        // Get copy of targets so that they aren't effected by remove
        var lockedTargets = inRangeEnemyDelegates.Keys.ToArray();

        // For each locked target, just contaminate them
        foreach (EnemyStatus tgt in lockedTargets) {
            if (tgt.isPoisoned()) {
                LobAction curEffect = Object.Instantiate(contaminateVisualEffect, transform.position, Quaternion.identity);
                curEffect.dynamicLobWithTime(transform.position, tgt.transform, contaminateEffectTime, null);
            }
        }

        yield return new WaitForSeconds(contaminateEffectTime);

        // For each locked target, just contaminate them
        foreach (EnemyStatus tgt in lockedTargets) {
            if (tgt.isPoisoned()) {
                tgt.contaminate(attackModifier);
            }
        }

        PlayerCameraController.hitStop(contaminateTimeStopFrames);
        PlayerCameraController.shakeCamera(contaminateTimeStopFrames, cameraShakeMagnitude);
    }


    // Main on trigger enter
    private void OnTriggerEnter(Collider collider) {
        EnemyStatus tgt = collider.GetComponent<EnemyStatus>();

        if (tgt != null && !inRangeEnemyDelegates.ContainsKey(tgt)) {
            UnityAction curDelegate;
            tgt.deathEvent.AddListener(curDelegate = delegate { onEnemyDeath(tgt); });
            inRangeEnemyDelegates.Add(tgt, curDelegate);
        }
    }


    // Main on trigger exit
    private void OnTriggerExit(Collider collider) {
        EnemyStatus tgt = collider.GetComponent<EnemyStatus>();

        if (tgt != null && inRangeEnemyDelegates.ContainsKey(tgt)) {
            tgt.deathEvent.RemoveListener(inRangeEnemyDelegates[tgt]);
            inRangeEnemyDelegates.Remove(tgt);
        }
    }


    // Main event handler
    private void onEnemyDeath(EnemyStatus enemy) {
        if (enemy != null && inRangeEnemyDelegates.ContainsKey(enemy)) {
            // Remove listener
            enemy.deathEvent.RemoveListener(inRangeEnemyDelegates[enemy]);
            inRangeEnemyDelegates.Remove(enemy);

            // Invoke nearby enemy death event for player
            nearbyEnemyDeathEvent.Invoke();
        }
    }


    // Main function to check if you can contaminate or not
    public bool contaminateTargetsFound() {
        var inRange = inRangeEnemyDelegates.Keys.ToArray();

        foreach(EnemyStatus tgt in inRange) {
            if (tgt.isPoisoned()) {
                return true;
            }
        }

        return false;
    }
}
