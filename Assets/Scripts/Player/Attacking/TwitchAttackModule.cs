using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.Events;

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
    [SerializeField]
    private PlayerScreenUI screenUI;
    private MeshRenderer render;
    private TwitchInventory inventory;
    private PlayerAudioManager audioManager;
    private PlayerStatus status;

    
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
    [SerializeField]
    private IAimAssist aimAssist;

    [Header("Contaminate Frame Data")]
    [SerializeField]
    [Min(1)]
    private int contaminateStartFrames = 1;
    [SerializeField]
    [Min(1)]
    private int contaminateEndFrames = 1;
    [SerializeField]
    private ContaminateManager contaminateZone = null;


    // Variables for Ambush
    [Header("Ambush")]
    [SerializeField]
    [Min(0.1f)]
    private float ambushStartupTime = 1f;
    [SerializeField]
    [Min(0.1f)]
    private float ambushInvisibilityTime = 10f;
    [SerializeField]
    [Min(0.1f)]
    private float ambushAttackSpeedBuffTime = 5f;
    [SerializeField]
    [Min(1f)]
    private float ambushAttackSpeedBuff = 1.5f;
    [SerializeField]
    [Min(1.01f)]
    private float ambushInvisibilityMovementBuff = 1.2f;
    [SerializeField]
    private InvisibilitySensor ambushProximitySensor = null;
    [SerializeField]
    private GameObject ambushBuffVisualEffect = null;
    private Coroutine runningAmbushSequence = null;


    [Header("Cask collision aiming")]
    [SerializeField]
    [Min(0.01f)]
    private float caskThrowCollisionOffset = 0.5f;
    [SerializeField]
    private float caskThrowRange = 7f;
    [SerializeField]
    private LayerMask throwOffsetCollisionMask;

    [Header("Error messaging")]
    [SerializeField]
    private string contaminateErrorMessage = "No poisoned units nearby to contaminate";

    // Variables for attacking
    private Coroutine runningAttackSequence;
    private bool holdingFireButton;
    private Vector2 inputMouseCoordinates;
    private TwitchMovementState movementState = TwitchMovementState.MOVING;
    private Vector3 attackAnimForward = Vector3.forward;


    // On awake, error check
    private void Start() {
        if (defaultWeakBolt == null) {
            Debug.LogError("ERROR: Set a default weak bolt for Twitch just in case he has no vial!");
        }

        if (playerCamera == null) {
            Debug.LogError("No player camera attached to this attack module");
        }

        if (playerCharacter == null) {
            Debug.LogError("No player character attached to this attack module");
        }

        if (screenUI == null) {
            Debug.LogError("No player screen UI detected");
        }

        render = playerCharacter.GetComponent<MeshRenderer>();
        status = playerCharacter.GetComponent<PlayerStatus>();
        audioManager = playerCharacter.GetComponent<PlayerAudioManager>();
        inventory = GetComponent<TwitchInventory>();

        if (inventory == null) {
            Debug.LogError("No Twitch Inventory found for this Attack Module");
        }

        if (audioManager == null) {
            Debug.LogError("No player audio manager found on player character transform");
        }

        if (ambushProximitySensor == null) {
            Debug.LogError("No InvisibilitySensor attached to character for ambush");
        }
        ambushProximitySensor.displaySensor(false);
    }


    // On update, check if you're holding fire
    private void Update() {
        if (holdingFireButton && runningAttackSequence == null && movementState != TwitchMovementState.IN_ATTACK_ANIM) {
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
            int startFrames = (inventory.carryingPrimaryVial()) ? inventory.getPrimaryStartFrame() : weakBoltStartFrames;
            int endFrames = (inventory.carryingPrimaryVial()) ? inventory.getPrimaryEndFrame() : weakBoltEndFrames;
            startFrames = applyAttackSpeedModifier(startFrames);
            endFrames = applyAttackSpeedModifier(endFrames);

            // Startup should be interruptable
            render.material.color = Color.blue;
            primaryFireTrigger.Invoke();
            yield return waitForFrames(startFrames, () => !holdingFireButton);

            // Only fire if you commited to it
            if (holdingFireButton) {
                // Get aiming direction
                Vector3 boltDir = getWorldAimLocation() - playerCharacter.position;
                if (aimAssist != null) {
                    boltDir = aimAssist.adjustAim(boltDir, transform.position);
                }

                // Try to fire the bullet using the primary bolt. If that's not successful, fire a dead bullet
                if (!inventory.firePrimaryBolt(boltDir, playerCharacter, status.getBaseAttack())) {
                    IPrimaryAttack curBolt = Object.Instantiate(defaultWeakBolt, playerCharacter.position, Quaternion.identity);
                    curBolt.setUp(boltDir, weakBoltDamage * status.getBaseAttack(), null);
                }

                inventory.playLaunchPoisonBoltSound();

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


    // Main secondary fire sequence
    //  Pre: inventory.canFireSecondaryLob is true
    //  Post: fires lob with startUp and end frames
    private IEnumerator secondaryFireSequence() {
        Debug.Assert(inventory.canFireSecondaryLob());

        // Set up
        Vector3 tgt = caskAimPosition();
        attackAnimForward = (tgt - transform.position).normalized;
        movementState = TwitchMovementState.IN_ATTACK_ANIM;

        // Obtain frame data
        int startFrames = applyAttackSpeedModifier(inventory.getSecondaryAttackStartFrames());
        int endFrames = applyAttackSpeedModifier(inventory.getSecondaryAttackEndFrames());

        // Startup
        audioManager.playLobCaskSound();
        render.material.color = Color.blue;
        abilityOneTrigger.Invoke();
        yield return waitForFrames(startFrames);

        // Lob
        inventory.fireSecondaryLob(tgt, playerCharacter);

        // Ending
        render.material.color = Color.magenta;
        yield return waitForFrames(endFrames);

        // Cleanup
        render.material.color = Color.green;
        movementState = TwitchMovementState.MOVING;
        runningAttackSequence = null;
    }


    // Main Contaminate sequence
    //  Pre: contaminateZone.contaminateTargetsFound is true and contaminate is off cooldown
    //  Post: contaminates all infected units
    private IEnumerator contaminateSequence() {
        Debug.Assert(contaminateZone.contaminateTargetsFound());

        // Set up
        attackAnimForward = playerCharacter.forward;
        movementState = TwitchMovementState.IN_ATTACK_ANIM;

        // Startup
        audioManager.playContaminateSound();
        render.material.color = Color.blue;
        abilityTwoTrigger.Invoke();
        yield return waitForFrames(contaminateStartFrames);

        // Contaminate
        contaminateZone.contaminateAll();

        // Ending
        render.material.color = Color.magenta;
        yield return waitForFrames(contaminateEndFrames);

        // Cleanup
        render.material.color = Color.green;
        movementState = TwitchMovementState.MOVING;
        runningAttackSequence = null;
    }


    // Main function to do ambush sequence
    //  Pre: none
    //  Post: does ambush sequence
    private IEnumerator ambushSequence() {
        // Startup
        if (ambushBuffVisualEffect != null) {
            ambushBuffVisualEffect.SetActive(false);
        }
        audioManager.playAmbushStartup();
        yield return new WaitForSeconds(ambushStartupTime);


        status.applySpeedModifier(ambushInvisibilityMovementBuff);
        status.invisible = true;
        screenUI.displayAmbushInvisibility();
        ambushProximitySensor.displaySensor(true);

        // Timer to wait out invisibility
        float timer = 0f;
        while (timer < ambushInvisibilityTime && movementState == TwitchMovementState.MOVING) {
            yield return 0;
            timer += Time.deltaTime;
            screenUI.setInvisBarFill(timer, ambushInvisibilityTime);
        }

        // Attack speed buff
        status.invisible = false;
        status.revertSpeedModifier(ambushInvisibilityMovementBuff);
        runningAmbushSequence = null;
        inventory.activateAmbushCooldown();
        screenUI.removeAmbushInvisibility();
        ambushProximitySensor.displaySensor(false);

        status.applyAttackSpeedEffect(ambushAttackSpeedBuff);
        audioManager.playAmbushBuff();
        if (ambushBuffVisualEffect != null) {
            ambushBuffVisualEffect.SetActive(true);
        }
        yield return new WaitForSeconds(ambushAttackSpeedBuffTime);

        // Cleanup
        if (ambushBuffVisualEffect != null) {
            ambushBuffVisualEffect.SetActive(false);
        }
        status.revertAttackSpeedEffect(ambushAttackSpeedBuff);
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


    // Function to see if player is still shooting
    //  Pre: none
    //  Post: returns if player is shooting
    public override bool isShooting() {
        return movementState == TwitchMovementState.FIRING;
    }

    // Main function to see if you're in the middle of dashing or not
    public override bool isDashing() {
        return runningAmbushSequence != null;
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


    // Event handler method for when secondary fire button click
    public void onSecondaryButtonAction(InputAction.CallbackContext value) {
        if (value.started && movementState != TwitchMovementState.IN_ATTACK_ANIM && !inUninterruptableAnimationSequence) {
            // Check if you can actually fire
            if (inventory.canFireSecondaryLob()) {
                // Cancel running attack sequence
                if (runningAttackSequence != null) {
                    StopCoroutine(runningAttackSequence);
                }

                // Set this as the runnning attack sequence
                runningAttackSequence = StartCoroutine(secondaryFireSequence());

            }
        }
    }


    // Main event handler function for stealth
    public void onSwapVialButtonAction(InputAction.CallbackContext value) {
        if (value.started && movementState != TwitchMovementState.IN_ATTACK_ANIM) {
            inventory.swapVials();
        }
    }


    // Event handler method for when secondary fire button click
    public void onContaminateButtonAction(InputAction.CallbackContext value) {
        if (value.started && movementState != TwitchMovementState.IN_ATTACK_ANIM && !inUninterruptableAnimationSequence) {
            bool poisonedUnitsNearby = contaminateZone.contaminateTargetsFound();

            // Check if you can actually fire
            if (inventory.canContaminate() && poisonedUnitsNearby) {
                // Cancel running attack sequence
                if (runningAttackSequence != null) {
                    StopCoroutine(runningAttackSequence);
                }

                // Set this as the runnning attack sequence
                inventory.activateContaminationCooldown();
                runningAttackSequence = StartCoroutine(contaminateSequence());

            } else if (!poisonedUnitsNearby) {
                screenUI.displayErrorMessage(contaminateErrorMessage);
            }
        }
    }


    // Main event handler function for stealth
    public void onAmbushButtonAction(InputAction.CallbackContext value) {
        if (value.started && inventory.canAmbush() && runningAmbushSequence == null && !inUninterruptableAnimationSequence) {
            runningAmbushSequence = StartCoroutine(ambushSequence());
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


    // Main function to get cask aim position
    private Vector3 caskAimPosition() {
        Vector3 rawAimPosition = getWorldAimLocation();
        Vector3 rawAimDirection = (rawAimPosition - playerCharacter.position).normalized;
        float maxDist = Mathf.Min(Vector3.Distance(rawAimPosition, transform.position), caskThrowRange);
        RaycastHit hitInfo;

        if (Physics.Raycast(playerCharacter.position, rawAimDirection, out hitInfo, maxDist, throwOffsetCollisionMask)) {
            return hitInfo.point - (caskThrowCollisionOffset * rawAimDirection);
        } else {
            return playerCharacter.position + (maxDist * rawAimDirection);
        }
    }


    // Private helper method to wait for a specified amount of frames
    //  Pre: numFrames > 0
    //  Post: wait a number amount of frames before moving on after this sequence
    private IEnumerator waitForFrames(int numFrames, System.Func<bool> interrupted = null) {
        Debug.Assert(numFrames > 0);

        int f = 0;

        while (f < numFrames && (interrupted == null || !interrupted())) {
            yield return 0;
            if (Time.timeScale != 0f) {
                f++;
            }
        }
    }


    // Private helper function to get the number of frames after applying the attack speed modifier
    //  Pre: framesEntered is the number of frames before being modified
    //  Post: returns the frameData after the modifier is applied
    private int applyAttackSpeedModifier(int frameData) {
        float attackSpeedFactor = 1f / status.getAttackSpeedModifier();
        float rawFrameData = (float)frameData * attackSpeedFactor;

        return (int)Mathf.Ceil(rawFrameData);
    }

}
