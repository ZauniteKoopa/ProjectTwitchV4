using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerCameraController : MonoBehaviour
{
    // Static variables concerning camera transition
    private static PlayerCameraController mainPlayerCamera = null;
    private static Coroutine cameraTransitionCoroutine = null;

    // Static variables concerning original transition spot
    private static Transform playerPackage;
    private static Vector3 originalLocalPos;
    private static Quaternion originalLocalRot;
    private static Transform cameraPivot;


    // Static variables for time stop
    private static int numFramesPerSecond = 60;

    // Static variables for camera shake
    private static Coroutine cameraShakeCoroutine = null;
    private static int numFramesPerShake = 2;


    // On awake, set this to the PlayerCameraController
    private void Awake() {
        // Error check
        PlayerCameraController[] sceneCameraControllers = Object.FindObjectsOfType<PlayerCameraController>();
        if (sceneCameraControllers.Length > 1) {
            Debug.LogError("CANNOT HAVE MORE THAN 1 CAMERA CONTROLLER SCRIPT WITHIN THE SCENE");
        }

        // set to main player camera
        cameraPivot = transform.parent;
        playerPackage = cameraPivot.parent;
        originalLocalPos = transform.localPosition;
        originalLocalRot = transform.localRotation;
        mainPlayerCamera = this;

        Application.targetFrameRate = numFramesPerSecond;
    }


    // Static function to do camera coroutine sequence
    //  Pre: target is the target the camera will focus on, pitch is the x rot, yaw is the y rot, zoom is how zoomed in the camera is on target
    //       transSpeed is the transition speed of the camera
    //  Post: Moves the camera to focus on a specific target, with specified zoom, pitch (x rot), yaw (y rot). tranSpeed is the speed at which you move to a location
    public static void moveCamera(Transform target, float pitch, float yaw, float zoom, float transSpeed) {
        Debug.Assert(mainPlayerCamera != null);

        if (cameraTransitionCoroutine != null) {
            mainPlayerCamera.StopCoroutine(cameraTransitionCoroutine);
        }

        cameraTransitionCoroutine = mainPlayerCamera.StartCoroutine(mainPlayerCamera.moveCameraSequence(
            target,
            pitch,
            yaw,
            zoom,
            transSpeed
        ));
    }


    // Main static function to just move a camera instantly
    public static void instantMoveCamera(Transform target, float pitch, float yaw, float zoom) {
        cameraPivot.parent = target;
        cameraPivot.rotation = Quaternion.Euler(pitch, yaw, 0f);
        cameraPivot.localPosition = Vector3.zero;
        
        mainPlayerCamera.transform.localPosition = zoom * Vector3.back;
    }


    // Static function to reset the camera
    //  Pre: mainPlayerCamera != null
    //  Post: moves the camera back to the default position on top of the player
    // public static void reset() {
    //     Debug.Assert(mainPlayerCamera != null);
    //     moveCamera(playerPackage, originalLocalPos, originalLocalRot);
    // }


    // Static function to instantly reset the camera
    //  Pre: mainPlayerCamera != null
    //  Post: moves the camera back to the default position on top of the player
    public static void instantReset() {
        Debug.Assert(mainPlayerCamera != null);

        if (cameraTransitionCoroutine != null) {
            mainPlayerCamera.StopCoroutine(cameraTransitionCoroutine);
            cameraTransitionCoroutine = null;
        }
        
        mainPlayerCamera.transform.parent = playerPackage;
        mainPlayerCamera.transform.localPosition = originalLocalPos;
        mainPlayerCamera.transform.localRotation = originalLocalRot;
    }


    // Static function to get UI to face the camera, (object forward points in same direction as camera)
    //  Pre: object != null, mainPlayerCamera != null
    //  Post: Object now faces the camera
    public static void faceCamera(Transform facingObject) {

        // Get calculated X
        Vector3 rawForward = facingObject.position - mainPlayerCamera.transform.position;
        facingObject.forward = rawForward;
        float rotX = facingObject.eulerAngles.x;
        float rotY = 0f;
        float rotZ = 0f;

        // Create new rotation
        facingObject.rotation = Quaternion.Euler(rotX, rotY, rotZ);
    }


    // Main IE numerator to moving the camera
    //  Pre: parent is the transform you want the camera to parent to, localPosition is the local position of the camera relative to the parent, mainPlayerCamera != null
    //  Post: moves the camera
    private IEnumerator moveCameraSequence(Transform target, float pitch, float yaw, float zoom, float transSpeed) {
        // Calculate the time to be in final position and calculate positions points for PIVOT (actual camera does no position change)
        Vector3 globalPivotStart = transform.position;
        Vector3 globalPivotFinish = target.position;
        float pivotDist = Vector3.Distance(globalPivotStart, globalPivotFinish);
        float pivotTime = pivotDist / transSpeed;

        // Calculate the time to be in final zoom position of camera
        Vector3 cameraLocalZoomStart = transform.localPosition;
        Vector3 cameraZoomFinish = transform.parent.TransformPoint(zoom * Vector3.back);
        float zoomDist = Vector3.Distance(transform.position, cameraZoomFinish);
        float zoomTime = zoomDist / transSpeed;
        float usedTime = Mathf.Max(zoomTime, pivotTime);

        // Get rotation starts
        Quaternion rotStart = cameraPivot.rotation;
        Quaternion rotFinish = Quaternion.Euler(pitch, yaw, 0f);

        // Transition timer and set up
        cameraPivot.parent = target;
        float timer = 0f;
        WaitForSecondsRealtime waitFrame = new WaitForSecondsRealtime(0.04f);

        while (timer < usedTime) {
            yield return waitFrame;

            timer += 0.04f;

            // Update camera pivot and rotation
            cameraPivot.position = Vector3.Lerp(globalPivotStart, globalPivotFinish, timer / usedTime);
            cameraPivot.rotation = Quaternion.Lerp(rotStart, rotFinish, timer / usedTime);

            // Update camera zoom
            transform.localPosition = Vector3.Lerp(cameraLocalZoomStart, zoom * Vector3.back, timer / usedTime);
        }

        // Finish off transition
        transform.localPosition = zoom * Vector3.back;
        cameraPivot.position = globalPivotFinish;
        cameraTransitionCoroutine = null;
    }


    // Main function to start hit stop
    //  Pre: numFrames >= 0 (60 FPS)
    //  Post: the game will freeze time for a specified number of frames
    public static void hitStop(int numFrames) {
        Debug.Assert(numFrames >= 0);

        if (numFrames > 0) {
            mainPlayerCamera.StartCoroutine(mainPlayerCamera.hitStopSequence(numFrames));
        }
    }


    // Main IEnumerator
    private IEnumerator hitStopSequence(int numFrames) {
        float timePerFrame = 1f / (float)(numFramesPerSecond);
        Time.timeScale = 0f;

        for (int f = 0; f < numFrames; f++) {
            yield return new WaitForSecondsRealtime(timePerFrame);
        }

        Time.timeScale = 1f;
    }


    // Static function to do a camera shake sequence
    //  Pre: shakeFrameDuration >= 0 and shakeMagnitude >= 0
    //  Post: Moves the camera to specific position
    public static void shakeCamera(int shakeFrameDuration, float shakeMagnitude) {
        Debug.Assert(mainPlayerCamera != null);
        Debug.Assert(shakeFrameDuration >= 0 && shakeMagnitude >= 0f);

        if (shakeFrameDuration > 0 && shakeMagnitude > 0f) {
            if (cameraShakeCoroutine != null) {
                mainPlayerCamera.StopCoroutine(cameraShakeCoroutine);
            }

            cameraShakeCoroutine = mainPlayerCamera.StartCoroutine(mainPlayerCamera.cameraShakeSequence(shakeFrameDuration, shakeMagnitude));
        }
    }


    // Main IEnumerator to do camera shake
    private IEnumerator cameraShakeSequence(int shakeFrameDuration, float maxShakeMagnitude) {
        float timePerFrame = 1f / (float)(numFramesPerSecond);
        Vector3 originalPos = transform.localPosition; 

        for (int f = 0; f < shakeFrameDuration; f++) {
            yield return new WaitForSecondsRealtime(timePerFrame);

            if (f % numFramesPerShake == 0) {
                Vector3 shakeDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f).normalized;
                float curShakeMagnitude = Mathf.Lerp(1f, 0f, (float)f / (float)shakeFrameDuration) * maxShakeMagnitude;

                transform.localPosition = originalPos;
                transform.Translate(curShakeMagnitude * shakeDir);
            }
        }


        transform.localPosition = originalPos;
        cameraShakeCoroutine = null;
    }
}
