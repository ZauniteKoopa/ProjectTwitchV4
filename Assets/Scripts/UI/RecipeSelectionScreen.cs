using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RecipeSelectionScreen : MonoBehaviour
{
    [Header("Recipe Pages")]
    [SerializeField]
    private RecipeDisplay[] recipeDisplays;
    
    [Header("Inventory Clusters")]
    [SerializeField]
    private Image primaryVialDisplay;
    [SerializeField]
    private Image secondaryVialDisplay;
    [SerializeField]
    private ScreenInventorySlot[] inventorySlots;

    [Header("Events")]
    public UnityEvent selectionEndEvent;

    // Runtime variables
    private RecipeBook curRecipeBook;
    private List<Recipe> selectedRecipes;
    private float prevTimeScale = 1f;

    
    // Main function to start pause sequence
    public void startRecipeSelectionSequence(
        RecipeBook rb,
        Dictionary<PoisonVialStat, int> ingredientInventory,
        int maxInventory,
        PoisonVial primaryVial,
        PoisonVial secondaryVial
    ) {
        // Get recipes
        gameObject.SetActive(true);
        curRecipeBook = rb;
        selectedRecipes = curRecipeBook.getRandomRecipes(recipeDisplays.Length);

        // Display the recipes
        for (int r = 0; r < recipeDisplays.Length; r++) {
            if (r < selectedRecipes.Count) {
                recipeDisplays[r].displayRecipe(selectedRecipes[r]);
            } else {
                recipeDisplays[r].displayEmpty();
            }
        }

        // Display the inventory
        displayIngredientInventory(ingredientInventory, maxInventory);
        displayVial(primaryVial, primaryVialDisplay);
        displayVial(secondaryVial, secondaryVialDisplay);
        
        prevTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        PauseConstraints.externalPause(true);
    }


    // Main function to end pause sequence with a recipe selected
    public void endRecipeSelectionSequence(int selectedRecipeIndex) {
        curRecipeBook.addNewRecipe(selectedRecipes[selectedRecipeIndex]);
        gameObject.SetActive(false);

        Time.timeScale = prevTimeScale;
        PauseConstraints.externalPause(false);

        selectionEndEvent.Invoke();
    }


    // Main function to display the state of the ingredient inventory slots
    private void displayIngredientInventory(Dictionary<PoisonVialStat, int> ingredientInventory, int maxInventory) {
        int displayedSlots = Mathf.Min(maxInventory, inventorySlots.Length);
        int numProcessedSlots = 0;

        // Process ingredients that are filled
        foreach(KeyValuePair<PoisonVialStat, int> entry in ingredientInventory) {
            int i = 0;

            while (numProcessedSlots < displayedSlots && i < entry.Value) {
                inventorySlots[numProcessedSlots].displayFilled(entry.Key);
                
                numProcessedSlots++;
                i++;
            }
        }

        // Process ingredients that are empty
        while (numProcessedSlots < displayedSlots) {
            inventorySlots[numProcessedSlots].displayEmpty();
            numProcessedSlots++;
        }

        // Process slots that shouldn't exist
        while (numProcessedSlots < inventorySlots.Length) {
            inventorySlots[numProcessedSlots].turnOff();
            numProcessedSlots++;
        }
    }


    // Main function to display vials
    public void displayVial(PoisonVial vial, Image vialDisplay) {
        vialDisplay.color = (vial != null) ? vial.getColor() : Color.black;
        vialDisplay.sprite = (vial != null) ? vial.sideEffect.spriteIcon : null;
    }
}
