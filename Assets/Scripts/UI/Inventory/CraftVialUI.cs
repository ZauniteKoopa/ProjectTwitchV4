using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Assertions;

public class CraftParameters {
    public PoisonVialStat stat;
    public PoisonVial vial;
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

    // Runtime variables
    private PoisonVial primaryVial;
    private PoisonVial secondaryVial;
    private bool linkedToPrimary = true;
    public CraftVialEvent craftingVialEvent;



    // Main function for opening the craft UI
    public void open(PoisonVial p, PoisonVial s) {
        primaryVialDisplay.display(p);
        secondaryVialDisplay.display(s);

        turnOffValve(primaryValve);
        turnOffValve(secondaryValve);
    }


    // Main event handler function for when an ingredient has been dropped
    public void onIngredientSlotFilled() {
        Image[] activeValve = (linkedToPrimary) ? primaryValve : secondaryValve;
        turnOnValve(activeValve, Color.red);
    }


    // Main event handler function for when you swapped valves
    public void onValveSwap() {
        linkedToPrimary = !linkedToPrimary;


        if (ingSlot.hasIngredient()) {
            Image[] activeValve = (linkedToPrimary) ? primaryValve : secondaryValve;
            Image[] passiveValve = (!linkedToPrimary) ? primaryValve : secondaryValve;

            turnOffValve(passiveValve);
            turnOnValve(activeValve, Color.red);
        }
    }


    // Main event handler function for when craft button has been pressed
    public void onCraftButtonPress() {
        // Case where ingredient has not been found
        PoisonVialStat ingStat;
        if (!ingSlot.hasIngredient(out ingStat)) {
            Debug.LogWarning("CANNOT CRAFT, NO INGREDIENT FOUND");
            return;
        }

        // Check for case where vial can't be crafted
        PoisonVial tgtVial = (linkedToPrimary) ? primaryVial : secondaryVial;
        if (tgtVial != null && !tgtVial.canCraft()) {
            Debug.LogWarning("CANNOT CRAFT, EXCEEDED NUMBER OF UPGRADES POSSIBLE");
            return;
        }

        // Go ahead and craft
        CraftParameters craftParameters = new CraftParameters();
        craftParameters.stat = ingStat;
        craftParameters.vial = tgtVial;
        craftingVialEvent.Invoke(craftParameters);
    }


    // Main event handler function for when you want to lookup the primary vial's side effect
    public void lookAtPrimarySideEffect() {
        recipeBookDisplay.displaySpecificSideEffect(primaryVial.sideEffect);
    }


    // Main event handler function for when you want to lookup the secondary vial's side effect
    public void lookAtSecondarySideEffect() {
        recipeBookDisplay.displaySpecificSideEffect(secondaryVial.sideEffect);
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
