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
    private static Coroutine runningCameraRoomSequence = null;
    private static bool overrideRoomCamera = false;

    // Static variables for time stop
    private static int numFramesPerSecond = 60;

    // Static variables for camera shake
    private static Coroutine cameraShakeCoroutine = null;
    private static int numFramesPerShake = 2;

    // Static variables for room mode
    private static readonly float CAMERA_OFFSET_X = 8f;
    private static readonly float CAMERA_OFFSET_Z = 5f;


    // Main runtime variable for getting the runtime zoom (you can get actual position by multiplying this value by Vector3.back)
    public float curRuntimeZoom = 0f;

    // Default variables for resetting
    private static float defaultPitch;
    private static float defaultYaw;
    private static float defaultZoom;
    private static Vector3 defaultTgtLocalPos;


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

        // set default variables
        defaultPitch = cameraPivot.eulerAngles.x;
        defaultYaw = cameraPivot.eulerAngles.y;
        defaultZoom = -transform.localPosition.z;
        defaultTgtLocalPos = cameraPivot.localPosition;

        // Routines
        runningCameraRoomSequence = null;
        cameraTransitionCoroutine = null;

        // Set runtime variables
        curRuntimeZoom = -transform.localPosition.z;
        Application.targetFrameRate = numFramesPerSecond;
    }



    // Static function to do camera coroutine sequence
    //  Pre: target is the target the camera will focus on, pitch is the x rot, yaw is the y rot, zoom is how zoomed in the camera is on target
    //       transSpeed is the transition speed of the camera
    //  Post: Moves the camera to focus on a specific target, with specified zoom, pitch (x rot), yaw (y rot). tranSpeed is the speed at which you move to a location
    public static void moveCamera(Transform target, float pitch, float yaw, float zoom, float transSpeed, Vector3 tgtLocalPosition) {
        Debug.Assert(mainPlayerCamera != null);

        if (cameraTransitionCoroutine != null) {
            mainPlayerCamera.StopCoroutine(cameraTransitionCoroutine);
        }

        cameraTransitionCoroutine = mainPlayerCamera.StartCoroutine(mainPlayerCamera.moveCameraSequence(
            target,
            pitch,
            yaw,
            zoom,
            transSpeed,
            tgtLocalPosition
        ));
    }


    // Main static function to just move a camera instantly
    public static void instantMoveCamera(Transform target, float pitch, float yaw, float zoom, Vector3 tgtLocalPos) {
        if (cameraTransitionCoroutine != null) {
            mainPlayerCamera.StopCoroutine(cameraTransitionCoroutine);
            cameraTransitionCoroutine = null;
        }
        
        cameraPivot.parent = target;
        cameraPivot.rotation = Quaternion.Euler(pitch, yaw, 0f);
        cameraPivot.localPosition = tgtLocalPos;
        
        mainPlayerCamera.transform.localPosition = zoom * Vector3.back;
        mainPlayerCamera.curRuntimeZoom = zoom;
    }


    // Static function to reset the camera
    //  Pre: mainPlayerCamera != null
    //  Post: moves the camera back to the default position on top of the player
    public static void reset(float transSpeed) {
        Debug.Assert(mainPlayerCamera != null && transSpeed > 0f);
        moveCamera(
            playerPackage,
            defaultPitch,
            defaultYaw,
            defaultZoom,
            transSpeed,
            defaultTgtLocalPos
        );
    }


    // // Static function to instantly reset the camera
    // //  Pre: mainPlayerCamera != null
    // //  Post: moves the camera back to the default position on top of the player
    public static void instantReset() {
        Debug.Assert(mainPlayerCamera != null);
        instantMoveCamera(
            playerPackage,
            defaultPitch,
            defaultYaw,
            defaultZoom,
            defaultTgtLocalPos
        );
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
    private IEnumerator moveCameraSequence(Transform target, float pitch, float yaw, float zoom, float transSpeed, Vector3 tgtLocalPos) {
        // Calculate the time to be in final position and calculate positions points for PIVOT (actual camera does no position change)
        Vector3 globalPivotStart = transform.parent.position;
        Vector3 globalPivotFinish = target.TransformPoint(tgtLocalPos);
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
        WaitForSecondsRealtime waitFrame = new WaitForSecondsRealtime(0.016f);

        while (timer < usedTime) {
            yield return waitFrame;

            timer += 0.016f;

            // Update camera pivot and rotation
            cameraPivot.position = Vector3.Lerp(globalPivotStart, globalPivotFinish, timer / usedTime);
            cameraPivot.rotation = Quaternion.Lerp(rotStart, rotFinish, timer / usedTime);

            // Update camera zoom (zoom tends to be positive, but in the transform its negative)
            curRuntimeZoom = Mathf.Lerp(-cameraLocalZoomStart.z, zoom, timer / usedTime);
            transform.localPosition = curRuntimeZoom * Vector3.back;
        }

        // Finish off transition
        transform.localPosition = zoom * Vector3.back;
        curRuntimeZoom = zoom;
        cameraPivot.position = globalPivotFinish;
        cameraTransitionCoroutine = null;
    }


    // Main function to start the room sequence
    public static void startCameraRoomSequence(Room tgtRoom, bool transition) {
        if (runningCameraRoomSequence != null) {
            mainPlayerCamera.StopCoroutine(runningCameraRoomSequence);
        }

        runningCameraRoomSequence = mainPlayerCamera.StartCoroutine(mainPlayerCamera.cameraRoomSequence(tgtRoom, transition));
    }



    // Private IEnumerator for the running room sequence that should go on forever until the end of the dungeon
    private IEnumerator cameraRoomSequence(Room tgtRoom, bool gradualTransition = true) {
        while (true) {
            cameraPivot.parent = tgtRoom.transform;

            // Transition to current room quickly if established
            if (gradualTransition && !overrideRoomCamera) {
                Time.timeScale = 0f;
                yield return moveCameraSequence(
                    tgtRoom.transform,
                    defaultPitch,
                    defaultYaw,
                    defaultZoom,
                    30f,
                    getLocalCameraPivotRoomPosition(tgtRoom)
                );
                Time.timeScale = 1f;
            }

            // Afterwards, keep local position of camera pivot until someone overrides camera sequence
            while (!overrideRoomCamera) {
                cameraPivot.localPosition = getLocalCameraPivotRoomPosition(tgtRoom);
                yield return 0;
            }

            // If someone overrides room camera, wait until the override stops
            while (overrideRoomCamera) {
                yield return 0;
            }

            // Set gradual transition to true for a gradual transition back to room mode
            gradualTransition = true;
        }
    }


    // Main private helper function to get the local coordinates of the current room
    private Vector3 getLocalCameraPivotRoomPosition(Room curRoom) {
        // Calculate room limits 
        float xRoomLimits = Mathf.Max((Room.ROOM_SIZE / 2f) - CAMERA_OFFSET_X, 0f);
        float zRoomLimits = Mathf.Max((Room.ROOM_SIZE / 2f) - CAMERA_OFFSET_Z, 0f);

        // Get the players position given that he's in this room
        Vector3 playerRawLocalPos = curRoom.transform.InverseTransformPoint(playerPackage.position);

        // Return a clamped version of this raw local position
        return new Vector3(
            Mathf.Clamp(playerRawLocalPos.x, -xRoomLimits, xRoomLimits),
            playerRawLocalPos.y,
            Mathf.Clamp(playerRawLocalPos.z, -zRoomLimits, zRoomLimits)
        );
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

        for (int f = 0; f < shakeFrameDuration; f++) {
            yield return new WaitForSecondsRealtime(timePerFrame);

            if (f % numFramesPerShake == 0) {
                Vector3 shakeDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f).normalized;
                float curShakeMagnitude = Mathf.Lerp(1f, 0f, (float)f / (float)shakeFrameDuration) * maxShakeMagnitude;

                transform.localPosition = curRuntimeZoom * Vector3.back;
                transform.Translate(curShakeMagnitude * shakeDir);
            }
        }


        transform.localPosition = curRuntimeZoom * Vector3.back;
        cameraShakeCoroutine = null;
    }
}
