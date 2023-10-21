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
    [SerializeField]
    private PlayerStatus twitchStatus;
    private PoisonVial primaryVial;
    private PoisonVial secondaryVial;

    // Cooldown info
    [SerializeField]
    [Min(0.1f)]
    private float contaminateCooldown = 8f;
    [SerializeField]
    [Min(0.1f)]
    private float fullAmbushDuration = 6f;
    [SerializeField]
    [Min(0.1f)]
    private float minAmbushDurationRequirement = 2f;
    [SerializeField]
    [Min(0.1f)]
    private float ambushRegenMax = 3f;
    [SerializeField]
    [Min(0.1f)]
    private float ambushRegenCooldown = 8f;
    [SerializeField]
    [Min(0.1f)]
    private float onEnemyDeathAmbushRegen = 1f;
    private float curAmbushDuration;
    private bool isAmbushing;

    private float curCaskCooldown;
    private float curCaskCooldownTimer;
    private bool displaySecondaryAttackErrorMessage = true;
    
    private Coroutine runningCaskCooldownSequence = null;
    private Coroutine runningContaminateCooldownSequence = null;

    // Crafting
    [SerializeField]
    private PlayerInput playerInput;
    [SerializeField]
    [Min(0.1f)]
    private float craftingDuration = 1.5f; 
    [SerializeField]
    [Min(0)]
    private int numStartingRecipes = 3;
    [SerializeField]
    [Range(1, 16)]
    private int numStartingIngredientSlots = 4;
    [SerializeField]
    [Range(0f, 1f)]
    private float surplusDefinition = 0.8f;
    [SerializeField]
    [Range(0f, 1f)]
    private float scarcityDefinition = 0f;

    private Dictionary<PoisonVialStat, int> ingredientInventory = new Dictionary<PoisonVialStat, int>();
    private RecipeBook recipeBook = new RecipeBook();
    private int numIngredients = 0;
    private int curMaxInventory;

    private const int MAX_INVENTORY_SLOTS = 16;
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

    // Onboarding events
    public UnityEvent firstIngredientGainEvent;
    public UnityEvent firstPoisonVialEvent;
    private bool haveGottenIngredients = false;
    private bool haveMadePoisonVials = false;

    // Start is called before the first frame update
    void Awake()
    {
        if (poisonVialParameters == null) {
            Debug.LogError("POISON VIAL CONSTANTS AND PARAMETERS NOT SET FOR TWITCH TO CRAFT POISONS");
        }

        if (scarcityDefinition >= surplusDefinition) {
            Debug.LogError("SCARCITY DEFINITION SHOULD BE LESS THAN SURPLUS TO MODIFY PROBABILITY");
        }

        PoisonVial.poisonVialConstants = poisonVialParameters;
        curAmbushDuration = fullAmbushDuration;
        isAmbushing = false;

        initializeIngredientDictionary();
        resetScreenUI();
        updateAttackRangeIndicator();

        updateVialDisplays();
        originalMeshColor = playerMesh.material.color;
        PauseConstraints.setInventoryModule(this);

        addRandomRecipes(numStartingRecipes);
    }


    // On update, set ambush bar state
    private void Update() {
        float deltaTime = Time.deltaTime;

        // If you are ambushing and you still have cooldown, have the ambush bar go down
        if (isAmbushing && curAmbushDuration > 0f) {
            curAmbushDuration = Mathf.Max(0f, curAmbushDuration - deltaTime);
            screenUI.setInvisBarFill(curAmbushDuration, fullAmbushDuration, minAmbushDurationRequirement);

        // If not ambushing and not at minAmbushDuration requirement
        } else if (!isAmbushing && curAmbushDuration < ambushRegenMax) {
            float addedTime = deltaTime * (ambushRegenMax / ambushRegenCooldown);
            curAmbushDuration += addedTime;
            screenUI.setInvisBarFill(curAmbushDuration, fullAmbushDuration, minAmbushDurationRequirement);

        }
    }


    // Main function to reset ScreenUI variables: cooldown status and vial status
    private void resetScreenUI() {
        screenUI.displayAmbushCooldown(1f);
        screenUI.displayContaminateCooldown(1f);
        screenUI.displayCaskCooldown(1f, null);
        screenUI.setInvisBarFill(curAmbushDuration, fullAmbushDuration, minAmbushDurationRequirement);

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
            curMaxInventory = numStartingIngredientSlots;

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


    // Main function to get if secondary attack interrupts ambush
    public bool secondaryAttackInterruptsAmbush() {
        return primaryVial.sideEffect.caskLobInterruptsAmbush();
    }


    // Main function to fire the primary vial if it's possible
    //  Pre: the attackDirection is the direction of attack
    //  Post: returns true if successful. false otherwise
    public bool fireSecondaryLob(Vector3 tgtPos, Transform attacker) {
        if (primaryVial == null || runningCaskCooldownSequence != null) {
            return false;
        }

        // Fire bullet and then check ammo afterwards
        bool success = primaryVial.fireSecondaryAttack(tgtPos, attacker, twitchStatus.getBaseAttack());
        if (success) {
            displaySecondaryAttackErrorMessage = !primaryVial.sideEffect.hasAdditionalSecondaryAttackAction();
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
        if (displaySecondaryAttackErrorMessage) {
            if (runningCaskCooldownSequence != null) {
                screenUI.displayErrorMessage(abilityCooldownErrorMessage);
            } else if (primaryVial == null) {
                screenUI.displayErrorMessage(noVialEquippedErrorMessage);
            } else if (!primaryVial.canFireSecondaryLob()) {
                screenUI.displayErrorMessage(runOutOfAmmoMessage);
            }
        }

        return primaryVial != null && primaryVial.canFireSecondaryLob() && runningCaskCooldownSequence == null;
    }

    
    // Main function to do the secondary vial cooldown sequence
    //  Pre: curCooldown of the cask. > 0f
    private IEnumerator caskCooldownSequence(float curCooldown) {
        Debug.Assert(curCooldown > 0f);

        curCaskCooldown = curCooldown;

        // Setup
        curCaskCooldownTimer = 0f;
        screenUI.displayCaskCooldown(0f, primaryVial);

        // Loop
        while (curCaskCooldownTimer < curCaskCooldown) {
            yield return 0;
            curCaskCooldownTimer += Time.deltaTime;
            screenUI.displayCaskCooldown(curCaskCooldownTimer / curCaskCooldown, primaryVial);
        }

        displaySecondaryAttackErrorMessage = true;
        runningCaskCooldownSequence = null;
    }


    private void reduceCaskCooldown(float cooldownReducedTo) {
        Debug.Assert(cooldownReducedTo > 0f);

        float setTimer = (curCaskCooldown - cooldownReducedTo);
        if (setTimer > curCaskCooldownTimer) {
            curCaskCooldownTimer = setTimer;
        }

        displaySecondaryAttackErrorMessage = true;
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


    // Main public function to check if you can conatminate (not on cooldown)
    public bool canAmbush() {
        if (curAmbushDuration < minAmbushDurationRequirement) {
            screenUI.displayErrorMessage(abilityCooldownErrorMessage);
        }
        
        return curAmbushDuration >= minAmbushDurationRequirement;
    }


    // Main public function to turn ambush on
    public void activateAmbush(bool activateBool) {
        isAmbushing = activateBool;
    }


    // Main function to check if you can continue ambushing
    public bool canContinueAmbushing() {
        return curAmbushDuration > 0.0001f;
    }


    // Main event handler function to reset ambush cooldown
    private void onAmbushReset() {
        curAmbushDuration = fullAmbushDuration;
        screenUI.setInvisBarFill(curAmbushDuration, fullAmbushDuration, minAmbushDurationRequirement);
    }


    // Main event handler function to regenerate ambush on enemy death
    public void onNearbyEnemyDeath() {
        curAmbushDuration = Mathf.Min(curAmbushDuration + onEnemyDeathAmbushRegen, fullAmbushDuration);
        curAmbushDuration = Mathf.Min(curAmbushDuration, fullAmbushDuration);

        screenUI.setInvisBarFill(curAmbushDuration, fullAmbushDuration, minAmbushDurationRequirement);
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
        if (value.started && !inventoryUI.isMenuOpen() && runningCraftingSequence == null && !PauseConstraints.isPaused()) {
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

            if (!haveGottenIngredients) {
                haveGottenIngredients = true;
                firstIngredientGainEvent.Invoke();
            }

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


    // Main function to add an additional ingredient slot
    public void addIngredientSlot(int numSlotsAdded) {
        curMaxInventory = Mathf.Min(curMaxInventory + numSlotsAdded, MAX_INVENTORY_SLOTS);
        screenUI.displayIngredientInventory(ingredientInventory, curMaxInventory);
    }


    // Main function to check whether or not you can even add ingredient slots to this inventory
    public bool canAddIngredientSlots() {
        return curMaxInventory < MAX_INVENTORY_SLOTS;
    }


    // Main function to get scarcity-surplus state for ingredients when modifying probability
    //  Post: returns a value between 0f and 1f indicating how full your ingredient inv is based on scarcity and surplus def
    public float getIngredientScarcitySurplusState() {
        float rawIngredientState = (float)getCurrentNumberOfIng() / (float)curMaxInventory;
        float numerator = Mathf.Max(rawIngredientState - scarcityDefinition, 0f);
        float denominator = surplusDefinition - scarcityDefinition;
        float returnedState = Mathf.Min(numerator / denominator, 1f);

        Debug.Assert(returnedState >= 0f && returnedState <= 1f);

        return returnedState;
    }


    // Private helper function to get the cur number of ingredients in the inventory
    private int getCurrentNumberOfIng() {
        int count = 0;

        // Process ingredients that are filled
        foreach(KeyValuePair<PoisonVialStat, int> entry in ingredientInventory) {
            count += entry.Value;
        }

        return count;
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
            PoisonVial nonEffectedVial =  (craftParameters.isPrimary) ? secondaryVial : primaryVial;
            bool gotSideEffectBefore = craftParameters.vial.sideEffect != PoisonVial.poisonVialConstants.defaultSideEffect;
            bool success = craftParameters.vial.craft(craftParameters.stat, recipeBook, nonEffectedVial);
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
        if (!haveMadePoisonVials) {
            haveMadePoisonVials = true;
            firstPoisonVialEvent.Invoke();
        }

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
    public void addRandomRecipes(int numRecipes) {
        if (recipeBook.canAddNewRecipe()) {
            List<Recipe> newRecipes = recipeBook.getRandomRecipes(numRecipes);

            foreach (Recipe r in newRecipes) {
                recipeBook.addNewRecipe(r, false);
            }
        }
    }


    // Main function to add a specific recipe to the recipe book
    public void startRecipeSelectionSequence(RecipeSelectionScreen recipeSelection) {
        Debug.Assert(recipeSelection != null);

        recipeSelection.startRecipeSelectionSequence(
            recipeBook,
            ingredientInventory,
            curMaxInventory,
            primaryVial,
            secondaryVial
        );
    }


    // Main function to check if you can even add any recipes
    public bool canAddNewRecipes() {
        return recipeBook.canAddNewRecipe();
    }
    
}
