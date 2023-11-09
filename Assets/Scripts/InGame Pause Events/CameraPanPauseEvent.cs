using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPanPauseEvent : IGamePauseEvent
{
    // Main variables for the camera
    [Header("Camera variables")]
    [SerializeField]
    private Transform cameraTarget = null;
    [SerializeField]
    [Min(0.1f)]
    private float cameraSpeed = 10f;
    [SerializeField]
    private float cameraRotationPitch = 45f;
    [SerializeField]
    private float cameraRotationYaw = 0f;
    [SerializeField]
    [Min(0.1f)]
    private float cameraZoom = 10f;
    [SerializeField]
    private Vector3 cameraOffset;
    [SerializeField]
    [Min(0.1f)]
    private float cameraPanStayTargetDuration = 2f;
    [SerializeField]
    [Min(0.1f)]
    private float cameraPanStayPlayerDuration = 0.5f;

    // Popup variables
    [Header("Popup variables")]
    [SerializeField]
    private MultiPageOnboardingPopup onboardingPopup = null;



    // Main function to start up event with no consideration for pause
    protected override void startEventHelper() {
        StartCoroutine(cameraPanSequence());
    }


    // Main function to end event with no consideration for pause
    protected override void endEventHelper() {
        gameObject.SetActive(false);
    }


    // Main function to do the camera pan sequence
    private IEnumerator cameraPanSequence() {
        // Main function to move camera
        float timeToPanToTarget = PlayerCameraController.moveCamera(
            cameraTarget,
            cameraRotationPitch,
            cameraRotationYaw,
            cameraZoom,
            cameraSpeed,
            cameraOffset
        );

        yield return new WaitForSecondsRealtime(timeToPanToTarget);

        // Stay on target
        yield return new WaitForSecondsRealtime(cameraPanStayTargetDuration);

        // Reset
        float timeToReset = PlayerCameraController.reset(cameraSpeed);
        yield return new WaitForSecondsRealtime(timeToReset);

        // Stay on player
        yield return new WaitForSecondsRealtime(cameraPanStayPlayerDuration);

        // Decide whether or not to finish unpause sequence immediately after camera shot or to set up a popup
        if (onboardingPopup == null) {
            endEvent();
        } else {
            Debug.Log("set active");
            onboardingPopup.gameObject.SetActive(true);
        }
    }
}
