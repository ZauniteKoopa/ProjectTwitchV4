using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerCameraController : MonoBehaviour
{
    // Static variables concerning camera transition
    private static PlayerCameraController mainPlayerCamera = null;
    private static Coroutine cameraTransitionCoroutine = null;
    private readonly static float TRANSITION_SPEED = 30f;

    // Static variables concerning original transition spot
    private static Transform playerPackage;
    private static Vector3 originalLocalPos;
    private static Quaternion originalLocalRot;


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
        playerPackage = transform.parent;
        originalLocalPos = transform.localPosition;
        originalLocalRot = transform.localRotation;
        mainPlayerCamera = this;

        Application.targetFrameRate = numFramesPerSecond;
    }


    // Static function to do camera coroutine sequence
    //  Pre: parent is the transform you want the camera to parent to, localPosition is the local position of the camera relative to the parent, mainPlayerCamera != null
    //  Post: Moves the camera to specific position
    public static void moveCamera(Transform parent, Vector3 localPosition, Quaternion localRotation) {
        Debug.Assert(mainPlayerCamera != null);

        if (cameraTransitionCoroutine != null) {
            mainPlayerCamera.StopCoroutine(cameraTransitionCoroutine);
        }

        cameraTransitionCoroutine = mainPlayerCamera.StartCoroutine(mainPlayerCamera.moveCameraSequence(parent, localPosition, localRotation));
    }


    // Static function to reset the camera
    //  Pre: mainPlayerCamera != null
    //  Post: moves the camera back to the default position on top of the player
    public static void reset() {
        Debug.Assert(mainPlayerCamera != null);
        moveCamera(playerPackage, originalLocalPos, originalLocalRot);
    }


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
    private IEnumerator moveCameraSequence(Transform parent, Vector3 localPosition, Quaternion localRotation) {
        // Set the parent of the camera
        transform.parent = parent;

        // Calculate the time it takes to get to position with speed
        Vector3 globalStart = transform.position;
        Vector3 globalFinish = (parent == null) ? localPosition : parent.TransformPoint(localPosition);
        float dist = Vector3.Distance(globalStart, globalFinish);
        float time = dist / TRANSITION_SPEED;

        // Get rotation starts
        Quaternion rotStart = transform.localRotation;
        Quaternion rotFinish = localRotation;

        // Transition timer
        float timer = 0f;
        WaitForEndOfFrame waitFrame = new WaitForEndOfFrame();

        while (timer < time) {
            yield return waitFrame;

            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(globalStart, globalFinish, timer / time);
            transform.localRotation = Quaternion.Lerp(rotStart, rotFinish, timer / time);
        }

        // Finish off transition
        transform.position = globalFinish;
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
