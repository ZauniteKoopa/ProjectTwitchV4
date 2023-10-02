using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WarwickMoveEnum {
    DASH,
    SLASH,
    HOWL
}


[System.Serializable]
public class WarwickAggroPhaseModifiers {
    [Header("Dash Attacks")]
    [Min(1)]
    public int dashAnticipationFrames = 40;
    [Min(1f)]
    public float dashMovementMultiplier = 2.25f;
    [Min(1)]
    public int dashRecoilFrames = 20;

    [Header("Slash Variables")]
    [Min(1)]
    public int slashAnticipationFrames = 45;
    [Min(1)]
    public int slashRecoilFrames = 30;

    [Header("Howl Variables")]
    [Min(0.01f)]
    public float howlSlowDuration = 1.5f;
    [SerializeField]
    [Min(0.01f)]
    public float howlShieldHealth = 20f;

    [Header("Move Probabilities")]
    [SerializeField]
    [Min(0.01f)]
    private float dashProbability;
    [SerializeField]
    [Min(0.01f)]
    private float slashProbability;
    [SerializeField]
    [Min(0.01f)]
    private float howlProbability;


    // Main function to decide which move you can get
    public WarwickMoveEnum chooseMove(bool isChargingHowl) {
        float[] moveProbs = {dashProbability, slashProbability, howlProbability};
        int numConsideredProbs = (isChargingHowl) ? moveProbs.Length - 1 : moveProbs.Length;
        float totalProb = 0f;

        for (int i = 0; i < numConsideredProbs; i++) {
            totalProb += moveProbs[i];
        }

        // Roll the dice
        float diceRoll = Random.Range(0f, totalProb);
        float moveThreshold = moveProbs[0];
        int curMove = 0;
        
        while (diceRoll > moveThreshold) {
            curMove++;
            moveThreshold += moveProbs[curMove];
        }

        // Choose move
        switch (curMove) {
            case 0:
                return WarwickMoveEnum.DASH;

            case 1:
                return WarwickMoveEnum.SLASH;

            default:
                return WarwickMoveEnum.HOWL;
        }

    }
}




public class WarwickAggroBranch : IBossBehaviorBranch
{
    [Header("Navigation")]
    [SerializeField]
    [Range(0.01f, 0.5f)]
    private float pathRefreshTime = 0.15f;

    [Header("Pre-Dash Variables")]
    [SerializeField]
    [Min(0.1f)]
    private float minPreDashDistance = 5f;
    [SerializeField]
    [Min(0.1f)]
    private float nonDashDamage = 1f;

    [Header("Dash Variables")]
    [SerializeField]
    [Min(0.01f)]
    private float dashTime = 0.35f;
    [SerializeField]
    [Min(0.1f)]
    private float minDashSpeed = 16f;
    [SerializeField]
    [Min(0.01f)]
    private float dashDamage = 4f;
    [SerializeField]
    private LayerMask dashCollisionMask;
    [SerializeField]
    private EnemyLingeringHitbox lingeringBodyHitbox;


    [Header("Slash Variables")]
    [SerializeField]
    [Min(0.1f)]
    private float minSlashDistance = 3.5f;
    [SerializeField]
    [Min(0.1f)]
    private int slashAttackFrames = 20;
    [SerializeField]
    [Min(0.01f)]
    private float slashDamage = 10f;
    [SerializeField]
    private EnemyHitbox slashHitbox;
    private MeshRenderer slashMesh;
    [SerializeField]
    private Color anticipationSlashColor = Color.yellow;
    [SerializeField]
    private Color hitboxSlashColor = Color.red;

    [Header("Howl Variables")]
    [SerializeField]
    [Min(0.01f)]
    private float howlChargeDuration = 12f;
    [SerializeField]
    [Range(1f, 2f)]
    private float howlShieldArmorIncrease = 1.5f;
    [SerializeField]
    [Range(0.01f, 1f)]
    private float howlSlowFactor = 0.5f;
    [SerializeField]
    [Min(0.01f)]
    private float minTimeBeforeNextHowl = 10f;
    [SerializeField]
    private EnemyTimedSpeedEffectHitbox howlHitbox;
    [SerializeField]
    [Min(1)]
    private int howlRecoilStunFrames = 60;
    [SerializeField]
    [Min(1)]
    private int initialHowlChargeupFrames = 20;
    [SerializeField]
    private ResourceBar howlBar;
    private MeshRenderer howlMesh;
    private Coroutine runningHowlCoroutine;
    private float curHowlShieldHealth = 0f;

    [Header("Warwick Phase Escalation")]
    [SerializeField]
    private WarwickAggroPhaseModifiers[] warwickPhases;


    // Main function to do additional initialization for branch
    //  Pre: none
    //  Post: sets branch up
    protected override void initialize(BossEnemyStatus bossEnemyStatus) {
        slashMesh = slashHitbox.GetComponent<MeshRenderer>();
        howlMesh = howlHitbox.GetComponent<MeshRenderer>();
        lingeringBodyHitbox.setDamage(nonDashDamage);

        bossEnemyStatus.damageEvent.AddListener(onHowlShieldDamage);
        bossEnemyStatus.enemyPhaseTransitionBeginEvent.AddListener(onBossTransitionBegin);
        howlBar.setActive(false);

        if (warwickPhases.Length != bossEnemyStatus.getNumPhases()) {
            Debug.LogError("ERROR: NUM PHASES DOES NOT EQUAL BOSS ESCALATION ARRAY LENGTH");
        }
    }


    // Main function to check whether or not the enemy is currently too focused on something to be distracted from specific task
    public override bool canBeDistractedByPlayer() {
        return false;
    }


    // Main function to check whether or not the enemy is currently too focused on something to be distracted from specific task
    public override bool canBeDistractedByEnemies() {
        return false;
    }


    // Main function to execute the branch
    //  Pre: tgt is the player, cannot equal null
    //  Post: executes aggressive branch
    public override IEnumerator execute(Transform tgt, BossEnemyStatus bossEnemyStatus) {
        WarwickAggroPhaseModifiers currentPhaseValues = warwickPhases[bossEnemyStatus.getCurrentPhase()];
        WarwickMoveEnum currentMove = currentPhaseValues.chooseMove(runningHowlCoroutine != null);

        if (currentMove == WarwickMoveEnum.DASH) {
            yield return lunge(tgt, bossEnemyStatus, currentPhaseValues);
        } else if (currentMove == WarwickMoveEnum.SLASH) {
            yield return slash(tgt, currentPhaseValues);
        } else {
            runningHowlCoroutine = StartCoroutine(howlingSequence(bossEnemyStatus, currentPhaseValues));
            yield return AI_NavLibrary.waitForFrames(initialHowlChargeupFrames);
        }
    }


    // Main function to reset the branch when the overall tree gets overriden / switch branches
    public override void reset() {
        slashMesh.enabled = false;
        lingeringBodyHitbox.setDamage(nonDashDamage);
    }



    // Main attacking function to do a lunge
    private IEnumerator lunge(Transform tgt, BossEnemyStatus bossEnemyStatus, WarwickAggroPhaseModifiers curPhase) {
        // Keep moving until you're close enough to the target
        lingeringBodyHitbox.setDamage(nonDashDamage);
        while (Vector3.ProjectOnPlane(tgt.position - transform.position, Vector3.up).magnitude >= minPreDashDistance) {
            yield return AI_NavLibrary.goToPosition(
                tgt.position,
                navMeshAgent,
                enemyStats,
                pathExpiration: pathRefreshTime,
                interrupted: () => Vector3.ProjectOnPlane(tgt.position - transform.position, Vector3.up).magnitude <= minPreDashDistance
            );
        }

        // Dash anticipation
        navMeshAgent.velocity = Vector3.zero;
        navMeshAgent.isStopped = true;

        Vector3 dashDir = Vector3.ProjectOnPlane(tgt.position - transform.position, Vector3.up).normalized;
        transform.forward = dashDir;

        yield return AI_NavLibrary.waitForFrames(curPhase.dashAnticipationFrames);

        // Actual dash
        lingeringBodyHitbox.setDamage(dashDamage * bossEnemyStatus.getBaseAttack());
        float dashTimer = 0f;
        float startingDashSpeed = bossEnemyStatus.getMovementSpeed();
        
        while (dashTimer < dashTime) {
            yield return 0;
            dashTimer += Time.deltaTime;

            float distDelta = Time.deltaTime * Mathf.Max(minDashSpeed, startingDashSpeed * curPhase.dashMovementMultiplier);
            RaycastHit hitInfo;
            if (Physics.BoxCast(transform.position, transform.localScale, dashDir, out hitInfo, transform.rotation, distDelta, dashCollisionMask)) {
                distDelta = hitInfo.distance;
            }

            transform.Translate(distDelta * dashDir, Space.World);
        }

        // Recoil wait time
        lingeringBodyHitbox.setDamage(nonDashDamage);
        yield return AI_NavLibrary.waitForFrames(curPhase.dashRecoilFrames);
    }


    // Main function to do a slash attack
    private IEnumerator slash(Transform tgt, WarwickAggroPhaseModifiers curPhase) {
        // Keep moving until you're close enough to the target
        while (Vector3.ProjectOnPlane(tgt.position - transform.position, Vector3.up).magnitude >= minSlashDistance) {
            yield return AI_NavLibrary.goToPosition(
                tgt.position,
                navMeshAgent,
                enemyStats,
                pathExpiration: pathRefreshTime,
                interrupted: () => Vector3.ProjectOnPlane(tgt.position - transform.position, Vector3.up).magnitude <= minSlashDistance
            );
        }

        // Stop - Slash anticipation
        navMeshAgent.velocity = Vector3.zero;
        navMeshAgent.isStopped = true;
        slashMesh.enabled = true;
        slashMesh.material.color = anticipationSlashColor;
        yield return AI_NavLibrary.waitForFrames(curPhase.slashAnticipationFrames);

        // actual Slash
        slashHitbox.doDamage(slashDamage * enemyStats.getBaseAttack());
        slashMesh.material.color = hitboxSlashColor;
        yield return AI_NavLibrary.waitForFrames(slashAttackFrames);
        slashMesh.enabled = false;

        // Post Slash stun
        yield return AI_NavLibrary.waitForFrames(curPhase.slashRecoilFrames);
    }


    // Main howling sequence
    private IEnumerator howlingSequence(BossEnemyStatus bossEnemyStatus, WarwickAggroPhaseModifiers curPhase) {
        // Set up armor
        curHowlShieldHealth = curPhase.howlShieldHealth;
        bossEnemyStatus.applyDefenseModifier(howlShieldArmorIncrease);
        howlMesh.enabled = true;
        howlMesh.material.color = anticipationSlashColor;
        howlBar.setActive(true);
        howlHitbox.setUp(howlSlowFactor, curPhase.howlSlowDuration);

        // Howling charge up loop
        float howlTimer = 0f;
        while (howlTimer < howlChargeDuration && curHowlShieldHealth > 0f) {
            yield return 0;
            howlTimer += Time.deltaTime;
            howlBar.setFill(howlTimer, howlChargeDuration);
        }

        // If howl shield is still up, apply speed effect to player
        bossEnemyStatus.revertDefenseModifier(howlShieldArmorIncrease);
        howlBar.setActive(false);
        if (curHowlShieldHealth > 0f) {
            howlHitbox.doDamage(0.1f);
            howlMesh.material.color = hitboxSlashColor;
            yield return new WaitForSeconds(0.2f);
            howlMesh.enabled = false;

        // Else, be stunned for a period of time
        } else {
            howlMesh.enabled = false;
            bossEnemyStatus.stun(true);
            yield return AI_NavLibrary.waitForFrames(howlRecoilStunFrames);
            bossEnemyStatus.stun(false);
        }

        // Wait a certain amount of time before doing another howl
        yield return new WaitForSeconds(minTimeBeforeNextHowl);
        runningHowlCoroutine = null;
    }



    // Main function to handle howl shield damage
    private void onHowlShieldDamage(float damage) {
        if (runningHowlCoroutine != null && curHowlShieldHealth > 0f) {
            curHowlShieldHealth -= damage;
        }
    }


    // Main function for when unit transitions
    private void onBossTransitionBegin() {
        if (runningHowlCoroutine != null && curHowlShieldHealth > 0f) {
            curHowlShieldHealth = 0f;
        }
    }
}
