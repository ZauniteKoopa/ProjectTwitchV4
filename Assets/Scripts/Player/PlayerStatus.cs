using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : IUnitStatus
{
    [Header("Base Stats")]
    [SerializeField]
    private float movementSpeed = 5f;
    private bool moving = true;
    private SpeedModifierStatus speedStatus = new SpeedModifierStatus();
    private SpeedModifierStatus attackSpeedStatus = new SpeedModifierStatus();

    [SerializeField]
    [Min(0.01f)]
    private float maxHealth = 20f;
    private float curHealth;
    private float damageReduction = 0f;
    private readonly object healthLock = new object();


    [Header("UI")]
    [SerializeField]
    private PlayerScreenUI playerUI = null;

    public bool invisible = false;



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
        return 0;
    }


    // Main method to inflict basic damage on unit
    //  Pre: damage is a number greater than 0, isTrue indicates if its true damage. true damage is not affected by armor and canCrit: can the damage given crit
    //  Post: unit gets inflicted with damage. returns true if it happens. else otherwise
    public override bool damage(float dmg, bool isTrue) {
        float actualDamage = (isTrue) ? dmg : dmg * (1f - Mathf.Clamp(damageReduction, 0f, 1f));
        lock (healthLock) {
            if (isAlive()) {
                curHealth -= actualDamage;
                playerUI.displayHealth(curHealth, maxHealth);

                if (curHealth <= 0f) {
                    StopAllCoroutines();
                    StartCoroutine(death());
                }
            }
        }

        return curHealth <= 0f;
    }


    // Main function to check if the unit is still alive
    //  Pre: none
    //  Post: returns true is unit is still alive
    public override bool isAlive() {
        return curHealth > 0f;
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


    // Private helper function to die
    private IEnumerator death() {
        yield return 0;
        gameObject.SetActive(false);
    }
}
