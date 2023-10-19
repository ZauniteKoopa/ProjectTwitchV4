using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IGamePauseEvent : MonoBehaviour
{
    // Run time variables
    private float prevTimeScale = 1f;
    private bool ending = false;
    

    // Main function to start event
    public void startEvent() {
        prevTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        PauseConstraints.externalPause(true);

        startEventHelper();
    }


    // Main function to end event
    public void endEvent() {
        if (!ending) {
            StartCoroutine(endEventSequence());
        }
    }


    // Main sequence function to end the event
    private IEnumerator endEventSequence() {
        ending = true;

        yield return 0;

        PauseConstraints.externalPause(false);
        Time.timeScale = prevTimeScale;
        endEventHelper();
        
        ending = false;
    }


    // Main function to start up event with no consideration for pause
    protected abstract void startEventHelper();


    // Main function to end event with no consideration for pause
    protected abstract void endEventHelper();

}
