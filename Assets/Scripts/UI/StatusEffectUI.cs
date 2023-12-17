using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusEffectUI : MonoBehaviour
{
    [Header("Images")]
    [SerializeField]
    private Image stunIndicator;
    [SerializeField]
    private Image speedIndicator;
    [SerializeField]
    private Image manicIndicator;
    [SerializeField]
    private Image armorIndicator;

    [Header("Speed Up")]
    [SerializeField]
    private Sprite speedUpSprite;
    [SerializeField]
    private Sprite speedDownSprite;
    [SerializeField]
    private Color buffColor = Color.green;
    [SerializeField]
    private Color debuffColor = Color.red;
    private const float EPSILON = 0.05f;


    [Header("Armor")]
    [SerializeField]
    private Sprite armorDownSprite;


    // Main function to reset
    public void reset() {
        stunIndicator.gameObject.SetActive(false);
        manicIndicator.gameObject.SetActive(false);
        speedIndicator.gameObject.SetActive(false);
        armorIndicator.gameObject.SetActive(false);
    }


    // Main function to show if you're stunned or not
    public void showStunIndicator(bool willShow) {
        stunIndicator.gameObject.SetActive(willShow);
    }


    // Main function to show if you're manic or not
    public void showManicIndicator(bool willShow) {
        manicIndicator.gameObject.SetActive(willShow);
    }


    // Main function to show if your speed is affected
    public void showSpeedModifier(float speedModifier) {
        Debug.Assert(speedModifier >= 0f);

        bool isSpeedBuff = speedModifier >= 1f + EPSILON;
        bool isSpeedDebuff = speedModifier <= 1f - EPSILON;

        speedIndicator.gameObject.SetActive(isSpeedBuff || isSpeedDebuff);
        speedIndicator.sprite = (isSpeedDebuff) ? speedDownSprite : speedUpSprite;
        speedIndicator.color = (isSpeedDebuff) ? debuffColor : buffColor;
    }


    // Main function to show if your armor is affected
    public void showArmorModifier(float armorModifier) {
        Debug.Assert(armorModifier >= 0f);

        bool isDebuff = armorModifier <= 1f - EPSILON;

        armorIndicator.gameObject.SetActive(isDebuff);
        armorIndicator.sprite = (isDebuff) ? armorDownSprite : null;
        armorIndicator.color = (isDebuff) ? debuffColor : buffColor;
    }
}
