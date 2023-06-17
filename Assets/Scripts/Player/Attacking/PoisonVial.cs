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
    private static readonly float POISON_DMG_PER_STACK = 0.1f;
    private static readonly float BASE_CONTAMINATE_DMG = 5.5f;
    private static readonly float CONTAMINATE_DMG_PER_STACK = 1f;

    private static readonly int AMMO_GAIN_PER_INGREDIENT = 10;
    private static readonly int MAX_STAT = 3;
    private static readonly int MAX_CRAFT_ATTEMPTS = 8;
    public static readonly int MAX_AMMO = 60;
    public static PoisonVialConstants poisonVialConstants;

    // Main vial stats
    private Dictionary<PoisonVialStat, int> vialStats;
    private int ammo;
    public SideEffect sideEffect;
    private float startingBoltDamage = 2.5f;

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
        float basePoisonDamage = POISON_DMG_PER_STACK * numStacks;
        basePoisonDamage *= (reachedPotential && sideEffect.getType() == PoisonVialStat.POISON) ? poisonVialConstants.poisonDoTMultiplier : 1f;
        return POISON_DMG_PER_STACK * numStacks;
    }


    // Main function to get speed modifier
    public float getSpeedModifier(int numStacks) {
        float curModifier = 1f;

        if (reachedPotential && sideEffect.getType() == PoisonVialStat.STICKINESS) {
            curModifier -= (numStacks * poisonVialConstants.stickinessMovementReductionModifier);
        }

        return curModifier;
    }


    // Main function to get the decay rate of poison (second per tick)
    public float getPoisonDecayRate(int numStacks) {
        return poisonVialConstants.secondsPerPoisonTick * sideEffect.getPoisonDecayRateModifier(numStacks);
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
        return sideEffect.getPrimaryAttackStartFrames();
    }


    // Main accessor function to get the start-up frames for the primary attack
    public int getPrimaryAttackEndFrames() {
        return sideEffect.getPrimaryAttackEndFrames();
    }


    // Main fucntion to actually create and fire a projectile at attackDir direction from attacker's position
    //  Pre: attackDir is the direction of attack, attacker is the transform of the one attacking
    //  Post: returns true if projectile fires. Returns false if it didn't due to ammo constraints
    public bool firePrimaryAttack(Vector3 attackDir, Transform attacker, float attackMultiplier = 1f) {
        // If not enough ammo, return
        if (ammo < PRIMARY_BOLT_AMMO_COST) {
            return false;
        }

        ammo -= PRIMARY_BOLT_AMMO_COST;
        float curBoltDamage = startingBoltDamage * attackMultiplier;
        curBoltDamage *= (reachedPotential && sideEffect.getType() == PoisonVialStat.POTENCY) ? poisonVialConstants.potencyBoltMultiplier : 1f;
        sideEffect.firePrimaryAttack(attackDir, attacker, curBoltDamage, this);
        return true;
    }



    // ---------------------------------------------------------------------
    //  SECONDARY ATTACK
    // ---------------------------------------------------------------------


    // Main accessor function to get the start-up frames for the primary attack
    public int getSecondaryAttackStartFrames() {
        return sideEffect.getSecondaryAttackStartFrames();
    }


    // Main accessor function to get the start-up frames for the primary attack
    public int getSecondaryAttackEndFrames() {
        return sideEffect.getSecondaryAttackEndFrames();
    }


    // Main fucntion to actually create and fire a projectile at attackDir direction from attacker's position
    //  Pre: attackDir is the direction of attack, attacker is the transform of the one attacking
    //  Post: returns true if projectile fires. Returns false if it didn't due to ammo constraints
    public bool fireSecondaryAttack(Vector3 tgtPos, Transform attacker) {
        // If not enough ammo, return
        if (ammo < sideEffect.getSecondaryAttackCost()) {
            return false;
        }

        ammo -= sideEffect.getSecondaryAttackCost();
        sideEffect.fireSecondaryAttack(tgtPos, attacker, this);
        return true;
    }


    // Main function to check if you can actually fire secondary projectile
    //  Pre: none
    //  Post: returns whether or not you can fire secondary projectile
    public bool canFireSecondaryLob() {
        return ammo >= sideEffect.getSecondaryAttackCost();
    }


    // Main function to get the secondary attack cooldown
    //  Pre: none
    //  Post: returns the cooldown for the secondary attack
    public float getSecondaryAttackCooldown() {
        return sideEffect.getSecondaryAttackCooldown();
    }



    // ------------------------------
    // CONTAMINATE
    // ------------------------------


    // Function to do contamination on target
    //  Pre: tgt != null, poisonStacks > 0
    //  Post: applies contaminate damage to target. invoke contaminate execute event if it kills
    public void contaminate(EnemyStatus tgt, int poisonStacks) {
        Debug.Assert(tgt != null && poisonStacks > 0);

        Vector3 enemyPosition = tgt.transform.position;

        float contaminateDmg = (CONTAMINATE_DMG_PER_STACK * poisonStacks) + BASE_CONTAMINATE_DMG;
        contaminateDmg *= (reachedPotential && sideEffect.getType() == PoisonVialStat.REACTIVITY) ? poisonVialConstants.reactivityContaminateMultiplier : 1f;
        bool tgtKilled = tgt.damage(contaminateDmg, false);

        if (tgtKilled) {
            contaminateExecuteEvent.Invoke();
        }

        // Spawn a post contaminate side effect if it exists for this side effect
        if (sideEffect.postContaminateHitbox != null && poisonStacks >= poisonVialConstants.minPostContaminateStacks) {
            PostContaminateHitbox curHitbox = Object.Instantiate(sideEffect.postContaminateHitbox, enemyPosition, Quaternion.identity);
            curHitbox.setUp(contaminateDmg, this, tgtKilled);
        }
    }


    // ------------------------------
    // CRAFTING
    // ------------------------------

    // Main function to craft the vial
    //  Pre: stat is one of the 4 available stats
    //  Post: returns true if crafting is successful. returns 
    public bool craft(PoisonVialStat stat, RecipeBook recipeBook) {
        if (numCraftAttempts >= MAX_CRAFT_ATTEMPTS) {
            return false;
        }

        // Increase number of attempts and 
        numCraftAttempts++;
        ammo = Mathf.Min(ammo + AMMO_GAIN_PER_INGREDIENT, MAX_AMMO);

        // If potential not reached yet, increase stat
        if (!reachedPotential) {
            // Update maxStat if new max stat found
            vialStats[stat]++;
            if (vialStats[stat] > vialStats[maxStat]) {
                maxStat = stat;
            }

            // Update reachedPotential flag if vial stat has reached max stacks
            if (vialStats[stat] >= MAX_STAT) {
                reachedPotential = true;

                bool filledRecipe = (numCraftAttempts == RecipeBook.RECIPE_INGREDIENT_REQUIREMENTS);
                sideEffect = filledRecipe ? recipeBook.createSideEffectFromRecipe(vialStats, stat) : null;
                if (sideEffect == null) {
                    sideEffect = poisonVialConstants.obtainSideEffect(stat);
                    Recipe existingRecipe = recipeBook.jumpToSideEffect(sideEffect);

                    if (existingRecipe == null) {
                        Recipe newRecipe = new Recipe();
                        newRecipe.ingredients = (filledRecipe) ? vialStats : null;
                        newRecipe.resultingSideEffect = sideEffect;

                        recipeBook.addNewRecipe(newRecipe);

                    } else if (filledRecipe) {
                        existingRecipe.ingredients = vialStats;
                    }
                }
            }
        }

        return true;
    }


    // Main function to check if you can craft
    public bool canCraft() {
        return numCraftAttempts < MAX_CRAFT_ATTEMPTS;
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
    public void displayInfo(TMP_Text upgradesLeftText, Image sideEffectButton, PoisonCompositionDisplay statDisplay) {
        statDisplay.displayComposition(vialStats);
        upgradesLeftText.text = "Upgrades: " + numCraftAttempts + "/" + MAX_CRAFT_ATTEMPTS;
        upgradesLeftText.color = (numCraftAttempts < MAX_CRAFT_ATTEMPTS) ? Color.black : Color.red;
        sideEffectButton.sprite = (sideEffect != poisonVialConstants.defaultSideEffect) ? sideEffect.spriteIcon : null;
        sideEffectButton.gameObject.SetActive(sideEffect != poisonVialConstants.defaultSideEffect);
    }
}
