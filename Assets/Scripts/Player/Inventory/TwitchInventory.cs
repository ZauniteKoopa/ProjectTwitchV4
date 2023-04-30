using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class TwitchInventory : MonoBehaviour
{
    // Poison Vial Info
    [SerializeField]
    private PoisonVialConstants poisonVialParameters;
    private PoisonVial primaryVial;
    private PoisonVial secondaryVial;

    // Cooldown info
    [SerializeField]
    [Min(0.1f)]
    private float contaminateCooldown = 8f;
    [SerializeField]
    [Min(0.1f)]
    private float ambushCooldown = 12f;
    private Coroutine runningCaskCooldownSequence = null;
    private Coroutine runningContaminateCooldownSequence = null;
    private Coroutine runningAmbushCooldownSequence = null;

    // Crafting
    [SerializeField]
    private PlayerInput playerInput;
    [SerializeField]
    [Min(0.1f)]
    private float craftingDuration = 1.5f; 
    private Dictionary<PoisonVialStat, int> ingredientInventory = new Dictionary<PoisonVialStat, int>();
    private RecipeBook recipeBook = new RecipeBook();
    private int numIngredients = 0;
    private int curMaxInventory = 3;
    private Coroutine runningCraftingSequence = null;

    // UI
    [SerializeField]
    private PlayerScreenUI screenUI;
    [SerializeField]
    private MainInventoryUI inventoryUI;


    // Start is called before the first frame update
    void Awake()
    {
        if (poisonVialParameters == null) {
            Debug.LogError("POISON VIAL CONSTANTS AND PARAMETERS NOT SET FOR TWITCH TO CRAFT POISONS");
        }

        PoisonVial.poisonVialConstants = poisonVialParameters;

        initializeIngredientDictionary();
        resetScreenUI();

        primaryVial = new PoisonVial(PoisonVialStat.POTENCY);
        primaryVial.contaminateExecuteEvent.AddListener(onAmbushReset);
        screenUI.displayPrimaryVial(primaryVial);
    }


    // Main function to reset ScreenUI variables: cooldown status and vial status
    private void resetScreenUI() {
        screenUI.displayAmbushCooldown(1f);
        screenUI.displayContaminateCooldown(1f);
        screenUI.displayCaskCooldown(1f);

        screenUI.displayPrimaryVial(null);
        screenUI.displaySecondaryVial(null);
    }


    // Main function to initialize the ingredient dictionary
    private void initializeIngredientDictionary() {
        if (ingredientInventory.Count <= 0) {
            ingredientInventory.Add(PoisonVialStat.POTENCY, 1);
            ingredientInventory.Add(PoisonVialStat.POISON, 0);
            ingredientInventory.Add(PoisonVialStat.REACTIVITY, 2);
            ingredientInventory.Add(PoisonVialStat.STICKINESS, 0);

            numIngredients = 3;
        }
    }


    // Main function to check if you're carrying a vial or not
    public bool carryingPrimaryVial() {
        return primaryVial != null;
    }



    // ---------------------------------------
    //  PRIMARY ATTACK
    // ---------------------------------------


    // Main function to get primary variable frame data
    public int getPrimaryStartFrame() {
        return primaryVial.getPrimaryAttackStartFrames();
    }


    // Main function to get primary variable frame data
    public int getPrimaryEndFrame() {
        return primaryVial.getPrimaryAttackEndFrames();
    }


    // Main function to fire the primary vial if it's possible
    //  Pre: the attackDirection is the direction of attack
    //  Post: returns true if successful. false otherwise
    public bool firePrimaryBolt(Vector3 attackDir, Transform attacker) {
        if (primaryVial == null) {
            return false;
        }

        // Fire bullet and then check ammo afterwards
        bool success = primaryVial.firePrimaryAttack(attackDir, attacker);
        if (success && primaryVial.getAmmo() <= 0) {
            primaryVial = null;
        }

        screenUI.displayPrimaryVial(primaryVial);
        return success;
    }


    // ---------------------------------------
    //  SECONDARY ATTACK
    // ---------------------------------------


    // Main function to get secondary variable frame data
    public int getSecondaryAttackStartFrames() {
        return primaryVial.getSecondaryAttackStartFrames();
    }


    // Main function to get primary variable frame data
    public int getSecondaryAttackEndFrames() {
        return primaryVial.getSecondaryAttackEndFrames();
    }


    // Main function to fire the primary vial if it's possible
    //  Pre: the attackDirection is the direction of attack
    //  Post: returns true if successful. false otherwise
    public bool fireSecondaryLob(Vector3 tgtPos, Transform attacker) {
        if (primaryVial == null && runningCaskCooldownSequence != null) {
            return false;
        }

        // Fire bullet and then check ammo afterwards
        bool success = primaryVial.fireSecondaryAttack(tgtPos, attacker);
        if (success) {
            runningCaskCooldownSequence = StartCoroutine(caskCooldownSequence(primaryVial.getSecondaryAttackCooldown()));

            if (primaryVial.getAmmo() <= 0) {
                primaryVial = null;
            }
        }

        screenUI.displayPrimaryVial(primaryVial);
        return success;
    }


    // Main function to check if you can actually fire secondary attack
    public bool canFireSecondaryLob() {
        return primaryVial != null && primaryVial.canFireSecondaryLob() && runningCaskCooldownSequence == null;
    }

    
    // Main function to do the secondary vial cooldown sequence
    //  Pre: curCooldown of the cask. > 0f
    private IEnumerator caskCooldownSequence(float curCooldown) {
        Debug.Assert(curCooldown > 0f);

        // Setup
        float timer = 0f;
        screenUI.displayCaskCooldown(0f);

        // Loop
        while (timer < curCooldown) {
            yield return 0;
            timer += Time.deltaTime;

            screenUI.displayCaskCooldown(timer / curCooldown);
        }

        runningCaskCooldownSequence = null;
    }



    // ---------------------------------------
    //  Contaminate
    // ---------------------------------------


    // Main sequence for the cooldown sequence for contamination
    private IEnumerator contaminateCooldownSequence() {
        float timer = 0f;
        screenUI.displayContaminateCooldown(0f);

        while (timer <= contaminateCooldown) {
            yield return 0;
            timer += Time.deltaTime;

            screenUI.displayContaminateCooldown(timer / contaminateCooldown);
        }

        runningContaminateCooldownSequence = null;
    }


    // Main public function to check if you can conatminate (not on cooldown)
    public bool canContaminate() {
        return runningContaminateCooldownSequence == null;
    }


    // Main public function to activate contamination cooldown
    //  If already activated, don't do anything
    public void activateContaminationCooldown() {
        if (runningContaminateCooldownSequence == null) {
            runningContaminateCooldownSequence = StartCoroutine(contaminateCooldownSequence());
        }
    }



    // ---------------------------------------
    //  Ambush
    // ---------------------------------------


    // Main sequence for the cooldown sequence for contamination
    private IEnumerator ambushCooldownSequence() {
        float timer = 0f;
        screenUI.displayAmbushCooldown(0f);

        while (timer <= ambushCooldown) {
            yield return 0;
            timer += Time.deltaTime;

            screenUI.displayAmbushCooldown(timer / ambushCooldown);
        }

        runningAmbushCooldownSequence = null;
    }


    // Main public function to check if you can conatminate (not on cooldown)
    public bool canAmbush() {
        return runningAmbushCooldownSequence == null;
    }


    // Main public function to activate contamination cooldown
    //  If already activated, don't do anything
    public void activateAmbushCooldown() {
        if (runningAmbushCooldownSequence == null) {
            runningAmbushCooldownSequence = StartCoroutine(ambushCooldownSequence());
        }
    }


    // Main event handler function to reset ambush cooldown
    private void onAmbushReset() {
        if (runningAmbushCooldownSequence != null) {
            StopCoroutine(runningAmbushCooldownSequence);
            runningAmbushCooldownSequence = null;

            screenUI.displayAmbushCooldown(1f);
        }
    }



    // ---------------------------------------
    //  Vial and Ingredient Management
    // ---------------------------------------


    // Main function to swap vials
    public void swapVials() {
        PoisonVial tempVial = primaryVial;
        primaryVial = secondaryVial;
        secondaryVial = tempVial;

        screenUI.displayPrimaryVial(primaryVial);
        screenUI.displaySecondaryVial(secondaryVial);
    }


    // Main event handler for opening the inventory menu
    public void onOpenInventoryAction(InputAction.CallbackContext value) {
        if (value.started && !inventoryUI.isMenuOpen() && runningCraftingSequence == null) {
            playerInput.enabled = false;
            inventoryUI.open(ingredientInventory, curMaxInventory, primaryVial, secondaryVial, recipeBook);
        }
    }


    // Main event handler function for when the menu was closed directly
    public void onInventoryMenuClose() {
        playerInput.enabled = true;
    }


    // Main event handler function for when the inventory menu closes because the player wants to craft
    public void onInventoryMenuCraft(CraftParameters craftParameters) {
        if (runningCraftingSequence == null) {
            runningCraftingSequence = StartCoroutine(craftingSequence(craftParameters));
        }
    }


    // Sequence for an actual successful craft
    //  Pre: craftParameters != null
    private IEnumerator craftingSequence(CraftParameters craftParameters) {
        Debug.Assert(craftParameters != null);

        yield return new WaitForSeconds(craftingDuration);

        // Actually update the vials in the case vial exist
        if (craftParameters.vial != null) {
            bool success = craftParameters.vial.craft(craftParameters.stat);
            Debug.Assert(success);

        // Case where the parameters isn't pointing to a vial but isPrimary is true
        } else if (craftParameters.isPrimary) {
            primaryVial = new PoisonVial(craftParameters.stat);

        // Case where its not pointing to a vial and isPrimary is false
        } else {
            secondaryVial = new PoisonVial(craftParameters.stat);

        }

        // Update ingredient count
        Debug.Assert(ingredientInventory[craftParameters.stat] > 0);
        ingredientInventory[craftParameters.stat]--;

        // Update flags
        playerInput.enabled = true;
        runningCraftingSequence = null;

        screenUI.displayPrimaryVial(primaryVial);
        screenUI.displaySecondaryVial(secondaryVial);
    }
    
}
