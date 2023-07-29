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
    [SerializeField]
    private Transform attackRangeIndicator;
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
    private int curMaxInventory = 12;
    private Coroutine runningCraftingSequence = null;

    // UI
    [SerializeField]
    private PlayerScreenUI screenUI;
    [SerializeField]
    private MainInventoryUI inventoryUI;
    [SerializeField]
    private MeshRenderer playerMesh;
    [SerializeField]
    private Renderer[] primaryVialMeshes;
    [SerializeField]
    private Renderer[] secondaryVialMeshes;
    private Color originalMeshColor;

    // Error messages from inventory
    [SerializeField]
    private string abilityCooldownErrorMessage = "Ability is currently on cooldown";
    [SerializeField]
    private string noVialEquippedErrorMessage = "No vial currently equipped";
    [SerializeField]
    private string runOutOfAmmoMessage = "No ammo left for ability";
    [SerializeField]
    private string inventoryFilledErrorMessage = "No space in inventory";

    // Audio
    [SerializeField]
    private PlayerAudioManager twitchAudioManager;

    // Animator events
    public UnityEvent startCraftEvent;
    public UnityEvent endCraftEvent;
    public UnityEvent obtainedSideEffect;


    // Start is called before the first frame update
    void Start()
    {
        if (poisonVialParameters == null) {
            Debug.LogError("POISON VIAL CONSTANTS AND PARAMETERS NOT SET FOR TWITCH TO CRAFT POISONS");
        }

        PoisonVial.poisonVialConstants = poisonVialParameters;

        initializeIngredientDictionary();
        resetScreenUI();
        updateAttackRangeIndicator();

        // primaryVial = new PoisonVial(PoisonVialStat.POTENCY);
        // primaryVial.contaminateExecuteEvent.AddListener(onAmbushReset);
        updateVialDisplays();
        originalMeshColor = playerMesh.material.color;
        PauseConstraints.setInventoryModule(this);
    }


    // Main function to reset ScreenUI variables: cooldown status and vial status
    private void resetScreenUI() {
        screenUI.displayAmbushCooldown(1f);
        screenUI.displayContaminateCooldown(1f);
        screenUI.displayCaskCooldown(1f);

        updateVialDisplays();
    }


    // Main function to initialize the ingredient dictionary
    private void initializeIngredientDictionary() {
        if (ingredientInventory.Count <= 0) {
            ingredientInventory.Add(PoisonVialStat.POTENCY, 0);
            ingredientInventory.Add(PoisonVialStat.POISON, 0);
            ingredientInventory.Add(PoisonVialStat.REACTIVITY, 0);
            ingredientInventory.Add(PoisonVialStat.STICKINESS, 0);

            numIngredients = 0;
            curMaxInventory = 6;

            screenUI.displayIngredientInventory(ingredientInventory, curMaxInventory);
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


    // Main function to play the audio effect associated with this poison
    public void playLaunchPoisonBoltSound() {
        if (primaryVial != null) {
            twitchAudioManager.playLaunchPoisonBoltSound(primaryVial.sideEffect);
        } else {
            twitchAudioManager.playLaunchPoisonBoltSound();
        }
    }


    // Main function to update attack range indicator
    private void updateAttackRangeIndicator() {
        float curRange = (primaryVial != null) ? primaryVial.sideEffect.getAttackRange() : PoisonVial.poisonVialConstants.attackRange;
        curRange *= 2f;
        attackRangeIndicator.localScale = new Vector3(curRange, attackRangeIndicator.localScale.y, curRange);
    }


    // Main function to fire the primary vial if it's possible
    //  Pre: the attackDirection is the direction of attack
    //  Post: returns true if successful. false otherwise
    public bool firePrimaryBolt(Vector3 attackDir, Transform attacker, float primaryAttackModifier = 1f) {
        if (primaryVial == null) {
            return false;
        }

        // Fire bullet and then check ammo afterwards
        bool success = primaryVial.firePrimaryAttack(attackDir, attacker, primaryAttackModifier);
        if (success && primaryVial.getAmmo() <= 0) {
            primaryVial = null;
        } else if (!success) {
            screenUI.displayErrorMessage(runOutOfAmmoMessage);
        }

        updateVialDisplays();
        return success;
    }


    // Main function to access the primary attack animation
    public PrimaryAttackAnimation getPrimaryAttackAnimation() {
        return (primaryVial != null) ? primaryVial.sideEffect.getPrimaryAttackAnimation() : PrimaryAttackAnimation.NORMAL;
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
        if (primaryVial == null || runningCaskCooldownSequence != null) {
            string curError = (primaryVial == null) ? noVialEquippedErrorMessage : abilityCooldownErrorMessage;
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

        updateVialDisplays();
        return success;
    }


    // Main function to check if you can actually fire secondary attack
    public bool canFireSecondaryLob() {
        if (runningCaskCooldownSequence != null) {
            screenUI.displayErrorMessage(abilityCooldownErrorMessage);
        } else if (primaryVial == null) {
            screenUI.displayErrorMessage(noVialEquippedErrorMessage);
        } else if (!primaryVial.canFireSecondaryLob()) {
            screenUI.displayErrorMessage(runOutOfAmmoMessage);

        }

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
        if (runningContaminateCooldownSequence != null) {
            screenUI.displayErrorMessage(abilityCooldownErrorMessage);
        }

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
        if (runningAmbushCooldownSequence != null) {
            screenUI.displayErrorMessage(abilityCooldownErrorMessage);
        }
        
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

        updateVialDisplays();
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
        StartCoroutine(enableControls());
    }


    // Main accessor method to check if inventory menu is open
    public bool isInventoryMenuOpen() {
        return inventoryUI.isMenuOpen();
    }


    // Main sequence for when the inventory menu closed
    private IEnumerator enableControls() {
        yield return 0;
        playerInput.enabled = true;
    }


    // Main event handler function for when the inventory menu closes because the player wants to craft
    public void onInventoryMenuCraft(CraftParameters craftParameters) {
        if (runningCraftingSequence == null) {
            runningCraftingSequence = StartCoroutine(craftingSequence(craftParameters));
        }
    }


    // Main function to add ingredient to inventory. returns true if successful. false if not
    public bool addIngredient(PoisonVialStat vialStat) {
        if (numIngredients < curMaxInventory) {
            numIngredients++;
            ingredientInventory[vialStat]++;

            screenUI.displayIngredientInventory(ingredientInventory, curMaxInventory);

            return true;
        }

        screenUI.displayErrorMessage(inventoryFilledErrorMessage);
        return false;
    }


    // Main function to remove an ingredient from inventory. 
    public void removeIngredient(PoisonVialStat vialStat) {
        numIngredients--;
        ingredientInventory[vialStat]--;
        screenUI.displayIngredientInventory(ingredientInventory, curMaxInventory);
        inventoryUI.updateIngredients(ingredientInventory, curMaxInventory);
    }


    // Sequence for an actual successful craft
    //  Pre: craftParameters != null
    private IEnumerator craftingSequence(CraftParameters craftParameters) {
        Debug.Assert(craftParameters != null);

        playerMesh.material.color = Color.yellow;
        startCraftEvent.Invoke();

        yield return new WaitForSeconds(craftingDuration);

        playerMesh.material.color = Color.green;

        // Actually update the vials in the case vial exist
        if (craftParameters.vial != null) {
            // Check if you got a side effect before / after crafting
            bool gotSideEffectBefore = craftParameters.vial.sideEffect != PoisonVial.poisonVialConstants.defaultSideEffect;
            bool success = craftParameters.vial.craft(craftParameters.stat, recipeBook);
            bool gotSideEffectAfter = craftParameters.vial.sideEffect != PoisonVial.poisonVialConstants.defaultSideEffect;

            UnityEvent curEndEvent = (gotSideEffectBefore != gotSideEffectAfter) ? obtainedSideEffect : endCraftEvent;
            curEndEvent.Invoke();

        // Case where the parameters isn't pointing to a vial but isPrimary is true
        } else if (craftParameters.isPrimary) {
            primaryVial = new PoisonVial(craftParameters.stat);
            primaryVial.contaminateExecuteEvent.AddListener(onAmbushReset);
            endCraftEvent.Invoke();

        // Case where its not pointing to a vial and isPrimary is false
        } else {
            secondaryVial = new PoisonVial(craftParameters.stat);
            secondaryVial.contaminateExecuteEvent.AddListener(onAmbushReset);
            endCraftEvent.Invoke();
        }

        // Update ingredient count
        Debug.Assert(ingredientInventory[craftParameters.stat] > 0);
        ingredientInventory[craftParameters.stat]--;
        screenUI.displayIngredientInventory(ingredientInventory, curMaxInventory);
        numIngredients--;

        // Update flags
        playerInput.enabled = true;
        runningCraftingSequence = null;

        updateVialDisplays();
    }


    // Main private helper function to update both vial displays
    private void updateVialDisplays() {
        screenUI.displayPrimaryVial(primaryVial);
        screenUI.displaySecondaryVial(secondaryVial);
        updateAttackRangeIndicator();

        foreach (Renderer primaryVialMesh in primaryVialMeshes) {
            primaryVialMesh.enabled = (primaryVial != null);

            if (primaryVial != null) {
                primaryVialMesh.material.color = primaryVial.getColor();
            }
        }

        foreach (Renderer secondaryVialMesh in secondaryVialMeshes) {
            secondaryVialMesh.enabled = (secondaryVial != null);

            if (secondaryVial != null) {
                secondaryVialMesh.material.color = secondaryVial.getColor();
            }
        }
    }


    // Main function to add a new recipe to the recipe book
    public void addRandomRecipe() {
        if (recipeBook.canAddNewRecipe()) {
            recipeBook.addRandomRecipe();
        }
    }
    
}
