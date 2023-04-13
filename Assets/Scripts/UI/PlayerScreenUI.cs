using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScreenUI : MonoBehaviour
{
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


    // Main helper function to display ambush invisibility
    public void displayAmbushInvisibility() {
        if (runningColorScreenSequence != null) {
            StopCoroutine(runningColorScreenSequence);
        }

        runningColorScreenSequence = StartCoroutine(colorScreenSequence(ambushInvisibilityColor, ambushInvisibilityFadeIn));
    }


    // Main helper function to remove ambush invisibility
    public void removeAmbushInvisibility() {
        if (runningColorScreenSequence != null) {
            StopCoroutine(runningColorScreenSequence);
        }

        runningColorScreenSequence = StartCoroutine(colorScreenSequence(Color.clear, ambushInvisibilityFadeOut));
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
}
