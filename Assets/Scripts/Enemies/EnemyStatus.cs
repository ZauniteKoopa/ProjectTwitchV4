using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyStatus : IUnitStatus
{
    [Header("Base Stats")]
    [SerializeField]
    [Min(0.01f)]
    private float movementSpeed = 5f;
    [SerializeField]
    [Min(0.01f)]
    private float maxHealth = 20f;
    private float curHealth;
    private bool moving = true;
    private float damageReduction = 0f;

    private SpeedModifierStatus speedStatus = new SpeedModifierStatus();
    private SpeedModifierStatus attackMultiplierStatus = new SpeedModifierStatus();
    private readonly object healthLock = new object();

    [Header("UI")]
    [SerializeField]
    private EnemyStatusUI enemyStatusUI;

    [Header("Events")]
    public UnityEvent deathEvent;

    // Poison stacks
    private int curPoisonStacks = 0;
    private int curPoisonTick = 0;
    private Coroutine currentPoisoningSequence = null;
    private PoisonVial curPoison = null;
    private readonly object poisonLock = new object();

    private const int MAX_POISON_STACKS = 6;
    private const float POISON_TICK_DURATION = 1f;
    private const int MAX_POISON_TICKS = 6;


    // On awake, set curHealth to maxHealth
    private void Awake() {
        curHealth = maxHealth;

        // Initialize enemy status UI
        if (enemyStatusUI == null) {
            Debug.LogError("NO ENEMY STATUS UI CONNECTED");
        }

        enemyStatusUI.updateHealthBar(curHealth, maxHealth);
        enemyStatusUI.updatePoisonHalo(0, Color.white);
    }


    // Main method to get current movement speed considering all speed status effects on unit
    //  Pre: none
    //  Post: Returns movement speed with speed status effects in mind
    public override float getMovementSpeed() {
        return (moving && canMove()) ? movementSpeed * speedStatus.getSpeedModifier() : 0f;
    }


    // Main method to get current base attack for sword swings or ranged attacks 
    //  Pre: none
    //  Post: Returns a float that represents base attack (> 0)
    public override float getBaseAttack() {
        return 0;
    }


    // Main method to inflict basic damage on unit
    //  Pre: damage is a number greater than 0, isTrue indicates if its true damage. true damage is not affected by armor and canCrit: can the damage given crit
    //  Post: unit gets inflicted with damage. returns true if unit dies. false otherwise
    public override bool damage(float dmg, bool isTrue) {
        float actualDamage = (isTrue) ? dmg : dmg * (1f - Mathf.Clamp(damageReduction, 0f, 1f));
        lock (healthLock) {
            if (isAlive()) {
                curHealth -= actualDamage;
                enemyStatusUI.updateHealthBar(curHealth, maxHealth);

                if (curHealth <= 0f) {
                    StopAllCoroutines();
                    StartCoroutine(death());
                }
            }
        }

        return curHealth <= 0f;
    }


    // Main function to inflict poison damage on a unit
    //  Pre: dmg >= 0, isTrue represents true damage (damage reduction doesn't applie), poison != null && appliedStacks >= 0
    //  Post: does damage and resets poison timer with new poison and increased stacks
    public bool poisonDamage(float dmg, bool isTrue, PoisonVial poison, int appliedStacks, bool overridePoison = true) {
        Debug.Assert(dmg >= 0f && poison != null && appliedStacks >= 0);

        // Increase the number of poison and reset poison ticker
        if (isAlive()) {
            lock (poisonLock) {

                curPoisonStacks = Mathf.Min(curPoisonStacks + appliedStacks, MAX_POISON_STACKS);

                // If applies stack, then vibrate the poison tick counter
                if (appliedStacks > 0) {
                    enemyStatusUI.vibratePoisonHalo();
                }
                
                // If it overrides poison, override it
                if (overridePoison || curPoison == null) {
                    curPoison = poison;
                }

                // Poisoning sequence handling
                if (currentPoisoningSequence == null) {
                    currentPoisoningSequence = StartCoroutine(poisoningSequence());
                } else {
                    curPoisonTick = 0;
                }

                enemyStatusUI.updatePoisonHalo(curPoisonStacks, poison.getColor());
            }
        }

        // Actually damage the unit
        return damage(dmg, isTrue);
    }


    // Main function to check if the unit is still alive
    //  Pre: none
    //  Post: returns true is unit is still alive
    public override bool isAlive() {
        return curHealth > 0;
    }


    // Main function to reset unit, especially when player dies
    //  Pre: none
    //  Post: If enemy, reset to passive state, not sensing any enemies
    //        If player, reset all cooldowns to 0 and lose collectibles upon death
    public override void reset() {}


    // Main function to slow down or speed up by a specifed speed factor
    //  Pre: speedFactor > 0.0f. If less than 1, slow. Else, fast
    //  Post: speed is affected accordingly
    public override void applySpeedModifier(float speedFactor) {
        speedStatus.applySpeedModifier(speedFactor);
    }


    // Main function to reert a speed modifier
    //  Pre: speedFactor > 0.0f. If less than 1, slow. Else, fast
    //  Post: speed is affected accordingly
    public override void revertSpeedModifier(float speedFactor) {
        speedStatus.revertSpeedModifier(speedFactor);
    }

    // Main function to increase or decrease an attack by a specific factor
    //  Pre: attackFactor > 0.0f. If less than 1, debuff. Else, buff
    //  Post: attack is affected accordingly
    public override void applyAttackModifier(float attackFactor) {
        attackMultiplierStatus.applySpeedModifier(attackFactor);
    }


    // Main function to reert an attack modifier
    //  Pre: attackFactor > 0.0f. If less than 1, debuff. Else, buff
    //  Post: attack is affected accordingly
    public override void revertAttackModifier(float attackFactor) {
        attackMultiplierStatus.revertSpeedModifier(attackFactor);
    }


    // Main function to contaminate the unit based on the number of poison stacks
    public void contaminate() {
        if (curPoisonStacks > 0 && curPoison != null) {
            curPoison.contaminate(this, curPoisonStacks);
        }
    }


    // Main function to check if you're poisoned
    public bool isPoisoned() {
        return curPoisonStacks > 0;
    }


    // Function to set movement to true 
    //  Pre: bool representing whether the player is moving or not
    //  Post: enact effects that happen while you're moving or deactivate effects when you aren't
    public override void setMoving(bool isMoving) {
        moving = isMoving;
    }


    // Main private IEnumerator for poisoning
    //  ONLY 1 SHOULD BE RUNNING AT A TIME and unit is actually poisoned
    private IEnumerator poisoningSequence() {
        Debug.Assert(curPoison != null && curPoisonStacks > 0);

        curPoisonTick = 0;

        while (curPoisonTick < MAX_POISON_TICKS) {
            yield return new WaitForSeconds(POISON_TICK_DURATION);

            lock (poisonLock) {
                Debug.Assert(curPoison != null && curPoisonStacks > 0);
                damage(curPoison.getPoisonDamage(curPoisonStacks), true);
                curPoisonTick++;
            }
        }

        clearPoison();
    }


    // Private helper function to clear the poison
    private void clearPoison() {
        lock (poisonLock) {
            curPoisonStacks = 0;
            curPoison = null;
            curPoisonTick = 0;
            enemyStatusUI.updatePoisonHalo(curPoisonStacks, Color.white);

            if (currentPoisoningSequence != null) {
                StopCoroutine(currentPoisoningSequence);
                currentPoisoningSequence = null;
            }
        }
    }

    // Private helper function to die
    private IEnumerator death() {
        deathEvent.Invoke();

        yield return 0;
        gameObject.SetActive(false);
    }
}
