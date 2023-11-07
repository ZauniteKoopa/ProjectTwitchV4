using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class TopDownMovementController3D : MonoBehaviour
{
    // Variables to control movement
    private bool isMoving = false;
    private Vector2 inputVector = Vector2.zero;
    private Vector3 movementForward = Vector3.forward;
    private Coroutine runningAutoMoveSequence = null;

    // Reference variables
    [Header("Player Package Components")]
    [SerializeField]
    private Transform cameraTransform;
    [SerializeField]
    private Transform playerCharacterTransform;
    private IAttackModule attackController;

    // Collision sensors
    [Header("Solid Enviornment Sensors")]
    [SerializeField]
    private IBlockerSensor frontSensor;
    [SerializeField]
    private IBlockerSensor backSensor;
    [SerializeField]
    private IBlockerSensor leftSensor;
    [SerializeField]
    private IBlockerSensor rightSensor;

    // Unit status
    private IUnitStatus unitStatus;


    // On awake, automate initialization of variables and error check
    private void Awake() {
        // Initialize variables
        attackController = GetComponent<IAttackModule>();

        // Error check
        if (cameraTransform == null)
            Debug.LogError("Cannot find camera to set movement forward to! Please link a camera to " + this, transform);
        
        if (playerCharacterTransform == null)
            Debug.LogError("Cannot find playerCharacter to alter forward! Please link character to " + this, transform);

        if (frontSensor == null || backSensor == null || leftSensor == null || rightSensor == null)
            Debug.LogError("Collision Sensors not connected to movement controller! Make sure sensors are connected to " + this, transform);

        if (attackController == null)
            Debug.LogWarning("Attack Controller not connected to object! Attacking will not affect movement for " + this, transform);

        // if (uiModule == null) {
        //     Debug.LogError("UI Module not connected to attack package for " + transform, transform);
        // }

        unitStatus = playerCharacterTransform.GetComponent<IUnitStatus>();
        if (unitStatus == null)
            Debug.LogError("Cannot access unit's movement stats! Please link a Unit Status to " + playerCharacterTransform, playerCharacterTransform);
    }


    // FixedUpdate function: runs every frame
    private void FixedUpdate() {
        // Only do movement controls if unit is alive
        if (unitStatus.canMove() && runningAutoMoveSequence == null) {
            if (isMoving) {
                handleMovement(Time.fixedDeltaTime);
            }

            setFacingDirection();
        }
    }

    // Main method to handle movement
    //  Pre: deltaTime > 0, inputVector is a normalized vector (NOT THE ZERO VECTOR), cameraTransform != null
    //  Post: Translates / rotates the transform of the player based on the input vector and the camera direction
    private void handleMovement(float deltaTime) {
        // Preconditions
        Debug.Assert(inputVector.magnitude > 0.999f && inputVector.magnitude <= 1.001f);
        Debug.Assert(deltaTime > 0.0f);
        Debug.Assert(cameraTransform != null);

        Vector3 movementWorldDir = getWorldMovementDirection();

        // Translate via the movement vector and change facing direction via the forward vector
        transform.Translate(movementWorldDir * getMovementSpeed() * deltaTime, Space.World);
        movementForward = getWorldForwardDirection();
    }

    // Main method to determine where the character is facing, considering both movement and aim
    //  Pre: movementForward and aim direction is a vector that's not the zero vector, playerTransform != null
    //  Post: Sets the facing direction of the player character, assuming that player always face to its relative +Z dir
    private void setFacingDirection() {
        Debug.Assert(playerCharacterTransform != null);

        Vector3 localForward = Vector3.zero;

        if (attackController != null && attackController.getNewForward(out localForward)) {
            playerCharacterTransform.forward = localForward;
        } else {
            playerCharacterTransform.forward = movementForward;
        }
    }


    // Main helper method to calculate the speed considering multiple different factors
    private float getMovementSpeed() {
        float baseMovement = unitStatus.getMovementSpeed();
        baseMovement *= (attackController == null) ? 1.0f : attackController.getMovementSpeedFactor();
        return baseMovement;
    }

    // Event handler for 4 axis movement
    public void onInputVectorChange(InputAction.CallbackContext value) {
        // Set flag for whether player is pressing button
        bool prevIsMoving = isMoving;
        isMoving = !value.canceled;

        // If you transitioned from two movements state, set moving
        if (isMoving != prevIsMoving) {
            unitStatus.setMoving(isMoving);
        }

        // Set inputVector value
        Vector2 eventVector = value.ReadValue<Vector2>();
        inputVector = eventVector;
    }


    // Main function to see if the unit is currently moving. Is used primarily for animators
    public bool isCurrentlyMoving() {
        if (runningAutoMoveSequence != null) {
            return true;
        }

        if (!isMoving) {
            return false;
        }

        Vector3 worldMoveDir = getWorldMovementDirection();
        return worldMoveDir.magnitude > 0.0001f;
    }


    // Main private helper function to get the world movement direction
    //  Post: converts raw input vector from player input to actual movement vectors in the world
    private Vector3 getWorldMovementDirection() {
        // Get input axis values
        float inputX = inputVector.x;
        float inputY = inputVector.y;

        // Get camera axis values
        Vector3 forwardVector = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        Vector3 rightVector = Vector3.Cross(Vector3.up, forwardVector).normalized;

        // Get movement vector by checking sensors
        float movementX = (inputX < 0 && leftSensor.isBlocked()) ? 0 : inputX;
        movementX = (movementX > 0 && rightSensor.isBlocked()) ? 0 : movementX;
        float movementY = (inputY < 0 && backSensor.isBlocked()) ? 0 : inputY;
        movementY = (movementY > 0 && frontSensor.isBlocked()) ? 0 : movementY;

        return (movementX * rightVector) + (movementY * forwardVector);
    }


    // Main private helper function to get the world forward direction of the player (what direction the player is facing)
    private Vector3 getWorldForwardDirection() {
        // Get input axis values
        float inputX = inputVector.x;
        float inputY = inputVector.y;
        
        // Get camera axis values
        Vector3 forwardVector = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up);
        Vector3 rightVector = Vector3.Cross(Vector3.up, forwardVector);

        // Get directional forward vector
        Vector3 forwardWorldDir = (inputX * rightVector) + (inputY * forwardVector);
        forwardWorldDir.Normalize();

        return forwardWorldDir;
    }


    // Public function to face the camera on an event
    public void faceCamera() {
        inputVector = Vector2.down;
        movementForward = Vector3.back;
    }


    // Main function to do the automove sequence
    //  Pre: assumes that the straightline path between player and tgt Location is not blocked. Should not run 2 autoMoves at the same time
    //  Post: returns the time it takes to move to that destination in seconds
    public float autoMove(Vector3 targetDestination) {
        if (runningAutoMoveSequence != null) {
            Debug.LogError("AN AUTO MOVE SEQUENCE IS STILL RUNNING?? DON'T RUN 2 AUTO MOVES AT ONCE. PLEASE DISABLE CONTROLS WHEN AUTO MOVE IS RUNNING");
            return -1f;
        }

        runningAutoMoveSequence = StartCoroutine(autoMoveSequence(targetDestination));
        return Vector3.Distance(targetDestination, transform.position) / 6f;
    }


    // Main sequence to automove (regardless of time)
    private IEnumerator autoMoveSequence(Vector3 targetDestination) {
        // setup
        float timeToMove = Vector3.Distance(targetDestination, transform.position) / 6f;
        Vector3 sourceLocation = transform.position;
        float timer = 0f;
        unitStatus.transform.forward = Vector3.ProjectOnPlane((targetDestination - sourceLocation), Vector3.up);

        // Move
        while (timer < timeToMove) {
            yield return 0;
            timer += Time.unscaledDeltaTime;
            transform.position = Vector3.Lerp(sourceLocation, targetDestination, timer / timeToMove);
        }

        // Cleanup
        transform.position = targetDestination;
        runningAutoMoveSequence = null;
    }
}
