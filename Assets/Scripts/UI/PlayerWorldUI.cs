using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerWorldUI : MonoBehaviour
{
    [SerializeField]
    private Image primaryVialDisplay;
    [SerializeField]
    private Image secondaryVialDisplay;
    [SerializeField]
    private Image healthBarFill;
    [SerializeField]
    private Transform bulletList;



    // Main function to display primary vial
    public void displayPrimaryVial(PoisonVial vial) {
        int ammoCount = (vial == null) ? 0 : vial.getAmmo();

        // Update the bullet list
        bulletList.gameObject.SetActive(ammoCount <= bulletList.childCount && ammoCount > 0);
        if (ammoCount <= bulletList.childCount && ammoCount > 0) {
            for (int b = 0; b < bulletList.childCount; b++) {
                bulletList.GetChild(b).gameObject.SetActive(b < vial.getAmmo());
            }
        }
    }


    // Main function to display secondary vial
    public void displaySecondaryVial(PoisonVial vial) {

    }


    // Main function to display health bar
    public void displayHealthBar(float curHealth, float maxHealth) {
        healthBarFill.fillAmount = curHealth / maxHealth;
    }
}
