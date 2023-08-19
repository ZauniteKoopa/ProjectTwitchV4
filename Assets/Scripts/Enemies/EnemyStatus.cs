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
    private const float BACKSTAB_DAMAGE_MULTIPLIER = 2.5f;
    [SerializeField]
    [Min(0.01f)]
    private float maxHealth = 20f;
    private float curHealth;
    private bool moving = true;
    [SerializeField]
    [Range(0f, 0.8f)]
    private float damageReduction = 0f;
    private bool died = false;

    private SpeedModifierStatus speedStatus = new SpeedModifierStatus();
    private SpeedModifierStatus attackMultiplierStatus = new SpeedModifierStatus();
    private SpeedModifierStatus defenseModifierStatus = new SpeedModifierStatus();
    private readonly object healthLock = new object();

    [Header("UI")]
    [SerializeField]
    private EnemyStatusUI enemyStatusUI;
    [SerializeField]
    private DamagePopup damagePopupPrefab;
    [SerializeField]
    private Color normalDamagePopupColor = Color.yellow;
    [SerializeField]
    private Color criticalHitDamagePopupColor = Color.red;
    private const float DAMAGE_POPUP_STAY_DURATION = 0.1f;
    private DamagePopup curDamagePopup = null;
    private bool lastPopupWasCrit = false;


    [Header("Events")]
    public UnityEvent deathEvent;
    public UnityEvent enemyNoticesDamageEvent;

    // Poison stacks
    private int curPoisonStacks = 0;
    private float curPoisonTime = 0;
    private Coroutine currentPoisoningSequence = null;
    private PoisonVial curPoison = null;
    private readonly object poisonLock = new object();

    private const int MAX_POISON_STACKS = 6;
    private const float MAX_POISON_DURATION = 6f;

    // Loot status
    [SerializeField]
    private int numLootDrops = 1;
    [SerializeField]
    private float lootDropRange = 1f;
    [SerializeField]
    private LayerMask lootCollisionLayerMask;
    [SerializeField]
    private float lootDropSpeed = 20f;
    [SerializeField]
    private GameObject lootSatchel = null;
    public LootTable lootTable;
    public bool willDropLoot = false;

    [SerializeField]
    private bool spawnInOnAwake = false;
    [SerializeField]
    private BasicAppearEffect spawnVFX = null;
    private bool spawned;

    // On awake, set curHealth to maxHealth
    private void Awake() {
        // Initialize enemy status UI
        if (enemyStatusUI == null) {
            Debug.LogError("NO ENEMY STATUS UI CONNECTED");
        }

        gameObject.SetActive(false);

        if (spawnInOnAwake) {
            spawnIn();
        }
    }

    // Main function to spawn in unit
    public void spawnIn() {
        // Only spawn in a unit once
        if (!spawned) {
            gameObject.SetActive(false);
            spawned = true;
            curHealth = maxHealth;

            enemyStatusUI.updateHealthBar(curHealth, maxHealth);
            enemyStatusUI.updatePoisonHalo(0, Color.white, false, false);
            lootSatchel.SetActive(lootTable != null && willDropLoot);

            // Set up affect
            Vector3 spawnInForward = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(0f, 1f)).normalized;
            BasicAppearEffect curSpawnEffect = Object.Instantiate(spawnVFX, transform.position, Quaternion.identity);
            curSpawnEffect.transform.localScale = transform.localScale;
            curSpawnEffect.transform.forward = spawnInForward;
            curSpawnEffect.effectEndEvent.AddListener(delegate { onSpawnInSequenceEnd(spawnInForward); });
            curSpawnEffect.executeEffect();
        }
    }


    // Main event handler for when spawn in function finishes
    private void onSpawnInSequenceEnd(Vector3 spawnForward) {
        transform.forward = spawnForward;
        gameObject.SetActive(true);
    }


    // Main method to get current movement speed considering all speed status effects on unit
    //  Pre: none
    //  Post: Returns movement speed with speed status effects in mind
    public override float getMovementSpeed() {
        float curMovementSpeed = (moving && canMove()) ? movementSpeed * speedStatus.getSpeedModifier() : 0f;

        if (curPoison != null) {
            curMovementSpeed *= curPoison.getSpeedModifier(curPoisonStacks);
        }

        return curMovementSpeed;
    }



    // Main method to get current base attack for sword swings or ranged attacks 
    //  Pre: none
    //  Post: Returns a float that represents base attack (> 0)
    public override float getBaseAttack() {
        return attackMultiplierStatus.getSpeedModifier();
    }


    // Main method to inflict basic damage on unit
    //  Pre: damage is a number greater than 0, isTrue indicates if its true damage. true damage is not affected by armor and canCrit: can the damage given crit
    //  Post: unit gets inflicted with damage. returns true if unit dies. false otherwise
    public override bool damage(float dmg, bool isTrue, bool attractsAttention = true, bool isCrit = false) {
        if (attractsAttention) {
            enemyNoticesDamageEvent.Invoke();
        }
        
        float actualDamage = (isTrue) ? dmg : dmg * (1f - Mathf.Clamp(getDamageReduction(), 0f, 1f));
        actualDamage *= (isCrit) ? BACKSTAB_DAMAGE_MULTIPLIER : 1f;

        lock (healthLock) {
            if (isAlive()) {
                curHealth -= actualDamage;
                enemyStatusUI.updateHealthBar(curHealth, maxHealth);
                launchDamagePopup(actualDamage, isCrit);

                if (curHealth <= 0f && !died) {
                    died = true;
                    deathEvent.Invoke();
                    StopAllCoroutines();
                    StartCoroutine(death());
                }
            }
        }

        if (curPoison != null) {
            enemyStatusUI.updatePoisonHalo(
                curPoisonStacks,
                curPoison.getColor(),
                curPoison.sideEffect.maxStackEffect,
                willApplyPostContaminateHitbox()
            );
        }

        return curHealth <= 0f;
    }


    // Main function to inflict poison damage on a unit
    //  Pre: dmg >= 0, isTrue represents true damage (damage reduction doesn't applie), poison != null && appliedStacks >= 0
    //  Post: does damage and resets poison timer with new poison and increased stacks
    public bool poisonDamage(float dmg, bool isTrue, PoisonVial poison, int appliedStacks, bool overridePoison = true, bool isCrit = false) {
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
                if (currentPoisoningSequence == null && appliedStacks > 0) {
                    currentPoisoningSequence = StartCoroutine(poisoningSequence());
                } else if (appliedStacks > 0) {
                    curPoisonTime = 0f;
                }
            }
        }

        // Actually damage the unit
        return damage(dmg, isTrue, overridePoison, isCrit);
    }


    // Main private helper function to launch damage popup
    private void launchDamagePopup(float damage, bool willBackstab) {
        // Create new popup is no new popup found in time period
        if (curDamagePopup == null) {
            // Set up popup
            curDamagePopup = Object.Instantiate(damagePopupPrefab, transform.position, Quaternion.identity);
            Color damageColor = willBackstab ? criticalHitDamagePopupColor : normalDamagePopupColor;

            // Set up flags
            lastPopupWasCrit = willBackstab;

            // Launch
            curDamagePopup.launch(damage, transform, damageColor);
            StartCoroutine(launchPopupSequence());

        // Update existing popup if one is still going
        } else {
            // Set up color
            lastPopupWasCrit = willBackstab;
            Color newColor = (lastPopupWasCrit) ? criticalHitDamagePopupColor : normalDamagePopupColor;

            // Update
            curDamagePopup.updateDamage(damage, newColor);
        }
    }


    // Main launch popup sequence
    private IEnumerator launchPopupSequence() {
        yield return new WaitForSeconds(DAMAGE_POPUP_STAY_DURATION);
        curDamagePopup = null;
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


    
    // Main private helper function to check if user will apply post contaminate side effect
    private bool willApplyPostContaminateHitbox() {
        if (curPoison == null) {
            return false;
        }

        float contaminateDmg = curPoison.getProjectedContaminateDamage(curPoisonStacks);
        float actualDamage = contaminateDmg * (1f - Mathf.Clamp(getDamageReduction(), 0f, 1f));

        return curPoison.sideEffect.postContaminateHitbox != null &&
            (curPoisonStacks >= PoisonVial.poisonVialConstants.minPostContaminateStacks ||
             actualDamage >= curHealth);
    }


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


    // Main function to increase or decrease defense by a specific factor
    public override void applyDefenseModifier(float defenseFactor) {
        defenseModifierStatus.applySpeedModifier(defenseFactor);
    }


    // Main function to revert a defense modifier
    public override void revertDefenseModifier(float defenseFactor) {
        defenseModifierStatus.revertSpeedModifier(defenseFactor);
    }


    // Main function to contaminate the unit based on the number of poison stacks
    public void contaminate(float attackModifier) {
        if (curPoisonStacks > 0 && curPoison != null) {
            curPoison.contaminate(this, curPoisonStacks, attackModifier);
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

        curPoisonTime = 0f;

        while (curPoisonTime < MAX_POISON_DURATION && curPoison != null) {
            float curDecayRate = curPoison.getPoisonDecayRate(curPoisonStacks);

            yield return new WaitForSeconds(curDecayRate);

            lock (poisonLock) {
                Debug.Assert(curPoison != null && curPoisonStacks > 0);
                damage(curPoison.getPoisonDamage(curPoisonStacks), true, false);
                curPoisonTime += curDecayRate;
            }
        }

        clearPoison();
    }


    // Private helper function to clear the poison
    private void clearPoison() {
        lock (poisonLock) {
            curPoisonStacks = 0;
            curPoison = null;
            curPoisonTime = 0f;
            enemyStatusUI.updatePoisonHalo(curPoisonStacks, Color.white, false, false);

            if (currentPoisoningSequence != null) {
                StopCoroutine(currentPoisoningSequence);
                currentPoisoningSequence = null;
            }
        }
    }

    // Private helper function to die
    private IEnumerator death() {
        GetComponent<Collider>().enabled = false;
        dropLoot();

        yield return 0;
        gameObject.SetActive(false);
    }


    // Private helper function to drop loot
    private void dropLoot() {
        int curLootDrops = (willDropLoot) ? numLootDrops : 0;
        if (curPoison != null) {
            curLootDrops += curPoison.sideEffect.getAdditionalLoot(curPoisonStacks);
        }

        if (lootTable != null) {
            for (int d = 0; d < curLootDrops; d++) {
                // Get associated current loot
                LobAction chosenLoot = lootTable.getLootDrop();
                LobAction curLoot = Object.Instantiate(chosenLoot, transform.position, Quaternion.identity);

                curLoot.lobAroundRadius(transform.position, lootDropRange, lootDropSpeed, lootCollisionLayerMask);
            }
        }
    }


    // Main function to get the damage reduction
    private float getDamageReduction() {
        float curReduction = damageReduction * defenseModifierStatus.getSpeedModifier();
        if (curPoison != null) {
            curReduction *= curPoison.sideEffect.getDefenseReductionFactor(curPoisonStacks);
        }

        return curReduction;
    }
}
