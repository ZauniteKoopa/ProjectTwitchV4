using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class PoisonVialDisplay : MonoBehaviour
{
    [SerializeField]
    private TMP_Text sideEffectName;
    [SerializeField]
    private Image buttonIcon;
    [SerializeField]
    private PoisonCompositionDisplay compDisplay;
    [SerializeField]
    private Image vialFill;
    [SerializeField]
    private TMP_Text ammoLabel;


    public UnityEvent onLookupSideEffect;


    // Main function to display the poison vial
    public void display(PoisonVial p) {
        if (p == null) {
            displayEmpty();

        } else {
            p.displayInfo(sideEffectName, buttonIcon, compDisplay);

            vialFill.fillAmount = (float)p.getAmmo() / (float)PoisonVial.MAX_AMMO;
            vialFill.color = p.getColor();
            ammoLabel.text = "" + p.getAmmo();
        }
    }


    // Main function to display an empty poison vial
    public void displayEmpty() {
        sideEffectName.text = "Maybe I should add something";
        buttonIcon.gameObject.SetActive(false);
        compDisplay.displayEmptyComp();

        vialFill.fillAmount = 0f;
        vialFill.color = Color.black;
        ammoLabel.text = "0";
    }


    // Main event handler for when someone whats to learn of the side effect
    public void onLookupPress() {
        onLookupSideEffect.Invoke();
    }


}
