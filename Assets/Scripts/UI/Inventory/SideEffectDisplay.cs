using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SideEffectDisplay : MonoBehaviour
{
    [SerializeField]
    private Image sideEffectIcon;
    [SerializeField]
    private TMP_Text nameLabel;
    [SerializeField]
    private TMP_Text descriptionLabel;


    // On awake, error check
    private void Awake() {
        if (sideEffectIcon == null || nameLabel == null || descriptionLabel == null) {
            Debug.LogError("Please ensure that all properties of the side effect display are filled");
        }
    }


    // Main function to display side effect given the following
    public void displayItem(Sprite icon, string n, string d) {
        sideEffectIcon.sprite = icon;
        nameLabel.text = n;
        descriptionLabel.text = d;
    }


    // Main function to display empty
    public void displayEmpty() {
        displayItem(null, "?????????", "Hmmm... Surely there's something in these sewers..");
    }

}
