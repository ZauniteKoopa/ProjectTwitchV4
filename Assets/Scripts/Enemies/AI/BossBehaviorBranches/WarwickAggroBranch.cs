using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [Min(1)]
    private int dashAnticipationFrames = 40;
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
    [Min(1)]
    private int dashRecoilFrames = 15;
    [SerializeField]
    [Min(1f)]
    private float dashMovementMultiplier = 2.25f;
    [SerializeField]
    private LayerMask dashCollisionMask;
    [SerializeField]
    private EnemyLingeringHitbox lingeringBodyHitbox;


    [Header("Slash Variables")]
    [SerializeField]
    [Min(0.1f)]
    private float minSlashDistance = 3.5f;
    [SerializeField]
    [Min(1)]
    private int slashAnticipationFrames = 45;
    [SerializeField]
    [Min(0.1f)]
    private int slashAttackFrames = 20;
    [SerializeField]
    [Min(0.01f)]
    private float slashDamage = 10f;
    [SerializeField]
    [Min(1)]
    private int slashRecoilFrames = 30;
    [SerializeField]
    private EnemyHitbox slashHitbox;
    private MeshRenderer slashMesh;
    [SerializeField]
    private Color anticipationSlashColor = Color.yellow;
    [SerializeField]
    private Color hitboxSlashColor = Color.red;


    // Main function to do additional initialization for branch
    //  Pre: none
    //  Post: sets branch up
    protected override void initialize() {
        slashMesh = slashHitbox.GetComponent<MeshRenderer>();
        lingeringBodyHitbox.setDamage(nonDashDamage);
    }


    // Main function to execute the branch
    //  Pre: tgt is the player, cannot equal null
    //  Post: executes aggressive branch
    public override IEnumerator execute(Transform tgt, BossEnemyStatus bossEnemyStatus) {
        int diceRoll = Random.Range(0, 2);

        if (diceRoll == 0) {
            yield return lunge(tgt, bossEnemyStatus);
        } else {
            yield return slash(tgt);
        }
    }


    // Main function to reset the branch when the overall tree gets overriden / switch branches
    public override void reset() {
        slashMesh.enabled = false;
        lingeringBodyHitbox.setDamage(nonDashDamage);
    }



    // Main attacking function to do a lunge
    private IEnumerator lunge(Transform tgt, BossEnemyStatus bossEnemyStatus) {
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

        yield return AI_NavLibrary.waitForFrames(dashAnticipationFrames);

        // Actual dash
        lingeringBodyHitbox.setDamage(dashDamage * bossEnemyStatus.getBaseAttack());
        float dashTimer = 0f;
        while (dashTimer < dashTime) {
            yield return 0;
            dashTimer += Time.deltaTime;

            float distDelta = Time.deltaTime * Mathf.Max(minDashSpeed, bossEnemyStatus.getMovementSpeed() * dashMovementMultiplier);
            RaycastHit hitInfo;
            if (Physics.BoxCast(transform.position, transform.localScale, dashDir, out hitInfo, transform.rotation, distDelta, dashCollisionMask)) {
                distDelta = hitInfo.distance;
            }

            transform.Translate(distDelta * dashDir, Space.World);
        }

        // Recoil wait time
        lingeringBodyHitbox.setDamage(nonDashDamage);
        yield return AI_NavLibrary.waitForFrames(dashRecoilFrames);
    }


    // Main function to do a slash attack
    private IEnumerator slash(Transform tgt) {
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
        yield return AI_NavLibrary.waitForFrames(slashAnticipationFrames);

        // actual Slash
        slashHitbox.doDamage(slashDamage * enemyStats.getBaseAttack());
        slashMesh.material.color = hitboxSlashColor;
        yield return AI_NavLibrary.waitForFrames(slashAttackFrames);
        slashMesh.enabled = false;

        // Post Slash stun
        yield return AI_NavLibrary.waitForFrames(slashRecoilFrames);
    }
}
