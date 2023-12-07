using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu]
public class PoisonVialConstants : ScriptableObject
{
    [Header("Pure Vial Colors")]
    public Color purePotencyColor = Color.red;
    public Color purePotencyColor2 = Color.magenta;

    public Color pureReactivityColor = Color.yellow;
    public Color pureReactivityColor2 = Color.black;

    public Color pureStickinessColor = Color.blue;
    public Color pureStickinessColor2 = Color.black;
    
    public Color purePoisonColor = Color.green;
    public Color purePoisonColor2 = Color.green;


    [Header("In-Progress Vial Colors")]
    public Color tempPotencyColor = Color.red;
    public Color tempReactivityColor = Color.yellow;
    public Color tempStickinessColor = Color.blue;
    public Color tempPoisonColor = Color.green;
    public Color baseVialColor = Color.grey;


    [Header("Poisoning Variables")]
    public float secondsPerPoisonTick = 1f;


    [Header("Side Effects")]
    public SideEffect defaultSideEffect;
    [SerializeField]
    private SideEffect[] potencySideEffects;
    [SerializeField]
    private SideEffect[] poisonSideEffects;
    [SerializeField]
    private SideEffect[] reactivitySideEffects;
    [SerializeField]
    private SideEffect[] stickinessSideEffects;
    private Dictionary<PoisonVialStat, SideEffect[]> sideEffectDictionary = null;
    private List<SideEffect> sideEffectList = null;

    [Header("Default Primary Attack")]
    [SerializeField]
    private IPrimaryAttack primaryAttackPrefab;
    [SerializeField]
    [Min(1)]
    public int primaryAttackStartFrames = 10;
    [SerializeField]
    [Min(1)]
    public int primaryAttackEndFrames = 20;
    [SerializeField]
    [Min(0.1f)]
    private float primaryAttackMultiplier = 1.0f;
    [SerializeField]
    [Min(1f)]
    public float attackRange = 6f;
    [SerializeField]
    private AudioClip[] primaryAttackSoundEffects = null;

    [Header("Secondary Attack")]
    [SerializeField]
    private LobAction secondaryAttackPrefab;
    [SerializeField]
    [Min(0.01f)]
    private float secondaryAttackSpeed = 20f;
    [SerializeField]
    [Min(1)]
    public int secondaryAttackStartFrames = 15;
    [SerializeField]
    [Min(1)]
    public int secondaryAttackEndFrames = 10;
    [SerializeField]
    [Min(1)]
    public int secondaryAttackCost = 3;
    [SerializeField]
    [Min(0.1f)]
    public float secondaryAttackCooldown = 6f;
    [TextArea]
    public string defaultVenomCaskDescription;

    [Header("Default Side Effect Multipliers")]
    [Min(0.1f)]
    public float potencyBoltMultiplier = 1.5f;
    [Min(0.1f)]
    public float poisonDoTMultiplier = 2f;
    [Min(0.1f)]
    public float reactivityContaminateMultiplier = 1.25f;
    [Range(0f, 0.15f)]
    public float stickinessMovementReductionModifier = 0.05f;

    [Header("Post Contaminate Hitbox")]
    [Min(0)]
    public float minPostContaminateStacks = 4;
    


    
    // Main function to get the pure color
    public Color getPureColor(PoisonVialStat stat, bool usingSecondColor) {
        switch(stat)
        {
            case PoisonVialStat.POTENCY:
                return (usingSecondColor) ? purePotencyColor2 : purePotencyColor;
            
            case PoisonVialStat.POISON:
                return (usingSecondColor) ? purePoisonColor2 : purePoisonColor;

            case PoisonVialStat.REACTIVITY:
                return (usingSecondColor) ? pureReactivityColor2 : pureReactivityColor;

            case PoisonVialStat.STICKINESS:
                return (usingSecondColor) ? pureStickinessColor2 : pureStickinessColor;

            default:
                throw new System.Exception("INVALID STAT FOUND");
        }
    }


    // Main function to get the temp color
    public Color getTempColor(PoisonVialStat stat) {
        switch(stat)
        {
            case PoisonVialStat.POTENCY:
                return tempPotencyColor;
            
            case PoisonVialStat.POISON:
                return tempPoisonColor;

            case PoisonVialStat.REACTIVITY:
                return tempReactivityColor;

            case PoisonVialStat.STICKINESS:
                return tempStickinessColor;

            default:
                throw new System.Exception("INVALID STAT FOUND");
        }
    }


    // Main function to obtain a side effect of a certain type:
    public SideEffect obtainSideEffect(PoisonVialStat specialization, RecipeBook recipeBook, bool prioritizeNewRecipe) {
        if (sideEffectDictionary == null) {
            initializeSideEffectDictionary();
        }

        // Pick the side effect specialization and just pick a random index
        SideEffect[] sideEffectList = sideEffectDictionary[specialization];
        Debug.Assert(sideEffectList.Length > 0);
        int curIndex = Random.Range(0, sideEffectList.Length);

        // If prioritizeNewRecipe and recipe book specialization section is not filled out, find a side effect the recipe book doesn't have
        if (prioritizeNewRecipe && recipeBook.canAddNewRecipe(specialization)) {
            bool moveForward = (Random.Range(0, 2) == 0);
            bool recipeBookHasSideEffect = recipeBook.containsSideEffect(sideEffectList[curIndex]);

            while (recipeBookHasSideEffect) {
                // Increment curIndex
                curIndex += (moveForward) ? 1 : (sideEffectList.Length - 1);
                curIndex %= sideEffectList.Length;

                // Update while loop condition
                recipeBookHasSideEffect = recipeBook.containsSideEffect(sideEffectList[curIndex]);
            }
        }

        // Return recipe at cur index
        return sideEffectList[curIndex];
    }


    // Private helper function to initialize the dictionary
    private void initializeSideEffectDictionary() {
        Debug.Assert(sideEffectDictionary == null);

        // Init dict
        sideEffectDictionary = new Dictionary<PoisonVialStat, SideEffect[]>();
        sideEffectDictionary.Add(PoisonVialStat.POTENCY, potencySideEffects);
        sideEffectDictionary.Add(PoisonVialStat.POISON, poisonSideEffects);
        sideEffectDictionary.Add(PoisonVialStat.REACTIVITY, reactivitySideEffects);
        sideEffectDictionary.Add(PoisonVialStat.STICKINESS, stickinessSideEffects);

        // Init list
        sideEffectList = new List<SideEffect>();
        foreach(KeyValuePair<PoisonVialStat, SideEffect[]> entry in sideEffectDictionary) {
            foreach (SideEffect effect in entry.Value) {
                sideEffectList.Add(effect);
            }
        }
    }


    // Main function to fire default primary attack
    //  Pre: attackDir is the direction you attack to, attacker is the transform of the unit that's attacking
    //  Post: ALWAYS fires the attack. (Please check conditions before doing this)
    public void fireDefaultPrimaryPoisonAttack(Vector3 attackDir, Transform attacker, float damage, PoisonVial parentPoison) {
        Debug.Assert(attacker != null && primaryAttackPrefab != null);

        IPrimaryAttack curBolt = Object.Instantiate(primaryAttackPrefab, attacker.position, Quaternion.identity);
        curBolt.setUp(attackDir, damage * primaryAttackMultiplier, parentPoison, attackRange);
    }


    // Main function to get a random primary attack sound effect clip
    public AudioClip getDefaultPrimaryAttackSound() {
        Debug.Assert(primaryAttackSoundEffects != null && primaryAttackSoundEffects.Length > 0);
        return primaryAttackSoundEffects[Random.Range(0, primaryAttackSoundEffects.Length)];
    }


    // Main function to fire default secondary attack
    //  Pre: tgtPos is position within game, poisonVial is the poison associated with lob, and attack is the one sending out attack
    //  Post: fires secondary attack
    public void fireDefaultSecondaryAttack(Vector3 tgtPos, Transform attacker, PoisonVial parentPoison, float attackModifier) {
        Debug.Assert(attacker != null && parentPoison != null && secondaryAttackPrefab != null);

        LobAction curLob = Object.Instantiate(secondaryAttackPrefab, attacker.position, Quaternion.identity);
        curLob.lob(attacker.position, tgtPos, secondaryAttackSpeed, parentPoison, attacker.parent);
    }


    // Main function to obtain a random side effect based on a recipe book
    public SideEffect obtainRandomSideEffect(RecipeBook recipeBook, List<Recipe> excluded) {
        // Initialize
        if (sideEffectDictionary == null) {
            initializeSideEffectDictionary();
        }

        // Only do it if recipes found < total side effects possible
        if (recipeBook.getTotalCompletedRecipes() < sideEffectList.Count) {
            // Set up
            int curIndex = Random.Range(0, sideEffectList.Count);
            bool recipeBookHasSideEffect = recipeBook.containsSideEffect(sideEffectList[curIndex]);
            bool excludedHasSideEffect = excluded.FindIndex(f => f.resultingSideEffect == sideEffectList[curIndex]) >= 0;

            while (recipeBookHasSideEffect || excludedHasSideEffect) {
                // Iterate through index
                curIndex = (curIndex + 1) % sideEffectList.Count;

                // Update boolean checkers
                recipeBookHasSideEffect = recipeBook.containsSideEffect(sideEffectList[curIndex]);
                excludedHasSideEffect = excluded.FindIndex(f => f.resultingSideEffect == sideEffectList[curIndex]) >= 0;
            }

            return sideEffectList[curIndex];
        } else {

            Debug.LogError("RECIPE BOOK IS ALREADY FULL! WHY ARE WE CALLING THIS??");
            return null;
        }
    }


    // Main function to get the total number of side effects in the game
    public int getTotalNumberOfSideEffects() {
        // Initialize
        if (sideEffectDictionary == null) {
            initializeSideEffectDictionary();
        }

        return sideEffectList.Count;
    }


    // Main function to get the total number of side effects within a specialization
    public int getTotalNumberOfSideEffects(PoisonVialStat specialization) {
        // Initialize
        if (sideEffectDictionary == null) {
            initializeSideEffectDictionary();
        }

        // Return the length of that section
        return sideEffectDictionary[specialization].Length;
    }


    // Main function to get the temp color
    public string getBaseSideEffectDescription(PoisonVialStat stat) {
        switch(stat)
        {
            case PoisonVialStat.POTENCY:
                return "+" + ((potencyBoltMultiplier * 100f) - 100f) + "% Bolt Dmg";
            
            case PoisonVialStat.POISON:
                return "+" + ((poisonDoTMultiplier * 100f) - 100f) + "% Poison Dmg";

            case PoisonVialStat.REACTIVITY:
                return "+" + ((reactivityContaminateMultiplier * 100f) - 100f) + "% Cont Dmg";

            case PoisonVialStat.STICKINESS:
                return "Applies " + (stickinessMovementReductionModifier * 100f) + "% Slow per stack";

            default:
                throw new System.Exception("INVALID STAT FOUND");
        }
    }
}
