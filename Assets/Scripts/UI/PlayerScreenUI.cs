using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerScreenUI : MonoBehaviour
{
    [Header("Health")]
    [SerializeField]
    private Image healthBarFill;
    
    [Header("Primary Vial")]
    [SerializeField]
    private Image primaryVialFill;
    [SerializeField]
    private TMP_Text primaryVialAmmo;

    [Header("Secondary Vial")]
    [SerializeField]
    private Image secondaryVialFill;
    [SerializeField]
    private TMP_Text secondaryVialAmmo;

    
    [Header("Ability Cooldowns")]
    [SerializeField]
    private Image ambushCooldown;
    [SerializeField]
    private Image ambushIcon;
    [SerializeField]
    private Image caskCooldown;
    [SerializeField]
    private Image caskIcon;
    [SerializeField]
    private GameObject caskGameobject;
    [SerializeField]
    private Image contaminateCooldown;
    [SerializeField]
    private Image contaminateIcon;
    // [SerializeField]
    // private Image drinkVialCooldown;
    // [SerializeField]
    // private Image drinkVialIcon;
    // [SerializeField]
    // private GameObject drinkVialIcon;
    
    [Header("Color Screen Effects")]
    [SerializeField]
    private Image colorScreen;
    [SerializeField]
    private Color ambushInvisibilityColor = Color.black;
    [SerializeField]
    [Min(0.1f)]
    private float ambushInvisibilityFadeIn = 1f;
    [SerializeField]
    [Min(0.1f)]
    private float ambushInvisibilityFadeOut = 0.25f;
    private Coroutine runningColorScreenSequence;


    [Header("InvisibilityBar")]
    [SerializeField]
    private GameObject invisBarObject;
    [SerializeField]
    private Image invisBarFill;


    [Header("Other UI Elements")]
    [SerializeField]
    private PlayerWorldUI worldUI;


    // Main function to display health
    public void displayHealth(float curHealth, float maxHealth) {
        healthBarFill.fillAmount = curHealth / maxHealth;
        worldUI.displayHealthBar(curHealth, maxHealth);
    }


    // Main function to display the cooldown status for ambush
    public void displayAmbushCooldown(float cooldownProgress) {
        displayAbilityIcon(cooldownProgress, ambushIcon, ambushCooldown);
    }
    
    
    // Main function to display the cooldown status for ambush
    public void displayCaskCooldown(float cooldownProgress) {
        displayAbilityIcon(cooldownProgress, caskIcon, caskCooldown);
    }


    // Main function to display the cooldown status for ambush
    public void displayContaminateCooldown(float cooldownProgress) {
        displayAbilityIcon(cooldownProgress, contaminateIcon, contaminateCooldown);
    }


    // Main public function to display primaryVial
    public void displayPrimaryVial(PoisonVial vial) {
        displayPoisonVial(vial, primaryVialAmmo, primaryVialFill);
        worldUI.displayPrimaryVial(vial);
        caskGameobject.SetActive(vial != null);
    }


    // Main public function to display secondary vial
    public void displaySecondaryVial(PoisonVial vial) {
        displayPoisonVial(vial, secondaryVialAmmo, secondaryVialFill);
        worldUI.displaySecondaryVial(vial);
    }
    
    
    // Main helper function to display ambush invisibility
    public void displayAmbushInvisibility() {
        if (runningColorScreenSequence != null) {
            StopCoroutine(runningColorScreenSequence);
        }

        runningColorScreenSequence = StartCoroutine(colorScreenSequence(ambushInvisibilityColor, ambushInvisibilityFadeIn));
        invisBarObject.SetActive(true);
        invisBarFill.fillAmount = 0f;
    }


    // Main helper function to set invisibility bar fill
    public void setInvisBarFill(float curTimer, float maxDuration) {
        invisBarFill.fillAmount = curTimer / maxDuration;
    }


    // Main helper function to remove ambush invisibility
    public void removeAmbushInvisibility() {
        if (runningColorScreenSequence != null) {
            StopCoroutine(runningColorScreenSequence);
        }

        runningColorScreenSequence = StartCoroutine(colorScreenSequence(Color.clear, ambushInvisibilityFadeOut));
        invisBarObject.SetActive(false);
    }


    // Main coroutine to handle color screen
    private IEnumerator colorScreenSequence(Color screenEndColor, float fadeTime) {
        float fadeTimer = 0f;
        Color startColor = colorScreen.color;

        while (fadeTimer < fadeTime) {
            yield return 0;

            fadeTimer += Time.deltaTime;
            colorScreen.color = Color.Lerp(startColor, screenEndColor, fadeTimer / fadeTime);
        }

        colorScreen.color = screenEndColor;
        runningColorScreenSequence = null;
    }



    // Main private helper function to display a vial info: if vial is null, disable ammoText and set vialFill to 0f
    private void displayPoisonVial(PoisonVial vial, TMP_Text ammoText, Image vialFill) {
        ammoText.gameObject.SetActive(vial != null);
        if (vial != null) {
            ammoText.text = "" + vial.getAmmo();
        }

        vialFill.fillAmount = (vial != null) ? (float)vial.getAmmo() / (float)PoisonVial.MAX_AMMO : 0f;
        vialFill.color = (vial != null) ? vial.getColor() : Color.black;
    }


    // Private helper function to display cooldown for an icon ability
    private void displayAbilityIcon(float cooldownProgress, Image icon, Image cooldownFill) {
        float cooldownFillAmount = 1f - cooldownProgress;
        icon.color = (cooldownFillAmount < 0.01f) ? Color.white : Color.blue;
        cooldownFill.fillAmount = cooldownFillAmount;
    }
}
