using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

public enum TwitchMovementState {
    MOVING,
    FIRING,
    IN_ATTACK_ANIM
}

public class TwitchAttackModule : IAttackModule
{
    [Header("Reference Variables")]
    [SerializeField]
    private Transform playerCharacter;
    [SerializeField]
    private Camera playerCamera;
    private MeshRenderer render;

    
    [Header("Weak Bolt Properties")]
    [SerializeField]
    private IPrimaryAttack defaultWeakBolt;
    [SerializeField]
    [Min(0.01f)]
    private float weakBoltDamage = 0.01f;
    [SerializeField]
    [Min(1)]
    private int weakBoltStartFrames = 20;
    [SerializeField]
    [Min(1)]
    private int weakBoltEndFrames = 10;
    [SerializeField]
    [Range(0f, 1f)]
    private float weakBoltMovementReduction = 0.75f;

    // Variables for attacking
    private Coroutine runningAttackSequence;
    private bool holdingFireButton;
    private Vector2 inputMouseCoordinates;
    private TwitchMovementState movementState = TwitchMovementState.MOVING;
    private Vector3 attackAnimForward = Vector3.forward;


    // On awake, error check
    private void Awake() {
        if (defaultWeakBolt == null) {
            Debug.LogError("ERROR: Set a default weak bolt for Twitch just in case he has no vial!");
        }

        if (playerCamera == null) {
            Debug.LogError("No player camera attached to this attack module");
        }

        if (playerCharacter == null) {
            Debug.LogError("No player character attached to this attack module");
        }

        render = playerCharacter.GetComponent<MeshRenderer>();
        Application.targetFrameRate = 60;
    }


    // On update, check if you're holding fire
    private void Update() {
        if (holdingFireButton && runningAttackSequence == null) {
            runningAttackSequence = StartCoroutine(primaryFireSequence());
        }
    }


    // Main primary weapon fight sequence
    //  Pre: none
    //  Post: fire sequence
    private IEnumerator primaryFireSequence() {
        // Set movement state
        movementState = TwitchMovementState.FIRING;

        // While you're still holding on to the button
        while (holdingFireButton) {
            // Get data
            int startFrames = weakBoltStartFrames;
            int endFrames = weakBoltEndFrames;

            // Startup should be interruptable
            render.material.color = Color.blue;
            yield return waitForFrames(startFrames, () => !holdingFireButton);

            // Only fire if you commited to it
            if (holdingFireButton) {
                // Fire bullet
                Vector3 boltDir = getWorldAimLocation() - playerCharacter.position;
                IPrimaryAttack curBolt = UnityEngine.Object.Instantiate(defaultWeakBolt, playerCharacter.position, Quaternion.identity);
                curBolt.setUp(boltDir, weakBoltDamage);

                // End
                render.material.color = Color.magenta;
                yield return waitForFrames(endFrames);
            }
        }

        // Reset
        render.material.color = Color.green;
        movementState = TwitchMovementState.MOVING;
        runningAttackSequence = null;
    }

    
    // Function to return movement speed factor affected by this attack module
    //  Pre: none
    //  Post: returns a float that tells how much movement speed should be reduced by currently
    public override float getMovementSpeedFactor() {
        if (movementState == TwitchMovementState.IN_ATTACK_ANIM) {
            return 0f;

        } else if (movementState == TwitchMovementState.FIRING) {
            return weakBoltMovementReduction;

        } else {
            return 1f;

        }
    }


    // Function to get the new forward calculated by this attack module
    //  Pre: newForward needs to be any vector3
    //  Post: returns whether forward should be overriden and puts overriden forward into newForward
    public override bool getNewForward(out Vector3 newForward) {
        newForward = (movementState == TwitchMovementState.FIRING) ? getWorldAimLocation() - transform.position : attackAnimForward;
        return movementState != TwitchMovementState.MOVING;
    }


    // Event handler method for when mouse position changes
    public void onAimPositionChange(InputAction.CallbackContext value) {
        inputMouseCoordinates = value.ReadValue<Vector2>();
    }


    // Event handler method for when primary fire button click / removed
    public void onPrimaryButtonAction(InputAction.CallbackContext value) {

        // Only run the sequence
        if (value.started) {
            holdingFireButton = true;
        
        // Once left click is canceled, turn mouse hold flag off
        } else if (value.canceled) {
            holdingFireButton = false;
        }
    }


    // Private helper method to get world aim location
    //  Does so by creating a ray on mouse position on camera and have it intersect the aim plane
    private Vector3 getWorldAimLocation() {
        Ray inputRay = playerCamera.ScreenPointToRay(inputMouseCoordinates);
        float intersectionDist = 0.0f;
        Plane aimPlane = new Plane(Vector3.up, playerCharacter.position);

        aimPlane.Raycast(inputRay, out intersectionDist);
        return inputRay.GetPoint(intersectionDist);
    }


    // Private helper method to wait for a specified amount of frames
    //  Pre: numFrames > 0
    //  Post: wait a number amount of frames before moving on after this sequence
    private IEnumerator waitForFrames(int numFrames, Func<bool> interrupted = null) {
        Debug.Assert(numFrames > 0);

        int f = 0;

        while (f < numFrames && (interrupted == null || !interrupted())) {
            yield return 0;
            f++;
        }
    }

}
