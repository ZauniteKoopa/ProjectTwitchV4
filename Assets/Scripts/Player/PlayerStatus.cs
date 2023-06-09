using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerStatus : IUnitStatus
{
    [Header("Base Stats")]
    [SerializeField]
    private float movementSpeed = 5f;
    private bool moving = true;
    private SpeedModifierStatus speedStatus = new SpeedModifierStatus();
    private SpeedModifierStatus attackSpeedStatus = new SpeedModifierStatus();
    private SpeedModifierStatus attackMultiplierStatus = new SpeedModifierStatus();

    [SerializeField]
    [Min(0.01f)]
    private float maxHealth = 20f;
    [SerializeField]
    [Min(0.01f)]
    private float invincibilityFrameDuration = 0.3f;
    private Coroutine activeInvincibilityPeriod = null;
    private float curHealth;
    private float damageReduction = 0f;
    private readonly object healthLock = new object();


    [Header("UI")]
    [SerializeField]
    private PlayerScreenUI playerUI = null;

    [Header("Invisibility")]
    [SerializeField]
    private InvisibilitySensor invisSensor;
    public bool invisible = false;

    // Key management
    private int numKeys = 0;

    // Events
    [Header("Events")]
    public UnityEvent playerHurtEvent;
    public UnityEvent deathEvent;



    // On awake, set up
    private void Awake() {
        if (playerUI == null) {
            Debug.LogError("PLAYER UI NOT CONNECTED TO PLAYER STATUS");
        }

        curHealth = maxHealth;
        playerUI.displayHealth(curHealth, maxHealth);
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
        return attackMultiplierStatus.getSpeedModifier();
    }


    // Main method to inflict basic damage on unit
    //  Pre: damage is a number greater than 0, isTrue indicates if its true damage. true damage is not affected by armor and canCrit: can the damage given crit
    //  Post: unit gets inflicted with damage. returns true if death happens. else otherwise
    public override bool damage(float dmg, bool isTrue) {
        if (activeInvincibilityPeriod == null) {
            float actualDamage = (isTrue) ? dmg : dmg * (1f - Mathf.Clamp(damageReduction, 0f, 1f));
            lock (healthLock) {
                if (isAlive()) {
                    curHealth -= actualDamage;
                    playerUI.displayHealth(curHealth, maxHealth);

                    if (curHealth <= 0f) {
                        StopAllCoroutines();
                        StartCoroutine(death());
                    } else {
                        playerHurtEvent.Invoke();
                        activeInvincibilityPeriod = StartCoroutine(invincibilitySequence());
                    }
                }
            }

            return curHealth <= 0f;
        }

        return false;
    }


    // Main function to check if the unit is still alive
    //  Pre: none
    //  Post: returns true is unit is still alive
    public override bool isAlive() {
        return curHealth > 0f;
    }


    // Main function to gain health bonuses
    //  Pre: healthGain > 0f
    //  Post: increase max health and curHealth by gain
    public void gainHealth(float addedHealth) {
        curHealth += addedHealth;
        maxHealth += addedHealth;

        playerUI.displayHealth(curHealth, maxHealth);
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


    // Function to set movement to true 
    //  Pre: bool representing whether the player is moving or not
    //  Post: enact effects that happen while you're moving or deactivate effects when you aren't
    public override void setMoving(bool isMoving) {
        moving = isMoving;
    }


    // Main function to apply attack speed effect
    public void applyAttackSpeedEffect(float attackModifier) {
        attackSpeedStatus.applySpeedModifier(attackModifier);
    }


    // Main function to revert attack speed effect
    public void revertAttackSpeedEffect(float attackModifier) {
        attackSpeedStatus.revertSpeedModifier(attackModifier);
    }


    // Main function to get attack speed modifier
    public float getAttackSpeedModifier() {
        return attackSpeedStatus.getSpeedModifier();
    }


    // Main function to get the speed modifier for the animator
    public float getMovementSpeedModifier() {
        return speedStatus.getSpeedModifier();
    }


    // Private helper function to die
    private IEnumerator death() {
        yield return 0;
        
        deathEvent.Invoke();
        //gameObject.SetActive(false);
    }


    // Invincibility period
    private IEnumerator invincibilitySequence() {
        yield return new WaitForSeconds(invincibilityFrameDuration);
        activeInvincibilityPeriod = null;
    }


    // Public function to check if a unit is visible or not
    public bool canSeePlayer(EnemyStatus enemyBody) {
        return !invisible || invisSensor.isFoundWithinRange(enemyBody);
    }


    // Main function to take a key
    //  Pre: number of keys required
    //  Post: return true if successful. false otherwise. When successful, the key decrements
    public bool takeKey(int keysRequired) {
        if (numKeys >= keysRequired) {
            numKeys -= keysRequired;
            playerUI.displayNumKeys(numKeys);
            return true;
        }

        return false;
    }


    // Main function to add a key to the inventory
    public void addKey() {
        numKeys++;
        playerUI.displayNumKeys(numKeys);
    }
}
