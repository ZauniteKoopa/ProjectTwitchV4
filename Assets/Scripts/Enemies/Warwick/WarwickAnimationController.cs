using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class WarwickAnimationController : MonoBehaviour
{
    [SerializeField]
    private WarwickPassiveBranch passiveBranch;
    [SerializeField]
    private WarwickAggroBranch aggroBranch;
    [SerializeField]
    private EnemyBossBehaviorTree bossBehaviorTree;
    [SerializeField]
    private NavMeshAgent navMeshAgent;
    [SerializeField]
    private BossEnemyStatus warwickStatus;
    [SerializeField]
    private Animator warwickAnimator;

    // Start is called before the first frame update
    void Awake()
    {
        // Connect to all events related to Behavior tree
        bossBehaviorTree.aggressiveBranchActiveEvent.AddListener(onAggressiveBranchActive);
        bossBehaviorTree.passiveBranchActiveEvent.AddListener(onPassiveBranchActive);

        // Connect to all events relating to enemy status
        warwickStatus.enemyPhaseTransitionBeginEvent.AddListener(onBossTransitionBegin);
        warwickStatus.enemyPhaseTransitionEndEvent.AddListener(onBossTransitionEnd);

        // Connect to all events relating to Passive Branch
        passiveBranch.bloodHuntStartEvent.AddListener(onBloodHuntReactionStart);
        passiveBranch.bloodHuntEndEvent.AddListener(onBloodHuntReactionEnd);
        passiveBranch.bloodHuntTargetKilled.AddListener(onBloodHuntKilledEnemy);

        // Connect to all events relating to aggro branch
        aggroBranch.lungeStart.AddListener(onLungeAttackStart);
        aggroBranch.slashStart.AddListener(onSlashAttackStart);
        aggroBranch.howlStart.AddListener(onHowlAttackStart);
        aggroBranch.howlStunBeginEvent.AddListener(onHowlStunBegin);
        aggroBranch.howlStunEndEvent.AddListener(onHowlStunEnd);
    }

    // Update is called once per frame
    void Update()
    {
        // Set runtime animation variables
        warwickAnimator.SetFloat("MoveSpeed", navMeshAgent.velocity.magnitude);
        warwickAnimator.SetInteger("AttackAnimationState", (int)aggroBranch.getAttackAnimState());
    }


    // -------------------------
    //  General Event handler functions for transitioning as a boss
    // -------------------------
    private void onBossTransitionBegin() {
        warwickAnimator.SetBool("Transitioning", true);
    }


    private void onBossTransitionEnd() {
        warwickAnimator.SetBool("Transitioning", false);
    }


    // -------------------------
    //  General Event handler functions for howl stunning
    // -------------------------
    private void onHowlStunBegin() {
        warwickAnimator.SetBool("InHowlStun", true);
    }


    private void onHowlStunEnd() {
        warwickAnimator.SetBool("InHowlStun", false);
    }


    // -------------------------
    //  General Event handler functions for behavior branches
    // -------------------------
    private void onPassiveBranchActive() {
        warwickAnimator.SetBool("BloodHuntFinished", false);
        warwickAnimator.SetBool("InBloodHuntReaction", false);
        warwickAnimator.SetBool("InAggroState", false);
    }


    private void onAggressiveBranchActive() {
        warwickAnimator.SetBool("InAggroState", true);
    }



    // -------------------------
    //  General Event handler functions for blood hunt
    // -------------------------
    private void onBloodHuntReactionStart() {
        warwickAnimator.SetBool("BloodHuntFinished", true);
        warwickAnimator.SetBool("IsBloodHuntTargetPlayer", passiveBranch.isBloodiedTargetPlayer());
        warwickAnimator.SetBool("InBloodHuntReaction", true);        
    }


    private void onBloodHuntReactionEnd() {
        warwickAnimator.SetBool("InBloodHuntReaction", false);
    }


    private void onBloodHuntKilledEnemy() {
        warwickAnimator.SetBool("BloodHuntFinished", false);
    }


    // -------------------------
    //  General Event handler functions for attacks
    // -------------------------
    private void onHowlAttackStart() {
        warwickAnimator.SetTrigger("HowlTrigger");
    }


    private void onSlashAttackStart() {
        warwickAnimator.SetTrigger("SlashTrigger");
    }


    private void onLungeAttackStart() {
        warwickAnimator.SetTrigger("LungeTrigger");
    }
}
