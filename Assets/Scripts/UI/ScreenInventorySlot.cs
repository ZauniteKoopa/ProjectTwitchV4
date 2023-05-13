using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenInventorySlot : MonoBehaviour
{
    [SerializeField]
    private Image slotImage;


    // Main function to display a filled slot 
    //  Pre: ingredientType is the type that corresponds to this poison vial stat
    //  Post: ingredient slot now displays that its filled with that stat type
    public void displayFilled(PoisonVialStat ingredientType) {
        gameObject.SetActive(true);
        slotImage.color = PoisonVial.poisonVialConstants.getPureColor(ingredientType);
    }


    // Main function to display an empty slot that's present
    //  Pre: none
    //  Post: ingredient slot now displays that its empty BUT present
    public void displayEmpty() {
        gameObject.SetActive(true);
        slotImage.color = Color.black;
    }


    // Main function to not display a slot
    //  Pre: none
    //  Post: ingredient slot will not be present
    public void turnOff() {
        gameObject.SetActive(false);
    }
}
