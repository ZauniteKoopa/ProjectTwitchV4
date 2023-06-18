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
    [Min(0.1f)]
    private float sideEffectFreezeTime = 1.5f;


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
    }


    // On update, update variables concerning the movement state
    private void Update() {
        animator.SetBool(movementBoolParameter, movementModule.isCurrentlyMoving());
        animator.SetBool(shootingBoolParameter, attackModule.isShooting());
        animator.SetBool(ambushBoolParameter, attackModule.isDashing());

        if (animator.GetBool(shootingBoolParameter)) {
            animator.speed = twitchStatus.getAttackSpeedModifier();
        } else {
            animator.speed = twitchStatus.getMovementSpeedModifier();
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
        animator.SetTrigger(craftEndTriggerParameter);
    }

    // Trigger event handler for contamination
    private void onObtainSideEffect() {
        StartCoroutine(sideEffectFreezeSequence());
    }


    // Obtain Side Effect freeze sequence
    private IEnumerator sideEffectFreezeSequence() {
        animator.SetTrigger(sideEffectTriggerParameter);
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(sideEffectFreezeTime);

        Time.timeScale = 1f;
    }
}
