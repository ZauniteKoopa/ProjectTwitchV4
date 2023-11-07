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
    [SerializeField]
    private TMP_Text healthText;
    
    [Header("Primary Vial")]
    [SerializeField]
    private Image primaryVialFill;
    [SerializeField]
    private TMP_Text primaryVialAmmo;
    [SerializeField]
    private TMP_Text primaryCaskCostText;

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
    [SerializeField]
    private Color ambushReadyColor = Color.blue;
    [SerializeField]
    private Color ambushNotReadyColor = Color.yellow;


    [Header("Inventory")]
    [SerializeField]
    private ScreenInventorySlot[] inventorySlots;
    [SerializeField]
    private TMP_Text keyText;


    [Header("Error Messaging")]
    [SerializeField]
    private GameObject errorMessageObject;
    [SerializeField]
    private TMP_Text errorMessageText;
    [SerializeField]
    [Min(0.01f)]
    private float errorMessageDuration = 5f;
    [SerializeField]
    private PlayerAudioManager twitchVoice;
    [SerializeField]
    [Min(1)]
    private int errorVoiceFrequency = 4;
    private int curErrorFreq = 0;
    private Coroutine runningErrorMessageSequence = null;


    [Header("Other UI Elements")]
    [SerializeField]
    private PlayerWorldUI worldUI;

    [Header("Audio")]
    [SerializeField]
    private AudioSource speaker;
    [SerializeField]
    private AudioClip errorSound;


    // Main function to display health
    public void displayHealth(float curHealth, float maxHealth) {
        float displayedCurHealth = Mathf.Round(curHealth);
        if (displayedCurHealth < 0.1f) {
            displayedCurHealth = 1f;
        }

        healthBarFill.fillAmount = curHealth / maxHealth;
        healthText.text = displayedCurHealth + "/" + maxHealth;
        worldUI.displayHealthBar(curHealth, maxHealth);
    }


    // Main function to display the cooldown status for ambush
    public void displayAmbushCooldown(float cooldownProgress) {
        displayAbilityIcon(cooldownProgress, ambushIcon, ambushCooldown, Color.white);
    }
    
    
    // Main function to display the cooldown status for ambush
    public void displayCaskCooldown(float cooldownProgress, PoisonVial caskVial) {
        Color caskColor = (caskVial == null) ? Color.white : caskVial.getColor();
        displayAbilityIcon(cooldownProgress, caskIcon, caskCooldown, caskColor);
    }


    // Main function to display the cooldown status for ambush
    public void displayContaminateCooldown(float cooldownProgress) {
        displayAbilityIcon(cooldownProgress, contaminateIcon, contaminateCooldown, Color.white);
    }


    // Main public function to display primaryVial
    public void displayPrimaryVial(PoisonVial vial) {
        displayPoisonVial(vial, primaryVialAmmo, primaryVialFill);
        worldUI.displayPrimaryVial(vial);
        caskGameobject.SetActive(vial != null);
        primaryCaskCostText.text = (vial != null) ? vial.sideEffect.getSecondaryAttackCost().ToString() : "0";

        if (vial != null) {
            caskIcon.color = vial.getColor();
        }
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
    }


    // Main helper function to set invisibility bar fill
    public void setInvisBarFill(float curTimer, float maxDuration, float requiredDuration) {
        invisBarFill.fillAmount = curTimer / maxDuration;
        invisBarFill.color = (curTimer >= requiredDuration) ? ambushReadyColor : ambushNotReadyColor;

        worldUI.setInvisBarFill(curTimer, maxDuration, requiredDuration);
    }


    // Main helper function to remove ambush invisibility
    public void removeAmbushInvisibility() {
        if (runningColorScreenSequence != null) {
            StopCoroutine(runningColorScreenSequence);
        }

        runningColorScreenSequence = StartCoroutine(colorScreenSequence(Color.clear, ambushInvisibilityFadeOut));
    }


    // Main function to display the state of the ingredient inventory slots
    public void displayIngredientInventory(Dictionary<PoisonVialStat, int> ingredientInventory, int maxInventory) {
        int displayedSlots = Mathf.Min(maxInventory, inventorySlots.Length);
        int numProcessedSlots = 0;

        // Process ingredients that are filled
        foreach(KeyValuePair<PoisonVialStat, int> entry in ingredientInventory) {
            int i = 0;

            while (numProcessedSlots < displayedSlots && i < entry.Value) {
                inventorySlots[numProcessedSlots].displayFilled(entry.Key);
                
                numProcessedSlots++;
                i++;
            }
        }

        // Process ingredients that are empty
        while (numProcessedSlots < displayedSlots) {
            inventorySlots[numProcessedSlots].displayEmpty();
            numProcessedSlots++;
        }

        // Process slots that shouldn't exist
        while (numProcessedSlots < inventorySlots.Length) {
            inventorySlots[numProcessedSlots].turnOff();
            numProcessedSlots++;
        }
    }


    // Public function to display num keys the players have
    public void displayNumKeys(int numKeys) {
        keyText.text = "" + numKeys;
    }


    // Main function to do fade to black
    public void fadeToBlack(float fadeDuration, bool scaledTime = true) {
        if (runningColorScreenSequence != null) {
            StopCoroutine(runningColorScreenSequence);
        }

        runningColorScreenSequence = StartCoroutine(colorScreenSequence(Color.black, fadeDuration, scaledTime));
    }


    // Main function to do fade to black
    public void fadeToColor(float fadeDuration, Color endColor, bool scaledTime = true) {
        if (runningColorScreenSequence != null) {
            StopCoroutine(runningColorScreenSequence);
        }

        runningColorScreenSequence = StartCoroutine(colorScreenSequence(endColor, fadeDuration, scaledTime));
    }


    // Main coroutine to handle color screen
    private IEnumerator colorScreenSequence(Color screenEndColor, float fadeTime, bool scaledTime = true) {
        float fadeTimer = 0f;
        Color startColor = colorScreen.color;

        while (fadeTimer < fadeTime) {
            if (scaledTime) {
                yield return 0;
            } else {
                yield return new WaitForSecondsRealtime(0.04f);
            }

            fadeTimer += (scaledTime) ? Time.deltaTime : 0.04f;
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
    private void displayAbilityIcon(float cooldownProgress, Image icon, Image cooldownFill, Color defaultColor) {
        float cooldownFillAmount = 1f - cooldownProgress;
        icon.color = (cooldownFillAmount < 0.01f) ? defaultColor : Color.blue;
        cooldownFill.fillAmount = cooldownFillAmount;
    }


    // Public function for displaying an error message
    //  Pre: errorMessage is the message you want to display
    //  Post: displays the error message on screen for a specified amount of seconds
    public void displayErrorMessage(string errorMessage) {
        if (runningErrorMessageSequence != null) {
            StopCoroutine(runningErrorMessageSequence);
        }

        curErrorFreq++;
        if (curErrorFreq == errorVoiceFrequency) {
            curErrorFreq = 0;
            twitchVoice.playErrorMessageVoice();
        }

        runningErrorMessageSequence = StartCoroutine(errorMessageSequence(errorMessage));
    }



    // Main Coroutine sequence for error message handling
    private IEnumerator errorMessageSequence(string errorMessage) {
        Debug.Assert(errorMessageObject != null && errorMessageText != null);

        // Display message
        errorMessageObject.SetActive(true);
        errorMessageText.text = errorMessage;

        // Play sound
        speaker.clip = errorSound;
        speaker.Play();

        // Wait
        yield return new WaitForSeconds(errorMessageDuration);

        // Clear message
        errorMessageObject.SetActive(false);

        // Clear runningSequence variable
        runningErrorMessageSequence = null;
    }
}
