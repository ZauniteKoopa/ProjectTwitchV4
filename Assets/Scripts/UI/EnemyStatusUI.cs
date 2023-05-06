using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using TMPro;

public class EnemyStatusUI : MonoBehaviour
{
    [SerializeField]
    private Image healthBar;
    [SerializeField]
    private Image poisonHalo;
    private float startingPoisonHaloScale = 1f;
    [SerializeField]
    private TMP_Text poisonHaloCount;
    [SerializeField]
    private Animator poisonHaloAnimator;
    [SerializeField]
    [Min(0.001f)]
    private float tickVibrateDelta = 0.2f;
    [SerializeField]
    [Min(0.001f)]
    private float tickVibrateDuration = 0.2f;

    private Coroutine runningPoisonHaloVibrateSequence = null;


    // On awake, setup
    private void Awake() {
        startingPoisonHaloScale = poisonHalo.transform.localScale.x;
    }


    // Main function to update health bar
    //  Pre: maxHealth >= curHealth
    //  Post: healthBar is updated
    public void updateHealthBar(float curHealth, float maxHealth) {
        Debug.Assert(curHealth <= maxHealth);

        healthBar.fillAmount = curHealth / maxHealth;
    }


    // Main function to update poison halo
    //  Pre: poisonStacks >= 0
    //  Post: update poison stacks display
    public void updatePoisonHalo(int poisonStacks, Color poisonColor) {
        Debug.Assert(poisonStacks >= 0);

        bool poisoned = poisonStacks > 0;
        poisonHalo.gameObject.SetActive(poisoned);
        poisonHalo.color = poisonColor;

        if (poisonHaloCount != null) {
            poisonHaloCount.text = "" + poisonStacks;
        }

        if (poisonHaloAnimator != null) {
            poisonHaloAnimator.SetInteger("NumPoisonStacks", poisonStacks);
        }
    }


    // Main sequence to vibrate the poison halo
    public void vibratePoisonHalo() {
        if (runningPoisonHaloVibrateSequence != null) {
            StopCoroutine(runningPoisonHaloVibrateSequence);
            runningPoisonHaloVibrateSequence = null;
            poisonHalo.transform.localScale = Vector3.one * startingPoisonHaloScale;
        }

        runningPoisonHaloVibrateSequence = StartCoroutine(vibratePoisonTickSpriteSequence());
    }


    // Main sequence to vibrate the delta
    private IEnumerator vibratePoisonTickSpriteSequence() {
        float timer = 0f;
        float tickHalfDuration = tickVibrateDuration / 2f;
        float popScale = startingPoisonHaloScale + tickVibrateDelta;

        // Growth
        while (timer < tickHalfDuration) {
            yield return 0;

            timer += Time.deltaTime;
            poisonHalo.transform.localScale = Vector3.one * Mathf.Lerp(startingPoisonHaloScale, popScale, timer / tickHalfDuration);
        }

        // Shrink
        timer = 0f;
        while (timer < tickHalfDuration) {
            yield return 0;

            timer += Time.deltaTime;
            poisonHalo.transform.localScale = Vector3.one * Mathf.Lerp(popScale, startingPoisonHaloScale, timer / tickHalfDuration);
        }

        runningPoisonHaloVibrateSequence = null;
    }

}
