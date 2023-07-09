using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwitchAnimatorController : MonoBehaviour
{
    private Animator animator;

    [Header("Functional Modules")]
    [SerializeField]
    private TopDownMovementController3D movementModule;
    [SerializeField]
    private IAttackModule attackModule;
    [SerializeField]
    private PlayerStatus twitchStatus;
    [SerializeField]
    private TwitchInventory inventoryModule;
    [SerializeField]
    private PlayerAudioManager audioModule;
    [SerializeField]
    private PlayerScreenUI screenUiModule;

    [Header("Animator Parameter Names")]
    [SerializeField]
    private string movementBoolParameter;
    [SerializeField]
    private string shootingBoolParameter;
    [SerializeField]
    private string primaryAttackAnimationTypeParameter;
    [SerializeField]
    private string ambushBoolParameter;
    [SerializeField]
    private string caskThrowTriggerParameter;
    [SerializeField]
    private string contaminateTriggerParameter;
    [SerializeField]
    private string craftStartTriggerParameter;
    [SerializeField]
    private string craftEndTriggerParameter;
    [SerializeField]
    private string sideEffectTriggerParameter;
    [SerializeField]
    private string randomIntParameter;

    [Header("Obtaining side effect")]
    [SerializeField]
    [Min(0.1f)]
    private float sideEffectFreezeTime = 1.5f;
    [SerializeField]
    [Range(1f, 30f)]
    private float sideEffectCameraZoom = 10f;
    [SerializeField]
    [Range(0f, 90f)]
    private float sideEffectCameraPitchAngle = 48f;
    [SerializeField]
    [Min(0.1f)]
    private float sideEffectCameraTransitionSpeed = 500f;


    [Header("Hurt animation")]
    [SerializeField]
    [Min(1)]
    private int numHurtAnimations = 1;
    [SerializeField]
    [Min(0)]
    private int cameraShakeFrames = 0;
    [SerializeField]
    [Min(0)]
    private int hitStopFrames = 0;
    [SerializeField]
    [Range(0f, 1.5f)]
    private float cameraShakeMagnitude = 0f;
    [SerializeField]
    private string hurtTriggerName;
    [SerializeField]
    private string unHurtTriggerName;


    [Header("Death Animation")]
    [SerializeField]
    private string deathTriggerName;
    [SerializeField]
    [Min(0)]
    private int deathCameraShakeFrames = 0;
    [SerializeField]
    [Range(0f, 1.5f)]
    private float deathCameraShakeMagnitude = 0f;
    [SerializeField]
    [Min(0f)]
    private float deathDelayBetweenAnimAndShake = 0f;
    [SerializeField]
    [Range(0.2f, 1f)]
    private float deathSlowMotionFactor = 0.6f;
    [SerializeField]
    [Min(0.1f)]
    private float timeUntilDeathFadeOut = 5f;
    [SerializeField]
    [Range(4f, 25f)]
    private float deathZoom = 12f;
    [SerializeField]
    [Range(0f, 70f)]
    private float deathAngle = 48f;
    [SerializeField]
    [Min(0.01f)]
    private float deathFadeDuration = 0.75f;
    

    // On awake, set everything
    private void Awake() {
        animator = GetComponent<Animator>();

        if (animator == null) {
            Debug.LogError("No animator to control!!");
        }

        if (movementModule == null || attackModule == null || twitchStatus == null || inventoryModule == null) {
            Debug.LogError("Missing Modules Found - Make sure that all modules are filled for animator to work properly");
        }

        // Listen to events from attack module
        attackModule.abilityOneTrigger.AddListener(onCaskThrowTrigger);
        attackModule.abilityTwoTrigger.AddListener(onContaminateTrigger);
        attackModule.primaryFireTrigger.AddListener(onBeginBulletFire);

        // Listen to events from inventory module for crafting
        inventoryModule.startCraftEvent.AddListener(onCraftStart);
        inventoryModule.endCraftEvent.AddListener(onCraftEnd);
        inventoryModule.obtainedSideEffect.AddListener(onObtainSideEffect);


        // Player status
        twitchStatus.playerHurtEvent.AddListener(onTwitchHurt);
        twitchStatus.deathEvent.AddListener(onTwitchDeath);
    }


    // On update, update variables concerning the movement state
    private void Update() {
        animator.SetBool(movementBoolParameter, movementModule.isCurrentlyMoving());
        animator.SetBool(shootingBoolParameter, attackModule.isShooting());
        animator.SetBool(ambushBoolParameter, attackModule.isDashing());

        if (twitchStatus.isAlive()) {
            if (animator.GetBool(shootingBoolParameter)) {
                animator.speed = twitchStatus.getAttackSpeedModifier();
            } else {
                animator.speed = twitchStatus.getMovementSpeedModifier();
            }
        }
    }


    // Trigger event for when unit is starting to fire an individual bullet
    private void onBeginBulletFire() {
        animator.SetInteger(primaryAttackAnimationTypeParameter, (int)inventoryModule.getPrimaryAttackAnimation());
    }
    
    
    // Trigger event handler for throwing a cask
    private void onCaskThrowTrigger() {
        animator.SetTrigger(caskThrowTriggerParameter);
    }


    // Trigger event handler for contamination
    private void onContaminateTrigger() {
        animator.SetTrigger(contaminateTriggerParameter);
    }

    // Trigger event handler for contamination
    private void onCraftStart() {
        animator.SetTrigger(craftStartTriggerParameter);
    }

    // Trigger event handler for contamination
    private void onCraftEnd() {
        audioModule.playFinishedCraftingSound();
        animator.SetTrigger(craftEndTriggerParameter);
    }

    // Trigger event handler for contamination
    private void onObtainSideEffect() {
        StartCoroutine(sideEffectFreezeSequence());
    }


    // Obtain Side Effect freeze sequence
    private IEnumerator sideEffectFreezeSequence() {
        transform.parent.forward = Vector3.back;
        animator.SetTrigger(sideEffectTriggerParameter);
        Time.timeScale = 0f;

        attackModule.inUninterruptableAnimationSequence = true;

        // Play audio
        audioModule.playObtainedSideEffectSound();
        audioModule.playSideEffectObtainedVoice();

        // Move camera
        PlayerCameraController.moveCamera(
            transform.parent,
            sideEffectCameraPitchAngle,
            0f,
            sideEffectCameraZoom,
            sideEffectCameraTransitionSpeed,
            Vector3.zero
        );

        yield return new WaitForSecondsRealtime(sideEffectFreezeTime);

        // Move camera back to default position
        PlayerCameraController.reset(sideEffectCameraTransitionSpeed);

        attackModule.inUninterruptableAnimationSequence = false;
        Time.timeScale = 1f;
    }


    // onHurt sequence
    //  Pre: The player got hurt by an enemy
    private void onTwitchHurt() {
        StartCoroutine(hurtSequence());
    }


    // The hurt sequence
    private IEnumerator hurtSequence() {
        animator.SetTrigger(hurtTriggerName);
        animator.SetInteger(randomIntParameter, Random.Range(0, numHurtAnimations));

        PlayerCameraController.hitStop(hitStopFrames);
        PlayerCameraController.shakeCamera(cameraShakeFrames, cameraShakeMagnitude);
        audioModule.playHurtVoice();

        attackModule.inUninterruptableAnimationSequence = true;

        yield return new WaitForSeconds(0.01f);

        attackModule.inUninterruptableAnimationSequence = false;
        animator.SetTrigger(unHurtTriggerName);
    }


    // Event handler for when player dies
    private void onTwitchDeath() {
        StartCoroutine(deathSequence());
    }


    private IEnumerator deathSequence() {
        audioModule.playDeathImpact();
        animator.SetTrigger(deathTriggerName);

        Time.timeScale = 0f;
        animator.speed = deathSlowMotionFactor;
        PlayerCameraController.instantMoveCamera(transform.parent, deathAngle, 0f, deathZoom, Vector3.zero);
        PlayerCameraController.shakeCamera(deathCameraShakeFrames, deathCameraShakeMagnitude);

        float waitedTime = (1f / 60f) * deathCameraShakeFrames;

        yield return new WaitForSecondsRealtime(waitedTime + deathDelayBetweenAnimAndShake);

        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        audioModule.playDeathVoice();

        yield return new WaitForSecondsRealtime(timeUntilDeathFadeOut);

        screenUiModule.fadeToBlack(deathFadeDuration, false);
    }
}
