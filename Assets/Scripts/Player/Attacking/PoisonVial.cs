using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;
using UnityEngine.UI;
using TMPro;

public enum PoisonVialStat {
    POTENCY,
    POISON,
    REACTIVITY,
    STICKINESS
}

public class PoisonVial
{
    // Static const variables
    private static readonly int STARTING_AMMO = 40;
    private static readonly int PRIMARY_BOLT_AMMO_COST = 1;
    private static readonly float POISON_DMG_PER_STACK = 0.5f;
    private static readonly float CONTAMINATE_DMG_PER_STACK = 5f;

    private static readonly int AMMO_GAIN_PER_INGREDIENT = 10;
    private static readonly int MAX_STAT = 3;
    private static readonly int MAX_CRAFT_ATTEMPTS = 10;
    public static readonly int MAX_AMMO = 60;
    public static PoisonVialConstants poisonVialConstants;

    // Main vial stats
    private Dictionary<PoisonVialStat, int> vialStats;
    private int ammo;
    private SideEffect sideEffect;
    private float startingBoltDamage = 5f;

    // Crafting 
    private bool reachedPotential = false;
    private int numCraftAttempts = 1;
    private PoisonVialStat maxStat;

    // Main public events to listen to
    public UnityEvent contaminateExecuteEvent = new UnityEvent();


    // Main constructor
    //  Pre: side effect doesn't equal null and startingStat is one of the stats in the enum
    //  Post: creates a poison vial with 1 in the startingStat and 0 everything else
    public PoisonVial(PoisonVialStat startingStat) {
        Debug.Assert(poisonVialConstants != null);

        sideEffect = poisonVialConstants.defaultSideEffect;
        vialStats = new Dictionary<PoisonVialStat, int>();
        ammo = STARTING_AMMO;
        maxStat = startingStat;

        foreach (PoisonVialStat stat in System.Enum.GetValues(typeof(PoisonVialStat))) {
            int statVal = (startingStat == stat) ? 1 : 0;
            vialStats.Add(stat, statVal);
        }
    }


    // Main function to get poison damage
    public float getPoisonDamage(int numStacks) {
        return POISON_DMG_PER_STACK * numStacks;
    }


    // Main accessor function to get the ammo of this vial
    public int getAmmo() {
        return ammo;
    }



    // ---------------------------------------------------------------------
    //  PRIMARY ATTACK
    // ---------------------------------------------------------------------


    // Main accessor function to get the start-up frames for the primary attack
    public int getPrimaryAttackStartFrames() {
        return sideEffect.primaryAttackStartFrames;
    }


    // Main accessor function to get the start-up frames for the primary attack
    public int getPrimaryAttackEndFrames() {
        return sideEffect.primaryAttackEndFrames;
    }


    // Main fucntion to actually create and fire a projectile at attackDir direction from attacker's position
    //  Pre: attackDir is the direction of attack, attacker is the transform of the one attacking
    //  Post: returns true if projectile fires. Returns false if it didn't due to ammo constraints
    public bool firePrimaryAttack(Vector3 attackDir, Transform attacker) {
        // If not enough ammo, return
        if (ammo < PRIMARY_BOLT_AMMO_COST) {
            return false;
        }

        ammo -= PRIMARY_BOLT_AMMO_COST;
        sideEffect.firePrimaryAttack(attackDir, attacker, startingBoltDamage, this);
        return true;
    }



    // ---------------------------------------------------------------------
    //  SECONDARY ATTACK
    // ---------------------------------------------------------------------


    // Main accessor function to get the start-up frames for the primary attack
    public int getSecondaryAttackStartFrames() {
        return sideEffect.secondaryAttackStartFrames;
    }


    // Main accessor function to get the start-up frames for the primary attack
    public int getSecondaryAttackEndFrames() {
        return sideEffect.secondaryAttackEndFrames;
    }


    // Main fucntion to actually create and fire a projectile at attackDir direction from attacker's position
    //  Pre: attackDir is the direction of attack, attacker is the transform of the one attacking
    //  Post: returns true if projectile fires. Returns false if it didn't due to ammo constraints
    public bool fireSecondaryAttack(Vector3 tgtPos, Transform attacker) {
        // If not enough ammo, return
        if (ammo < sideEffect.secondaryAttackCost) {
            return false;
        }

        ammo -= sideEffect.secondaryAttackCost;
        sideEffect.fireSecondaryAttack(tgtPos, attacker, this);
        return true;
    }


    // Main function to check if you can actually fire secondary projectile
    //  Pre: none
    //  Post: returns whether or not you can fire secondary projectile
    public bool canFireSecondaryLob() {
        return ammo >= sideEffect.secondaryAttackCost;
    }


    // Main function to get the secondary attack cooldown
    //  Pre: none
    //  Post: returns the cooldown for the secondary attack
    public float getSecondaryAttackCooldown() {
        return sideEffect.secondaryAttackCooldown;
    }



    // ------------------------------
    // CONTAMINATE
    // ------------------------------


    // Function to do contamination on target
    //  Pre: tgt != null, poisonStacks > 0
    //  Post: applies contaminate damage to target. invoke contaminate execute event if it kills
    public void contaminate(EnemyStatus tgt, int poisonStacks) {
        Debug.Assert(tgt != null && poisonStacks > 0);

        float contaminateDmg = CONTAMINATE_DMG_PER_STACK * poisonStacks;
        if (tgt.damage(contaminateDmg, false)) {
            contaminateExecuteEvent.Invoke();
        }
    }


    // ------------------------------
    // CRAFTING
    // ------------------------------

    // Main function to craft the vial
    //  Pre: stat is one of the 4 available stats
    //  Post: returns true if crafting is successful. returns 
    public bool craft(PoisonVialStat stat) {
        if (numCraftAttempts >= MAX_CRAFT_ATTEMPTS) {
            return false;
        }

        // Increase number of attempts and 
        numCraftAttempts++;
        ammo = Mathf.Min(ammo + AMMO_GAIN_PER_INGREDIENT, MAX_AMMO);

        // If potential not reached yet, increase stat
        if (!reachedPotential) {
            vialStats[stat]++;

            // Update maxStat if new max stat found
            if (vialStats[stat] > vialStats[maxStat]) {
                maxStat = stat;
            }

            // Update reachedPotential flag if vial stat has reached max stacks
            reachedPotential = vialStats[stat] >= MAX_STAT;
        }

        return true;
    }


    // Main function to get the vial's current color
    public Color getColor() {
        // If reached potential already, just return the pure color
        if (reachedPotential) {
            return poisonVialConstants.getPureColor(maxStat);

        // Else, just return an intepolation between tempColor and baseVialColor
        } else {
            return Color.Lerp(poisonVialConstants.baseVialColor, poisonVialConstants.getTempColor(maxStat), (float)vialStats[maxStat] / (float)(MAX_STAT - 1));

        }
    }


    // Main function to display info
    public void displayInfo(TMP_Text sideEffectSlot, Image sideEffectButton, PoisonCompositionDisplay statDisplay) {
        statDisplay.displayComposition(vialStats);
        sideEffectSlot.text = sideEffect.displayName;
        sideEffectButton.sprite = (sideEffect != poisonVialConstants.defaultSideEffect) ? sideEffect.spriteIcon : null;
        sideEffectButton.gameObject.SetActive(sideEffect != poisonVialConstants.defaultSideEffect);
    }
}
