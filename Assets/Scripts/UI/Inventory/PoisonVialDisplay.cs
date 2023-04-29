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
    public UnityEvent onLookupSideEffect;


    // Main function to display the poison vial
    public void displayPoisonVial(PoisonVial p) {
        p.displayInfo(sideEffectName, buttonIcon, compDisplay);
    }


    // Main function to display an empty poison vial
    public void displayEmpty() {
        sideEffectName.text = "???????";
        buttonIcon.gameObject.SetActive(false);
        compDisplay.displayEmptyComp();
    }


    // Main event handler for when someone whats to learn of the side effect
    public void onLookupPress() {
        Debug.Log("Invoke");
        onLookupSideEffect.Invoke();
    }


}
