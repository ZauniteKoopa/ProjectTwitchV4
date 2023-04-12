using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;

[System.Serializable]
public class UnitDelegate : UnityEvent<IUnitStatus> {};

public abstract class IUnitStatus : MonoBehaviour
{
    // Main function to handle the movement
    private int stunners = 0;
    private readonly object stunLock = new object();
    public UnityEvent stunnedStartEvent;
    public UnityEvent stunnedEndEvent;

    // [SerializeField]
    // protected StatusEffectDisplay statusDisplay;

    // [SerializeField]
    // protected VFX_StatusEffectDisplay statusEffectVFXs;

    // [SerializeField]
    // protected DamagePopup damagePopupPrefab = null;

    // Main death event
    public UnitDelegate unitDeathEvent;
    

    // Main method to get current movement speed considering all speed status effects on unit
    //  Pre: none
    //  Post: Returns movement speed with speed status effects in mind
    public abstract float getMovementSpeed();


    // Main method to get current base attack for sword swings or ranged attacks 
    //  Pre: none
    //  Post: Returns a float that represents base attack (> 0)
    public abstract float getBaseAttack();


    // Main method to inflict basic damage on unit
    //  Pre: damage is a number greater than 0, isTrue indicates if its true damage. true damage is not affected by armor and canCrit: can the damage given crit
    //  Post: unit gets inflicted with damage 
    public abstract bool damage(float dmg, bool isTrue);


    // Main function to check if the unit is still alive
    //  Pre: none
    //  Post: returns true is unit is still alive
    public abstract bool isAlive();


    // Main function to reset unit, especially when player dies
    //  Pre: none
    //  Post: If enemy, reset to passive state, not sensing any enemies
    //        If player, reset all cooldowns to 0 and lose collectibles upon death
    public abstract void reset();


    // Main function to slow down or speed up by a specifed speed factor
    //  Pre: speedFactor > 0.0f. If less than 1, slow. Else, fast
    //  Post: speed is affected accordingly
    public abstract void applySpeedModifier(float speedFactor);


    // Main function to reert a speed modifier
    //  Pre: speedFactor > 0.0f. If less than 1, slow. Else, fast
    //  Post: speed is affected accordingly
    public abstract void revertSpeedModifier(float speedFactor);


    // Function to set movement to true 
    //  Pre: bool representing whether the player is moving or not
    //  Post: enact effects that happen while you're moving or deactivate effects when you aren't
    public abstract void setMoving(bool isMoving);


    // Main function to check if a unit canMove or not
    //  Pre: none
    //  Post: returns whether a unit can move or not
    public bool canMove() {
        bool isStunned;

        lock (stunLock) {
            isStunned = stunners > 0;
        }

        return !isStunned && isAlive();
    }


    // Main function to clear up all stunners
    //  Pre: none
    //  Post: clears out all stunners so that the stunner count is 0
    protected void clearStun() {
        lock(stunLock) {
            stunners = 0;
        }
    }


    // Main function to enable or disable movement. Can handle multiple requests
    //  Pre: a boolean that represents enabling (true) or disabling (false) movement
    //  Post: if the unit is not stunned and true, stun. If unit is stunned and false, unstun UNLESS another stun is active
    public void stun(bool willStun) {
        lock (stunLock) {
            // Invoke event if stunning has started
            if (stunners == 0) {
                stunnedStartEvent.Invoke();
            }

            // Change number fo stunners (it cannot be negative to account for reset functionality)
            stunners += (willStun) ? 1 : -1;
            stunners = (stunners < 0) ? 0 : stunners;

            // Display it
            // if (statusDisplay != null) {
            //     statusDisplay.displayStun(stunners > 0);
            // }

            // Invoke event if stunning has ended
            if (stunners == 0) {
                stunnedEndEvent.Invoke();
            }
        }
    }


    // Static function to help with damage calculations
    private static float STATIC_DEFENSE_FACTOR = 3f;

    // Main function to calculate damage
    //  Pre: attack >= 0f and defense > 0f
    public static float calculateDamage(float attack, float defense) {
        Debug.Assert(attack >= 0f && defense >= 0f);

        // If attack is so small (approaching 0), just return attack
        if (attack < 0.00001f) {
            return attack;
        }

        float damageReduction = STATIC_DEFENSE_FACTOR / (STATIC_DEFENSE_FACTOR + defense);
        return attack * damageReduction;
    }
}
