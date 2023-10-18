using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class OnboardPopup : MonoBehaviour
{
    // Main functions to make sure that only one popup is active
    private static OnboardPopup runningPopup = null;

    // Main variables to handle sequence
    [SerializeField]
    private GameObject infoPopup;
    [SerializeField]
    private float popupDuration = 5f;
    private bool activated = false;


    // On awake, set infoPopup to inactive
    private void Awake() {
        infoPopup.SetActive(false);
    }

    // Main static function to set running popup
    //  Pre: popup != null
    //  Post: makes popup the current running popup. If there is already one, stop that popup
    private static void setRunningPopup(OnboardPopup popup) {
        Debug.Assert(popup != null);
        // Interrupt running popup so that only 1 popup is running at a time
        if (runningPopup != null) {
            runningPopup.interrupt();
        }

        runningPopup = popup;
    }


    // Main function make sure that the script won't keep track of current popup
    //  Pre: stoppedPopup is the popup to be stopped.
    //  Post: if stoppedPopup == runningPopup, set runningPopup to null
    private static void stopRunningPopup(OnboardPopup stoppedPopup) {
        if (stoppedPopup == runningPopup) {
            runningPopup = null;
        }
    } 


    // Main function to stop popup
    public void interrupt() {
        StopAllCoroutines();
        infoPopup.SetActive(false);
        Object.Destroy(gameObject);
    }


    // Main function to trigger popup, can react based on events
    public void onPopupTrigger() {
        if (!activated) {
            activated = true;
            StartCoroutine(popupSequence());
        }
    }


    // Main function to do the sequence for the popup
    //  Pre: popupDuration > 0f
    //  Post: main way to do sequence
    private IEnumerator popupSequence() {
        setRunningPopup(this);
        infoPopup.SetActive(true);

        yield return new WaitForSeconds(popupDuration);

        infoPopup.SetActive(false);
        stopRunningPopup(this);
        Object.Destroy(gameObject);
    }
}
