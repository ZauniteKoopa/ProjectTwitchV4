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
    [SerializeField]
    private EnemyBossStatusUI bossUiModule;

    [Header("Spawn in animation")]
    [SerializeField]
    private EnemyStatus initialSpawnInDummy;
    [SerializeField]
    private WarwickBloodMark bloodMark;
    [SerializeField]
    [Min(0.1f)]
    private float spawnCameraTransitionToTargetSpeed;
    [SerializeField]
    private float spawnCameraTargetPitch = 50f;
    [SerializeField]
    [Min(0.1f)]
    private float spawnCameraZoom = 15f;
    [SerializeField]
    [Min(0.1f)]
    private float cameraStayOnTargetDuration;
    [SerializeField]
    [Min(0.1f)]
    private float cameraStayOnWarwickDuration;
    [SerializeField]
    [Min(0.1f)]
    private float spawnCameraTransitionToPlayerSpeed;
    [SerializeField]
    [Min(0.1f)]
    private float spawnEnemyKillHitStopTime = 1f;
    [SerializeField]
    [Min(0.1f)]
    private float cameraStayOnPlayerEnd;
    [SerializeField]
    private Vector3 cameraTargetOffset;
    [SerializeField]
    private AnimationClip warwickSpawnStartClip;
    [SerializeField]
    private AnimationClip warwickSpawnEndClip;
    [SerializeField]
    private float bloodFrenzyHealthRequirement = 0.6f;
    public UnityEvent spawnAnimationStarted;
    public UnityEvent spawnAnimationFinished;
    private bool spawned = false;
    private bool spawnInFinished = false;


    // Start is called before the first frame update (listens to the initialize event in UnitStatus to avoid race conditions)
    public void onInitialize()
    {
        // Connect to all events related to Behavior tree
        bossBehaviorTree.aggressiveBranchActiveEvent.AddListener(onAggressiveBranchActive);
        bossBehaviorTree.passiveBranchActiveEvent.AddListener(onPassiveBranchActive);

        // Connect to all events relating to enemy status
        warwickStatus.enemyPhaseTransitionBeginEvent.AddListener(onBossTransitionBegin);
        warwickStatus.enemyPhaseTransitionEndEvent.AddListener(onBossTransitionEnd);
        warwickStatus.stunnedStartEvent.AddListener(onStunStart);
        warwickStatus.stunnedEndEvent.AddListener(onStunEnd);

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

        initialSpawnInDummy.enemyNoticesDamageEvent.AddListener(delegate { onSpawnDummyDamaged(initialSpawnInDummy); });
    }

    // Update is called once per frame
    void Update()
    {
        // Set runtime animation variables
        if (spawnInFinished) {
            warwickAnimator.SetFloat("MoveSpeed", navMeshAgent.velocity.magnitude);
            warwickAnimator.SetInteger("AttackAnimationState", (int)aggroBranch.getAttackAnimState());
        }
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


    private void onStunStart() {
        warwickAnimator.SetBool("UnitStun", true);
    }


    private void onStunEnd() {
        warwickAnimator.SetBool("UnitStun", false);
    }


    // Main public IEnumerator to do spawn animation
    private IEnumerator startSpawnAnimation() {
        // Setup (wait for timestop to stop);
        warwickAnimator.updateMode = AnimatorUpdateMode.Normal;
        spawnAnimationStarted.Invoke();
        yield return new WaitForSeconds(0.1f);
        Time.timeScale = 0f;

        // Set blood hunt on target
        bloodMark.setActive(true);
        bloodMark.setTarget(initialSpawnInDummy.transform);
        bloodMark.setTrackingProgress(1f, 1f);

        // Pan to target
        float timeToPanToTarget = PlayerCameraController.moveCamera(
            initialSpawnInDummy.transform.parent,
            spawnCameraTargetPitch,
            0f,
            spawnCameraZoom,
            spawnCameraTransitionToTargetSpeed,
            cameraTargetOffset
        );

        yield return PauseConstraints.waitForSecondsRealtimeWithPause(timeToPanToTarget);

        // Stay on target
        yield return PauseConstraints.waitForSecondsRealtimeWithPause(cameraStayOnTargetDuration);

        // Do warwick animations spawnIn
        warwickAnimator.SetTrigger("StartSpawnIn");
        transform.localPosition = new Vector3(0f, transform.localPosition.y, 0f);
        warwickAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        PlayerCameraController.shakeCamera((int)Mathf.Floor(warwickSpawnEndClip.length * 60f), 0.2f);
        yield return PauseConstraints.waitForSecondsRealtimeWithPause(warwickSpawnStartClip.length);

        // Kill (TimeStop and CameraShake)
        bloodMark.setActive(false);
        initialSpawnInDummy.damage(9999999f, true);
        warwickAnimator.updateMode = AnimatorUpdateMode.Normal;
        PlayerCameraController.shakeCamera(40, 1f);
        yield return PauseConstraints.waitForSecondsRealtimeWithPause(spawnEnemyKillHitStopTime);
        warwickAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;

        // Spawn end clip
        yield return PauseConstraints.waitForSecondsRealtimeWithPause(warwickSpawnEndClip.length);

        // Stay on warwick
        yield return PauseConstraints.waitForSecondsRealtimeWithPause(cameraStayOnWarwickDuration);

        // Pan to player
        float timeToReset = PlayerCameraController.reset(spawnCameraTransitionToPlayerSpeed);
        yield return PauseConstraints.waitForSecondsRealtimeWithPause(timeToReset);

        // Stay on player
        yield return PauseConstraints.waitForSecondsRealtimeWithPause(cameraStayOnPlayerEnd);

        // Cleanup
        spawnInFinished = true;
        bossUiModule.gameObject.SetActive(true);
        warwickAnimator.updateMode = AnimatorUpdateMode.Normal;
        Time.timeScale = 1f;
        spawnAnimationFinished.Invoke();
    }


    // Main event handler for when a unit nearby gets damaged
    public void onSpawnDummyDamaged(IUnitStatus damagedUnit) {
        if (damagedUnit.getHealthPercentage() <= bloodFrenzyHealthRequirement && !spawned) {
            spawned = true;
            warwickStatus.gameObject.SetActive(true);

            StartCoroutine(startSpawnAnimation());
        }
    }
}
