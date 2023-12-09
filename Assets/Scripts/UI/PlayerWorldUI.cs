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
    [SerializeField]
    private Image invisBarFill;
    [SerializeField]
    private ResourceBar ambushSideEffectBar;
    [SerializeField]
    private Color ambushReadyColor = Color.blue;
    [SerializeField]
    private Color ambushNotReadyColor = Color.yellow;
    [SerializeField]
    private Color ambushSideEffectReadyColor = Color.green;
    [SerializeField]
    private Color ambushSideEffectNotReadyColor = Color.white;



    // Main function to display primary vial
    public void displayPrimaryVial(PoisonVial vial) {
        int ammoCount = (vial == null) ? 0 : vial.getAmmo();

        primaryVialDisplay.color = (vial != null) ? vial.getColor() : Color.black;
        primaryVialDisplay.sprite = (vial != null) ? vial.sideEffect.spriteIcon : null;

        // Update the bullet list
        bulletList.gameObject.SetActive(ammoCount <= bulletList.childCount && ammoCount > 0);
        if (ammoCount <= bulletList.childCount && ammoCount > 0) {
            for (int b = 0; b < bulletList.childCount; b++) {
                Transform curBullet = bulletList.GetChild(b);
                curBullet.gameObject.SetActive(b < vial.getAmmo());

                curBullet.GetComponent<Image>().color = primaryVialDisplay.color;
            }
        }
    }


    // Main function to display secondary vial
    public void displaySecondaryVial(PoisonVial vial) {
        secondaryVialDisplay.color = (vial != null) ? vial.getColor() : Color.black;
        secondaryVialDisplay.sprite = (vial != null) ? vial.sideEffect.spriteIcon : null;
    }


    // Main function to display health bar
    public void displayHealthBar(float curHealth, float maxHealth) {
        healthBarFill.fillAmount = curHealth / maxHealth;
    }


    // Main helper function to set invisibility bar fill
    public void setInvisBarFill(float curTimer, float maxDuration, float requiredDuration) {
        invisBarFill.fillAmount = curTimer / maxDuration;
        invisBarFill.color = (curTimer >= requiredDuration) ? ambushReadyColor : ambushNotReadyColor;
    }

    // Main helper function to set up invis side effect bar
    public void setAmbushSideEffectBar(float curTimer, float requiredDuration, bool isActive) {
        ambushSideEffectBar.setActive(isActive);

        if (isActive) {
            ambushSideEffectBar.setFill(curTimer, requiredDuration);
            
            Color curFillColor = (curTimer >= requiredDuration) ? ambushSideEffectReadyColor : ambushSideEffectNotReadyColor;
            ambushSideEffectBar.setColor(curFillColor);
        }
    }
}
