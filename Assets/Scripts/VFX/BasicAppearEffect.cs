using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAppearEffect : IStaticEffect
{
    // How long does the affect last
    [SerializeField]
    [Min(0.1f)]
    private float effectDuration = 2f;
    private bool running = false;


    // Main function to activate static visual effect
    public override void executeEffect() {
        if (!running) {
            running = true;
            StartCoroutine(executeEffectSequence());
        }
    }


    // Main function to activate static visual effect
    private IEnumerator executeEffectSequence() {
        gameObject.SetActive(true);

        yield return new WaitForSeconds(effectDuration);

        effectEndEvent.Invoke();
        Object.Destroy(gameObject);
    }
}
