using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Assertions;
using TMPro;

public class CraftParameters {
    public PoisonVialStat stat;
    public PoisonVial vial;
    public bool isPrimary;
}

[System.Serializable]
public class CraftVialEvent : UnityEvent<CraftParameters> {}

public class CraftVialUI : MonoBehaviour
{
    [SerializeField]
    private PoisonVialDisplay primaryVialDisplay;
    [SerializeField]
    private PoisonVialDisplay secondaryVialDisplay;
    [SerializeField]
    private CraftIngredientSlot ingSlot;
    [SerializeField]
    private RecipeBookDisplay recipeBookDisplay;
    [SerializeField]
    private Image[] primaryValve;
    [SerializeField]
    private Image[] secondaryValve;
    [SerializeField]
    private Color offColor = Color.black;
    private Color filledColor;

    [Header("Error messaging")]
    [SerializeField]
    private string noIngredientFoundError = "No Ingredient Found";
    [SerializeField]
    private string ranOutOfUpgradesError = "Run Out of Upgrades";
    [SerializeField]
    private TMP_Text errorMessageText = null;
    [SerializeField]
    private PlayerAudioManager twitchVoice;

    // Runtime variables
    private PoisonVial primaryVial;
    private PoisonVial secondaryVial;
    private bool linkedToPrimary = true;
    public CraftVialEvent craftingVialEvent;



    // Main function for opening the craft UI
    public void open(PoisonVial p, PoisonVial s) {
        primaryVialDisplay.display(p);
        secondaryVialDisplay.display(s);
        
        primaryVial = p;
        secondaryVial = s;

        turnOffValve(primaryValve);
        turnOffValve(secondaryValve);
        clearErrorMessage();

        ingSlot.Reset();
    }


    // Main event handler function for when an ingredient has been dropped
    public void onIngredientSlotFilled() {
        Image[] activeValve = (linkedToPrimary) ? primaryValve : secondaryValve;

        PoisonVialStat stat;
        ingSlot.hasIngredient(out stat);
        filledColor = PoisonVial.poisonVialConstants.getPureColor(stat);
        turnOnValve(activeValve, filledColor);
    }


    // Main event handler function for when you swapped valves
    public void onValveSwap() {
        linkedToPrimary = !linkedToPrimary;

        if (ingSlot.hasIngredient()) {
            Image[] activeValve = (linkedToPrimary) ? primaryValve : secondaryValve;
            Image[] passiveValve = (!linkedToPrimary) ? primaryValve : secondaryValve;

            turnOffValve(passiveValve);
            turnOnValve(activeValve, filledColor);
        }
    }


    // Main event handler function for when craft button has been pressed
    public void onCraftButtonPress() {
        // Case where ingredient has not been found
        PoisonVialStat ingStat;
        if (!ingSlot.hasIngredient(out ingStat)) {
            displayErrorMessage(noIngredientFoundError);
            return;
        }

        // Check for case where vial can't be crafted
        PoisonVial tgtVial = (linkedToPrimary) ? primaryVial : secondaryVial;
        if (tgtVial != null && !tgtVial.canCraft()) {
            displayErrorMessage(ranOutOfUpgradesError);
            return;
        }

        // Go ahead and craft
        ingSlot.CraftIngredient();

        CraftParameters craftParameters = new CraftParameters();
        craftParameters.stat = ingStat;
        craftParameters.vial = tgtVial;
        craftParameters.isPrimary = linkedToPrimary;
        craftingVialEvent.Invoke(craftParameters);
        clearErrorMessage();
    }


    // Main event handler function to replace a vial
    public void onReplaceButtonPress() {
        // Case where ingredient has not been found
        PoisonVialStat ingStat;
        if (!ingSlot.hasIngredient(out ingStat)) {
            displayErrorMessage(noIngredientFoundError);
            return;
        }

        // Go ahead and craft
        ingSlot.CraftIngredient();

        // Set craftparameters.vial to null so that it will always replace the poison vial
        CraftParameters craftParameters = new CraftParameters();
        craftParameters.stat = ingStat;
        craftParameters.vial = null;
        craftParameters.isPrimary = linkedToPrimary;
        craftingVialEvent.Invoke(craftParameters);
        clearErrorMessage();
    }


    // Main event handler function for when you want to lookup the primary vial's side effect
    public void lookAtPrimarySideEffect() {
        recipeBookDisplay.displaySpecificSideEffect(primaryVial.sideEffect);
    }


    // Main event handler function for when you want to lookup the secondary vial's side effect
    public void lookAtSecondarySideEffect() {
        recipeBookDisplay.displaySpecificSideEffect(secondaryVial.sideEffect);
    }


    // Main function to display error message
    private void displayErrorMessage(string errorMessage) {
        errorMessageText.gameObject.SetActive(true);
        errorMessageText.text = errorMessage;
        twitchVoice.playErrorMessageVoice();
    }


    // Main function to clear the error message
    private void clearErrorMessage() {
        errorMessageText.gameObject.SetActive(false);
    }


    // Main function to turn off a valve
    private void turnOffValve(Image[] valve) {
        Debug.Assert(valve != null);

        foreach (Image pipe in valve) {
            pipe.color = offColor;
        }
    }


    // Main function to turn on a valve
    private void turnOnValve(Image[] valve, Color filledColor) {
        Debug.Assert(valve != null);

        foreach (Image pipe in valve) {
            pipe.color = filledColor;
        }
    }
}
