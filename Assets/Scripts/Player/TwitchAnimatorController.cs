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

    [Header("Animator Parameter Names")]
    [SerializeField]
    private string movementBoolParameter;
    [SerializeField]
    private string shootingBoolParameter;
    [SerializeField]
    private string caskThrowTriggerParameter;
    [SerializeField]
    private string contaminateTriggerParameter;


    // On awake, set everything
    private void Awake() {
        animator = GetComponent<Animator>();

        if (animator == null) {
            Debug.LogError("No animator to control!!");
        }

        if (movementModule == null || attackModule == null) {
            Debug.LogError("Missing Modules Found - Make sure that all modules are filled for animator to work properly");
        }

        // Listen to events from attack module
        attackModule.abilityOneTrigger.AddListener(onCaskThrowTrigger);
        attackModule.abilityTwoTrigger.AddListener(onContaminateTrigger);
    }


    // On update, update variables concerning the movement state
    private void Update() {
        animator.SetBool(movementBoolParameter, movementModule.isCurrentlyMoving());
        animator.SetBool(shootingBoolParameter, attackModule.isShooting());
    }


    // Trigger event handler for throwing a cask
    private void onCaskThrowTrigger() {
        animator.SetTrigger(caskThrowTriggerParameter);
    }


    // Trigger event handler for contamination
    private void onContaminateTrigger() {
        animator.SetTrigger(contaminateTriggerParameter);
    }
}
