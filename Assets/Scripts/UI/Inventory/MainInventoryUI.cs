using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class MainInventoryUI : MonoBehaviour
{
    // Main components
    [SerializeField]
    private IngredientInventoryDisplay ingredientsDisplay;
    [SerializeField]
    private CraftVialUI craftingUI;
    [SerializeField]
    private RecipeBookDisplay recipeBookDisplay;
    [SerializeField]
    private PlayerInput playerInput;

    public UnityEvent inventoryMenuClosedEvent;
    private bool isOpened = false;
    private float prevTimeScale = 1.0f;


    // On open, display all the information necessary
    //  Pre: none of the parameters are null except primaryVial and secondaryVial, ingredientsInventory.Count == 4, maxInventorySize >= 0
    //  Post: opens the menu
    public void open(Dictionary<PoisonVialStat, int> ingredientInventory, int maxInventorySize, PoisonVial primaryVial, PoisonVial secondaryVial, RecipeBook recipeBook) {
        // Set up everything
        ingredientsDisplay.display(ingredientInventory, maxInventorySize);
        craftingUI.open(primaryVial, secondaryVial);
        recipeBookDisplay.display(recipeBook);

        gameObject.SetActive(true);
        StartCoroutine(openSequence());
    }


    // Main open sequence
    private IEnumerator openSequence() {
        yield return 0;

        // Set flags up
        isOpened = true;
        prevTimeScale = Time.timeScale;
        playerInput.enabled = true;

        Time.timeScale = 0f;
    }


    // Main function to close the menu
    //  Pre: none
    //  Post: closes the menu and sends out an event
    public void close() {
        if (isOpened) {
            isOpened = false;

            Time.timeScale = prevTimeScale;
            gameObject.SetActive(false);
            playerInput.enabled = false;
        }
    }



    // Main function to close with an event - should only be used when directly closing this menu with no after effects
    // (like waiting for an crafting sequence to end)
    public void closeWithEvent() {
        if (isOpened) {
            close();
            inventoryMenuClosedEvent.Invoke();
        }
    }


    // Main event handler function for closing the menu
    public void onCloseButtonPress(InputAction.CallbackContext value) {
        if (value.started && isOpened) {
            Debug.Log("CLOSE???");
            closeWithEvent();
        }
    }


    // Main accessor function for whether the menu is open
    public bool isMenuOpen() {
        return isOpened;
    }
}
