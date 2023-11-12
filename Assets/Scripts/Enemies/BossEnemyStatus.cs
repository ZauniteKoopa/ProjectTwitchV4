using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BossEnemyStatus : EnemyStatus
{
    // Unity Events
    public UnityEvent enemyPhaseTransitionBeginEvent;
    public UnityEvent enemyPhaseTransitionEndEvent;
    public UnityEvent initializedEvent;

    // Serialize Variables
    [SerializeField]
    private float[] healthPhaseThresholds;
    [SerializeField]
    [Min(1)]
    private int numLootDropsPerPhase = 1;
    [SerializeField]
    [Min(0.01f)]
    private float phaseTransitionTime = 1.5f;

    // Current phase of unit, starting from Phase 0
    private int curPhase = 0;
    private float requiredPhaseThreshold = 1f;
    private bool inTransition = false;
    private EnemyBossStatusUI bossStatusUI = null;


    // Initialize and error check
    protected override void initialize() {
        // Error check
        float sum = 0f;
        foreach (float healthThreshold in healthPhaseThresholds) {
            if (healthThreshold <= 0f) {
                Debug.LogError("HEALTH THESHOLD FOR BOSS PHASE SHOULD BE A POSITIVE NUMBER BETWEEN 0F AND 1F");
            }

            sum += healthThreshold;
        }

        if (sum < 0.999f || sum > 1.00001f) {
            Debug.LogError("ALL HEALTH PHASE THRESHOLDS SHOULD ADD TO 1");
        }

        bossStatusUI = (enemyStatusUI as EnemyBossStatusUI);
        if (bossStatusUI == null) {
            Debug.LogError("ATTACHED ENEMY STATUS UI IS NOT A BOSS STATUS UI FOR THIS BOSS UNIT");
        }

        // Set initial threshold
        curPhase = 0;
        requiredPhaseThreshold = 1f - healthPhaseThresholds[curPhase];
        bossStatusUI.updatePhaseBar(requiredPhaseThreshold);

        initializedEvent.Invoke();
    }


    // Damaging enemy
    public override bool damage(float dmg, bool isTrue, bool attractsAttention = true, bool isCrit = false) {        
        // Only deal damage if you're in transition
        if (!inTransition) {
            bool aliveState = !base.damage(dmg, isTrue, attractsAttention, isCrit);

            // If isAlive and you met transition checks, actually transition
            if (aliveState && curHealth <= (maxHealth * requiredPhaseThreshold)) {
                curPhase++;
                requiredPhaseThreshold -= healthPhaseThresholds[curPhase];

                bossStatusUI.updatePhaseBar(requiredPhaseThreshold);
                dropLoot(numLootDropsPerPhase);

                StartCoroutine(transitionPhases());
            }

            return !aliveState;
        }

        return false;
    }


    // Main private sequence for transitioning between phases
    private IEnumerator transitionPhases() {
        enemyPhaseTransitionBeginEvent.Invoke();
        inTransition = true;
        stun(true);

        yield return new WaitForSeconds(phaseTransitionTime);

        stun(false);
        inTransition = false;
        enemyPhaseTransitionEndEvent.Invoke();
    }


    // Main accessor function to get which phase the boss is in currently (starting from 0)
    public int getCurrentPhase() {
        return curPhase;
    }


    // Main accessor method to check if you're in transition
    public bool inTransitionState() {
        return inTransition;
    }


    // Main accessor method to get the number of phases
    public int getNumPhases() {
        return healthPhaseThresholds.Length;
    }
}
