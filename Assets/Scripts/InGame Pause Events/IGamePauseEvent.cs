using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class IGamePauseEvent : MonoBehaviour
{
    // Run time variables
    private float prevTimeScale = 1f;
    private bool ending = false;

    public UnityEvent pauseEventStarted;
    public UnityEvent pauseEventFinished;
    public bool startOnAwake = false;


    // On awake, setup
    private void Start() {
        if (startOnAwake) {
            startEvent();
        }
    }
    

    // Main function to start event
    public void startEvent() {
        prevTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        PauseConstraints.externalPause(true);

        startEventHelper();
        pauseEventStarted.Invoke();
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
        PauseConstraints.externalPause(false);

        yield return 0;

        Time.timeScale = prevTimeScale;
        endEventHelper();

        pauseEventFinished.Invoke();
        
        ending = false;
    }


    // Main function to start up event with no consideration for pause
    protected abstract void startEventHelper();


    // Main function to end event with no consideration for pause
    protected abstract void endEventHelper();

}
