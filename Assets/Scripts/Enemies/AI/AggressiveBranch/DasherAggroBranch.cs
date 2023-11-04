using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DasherAggroBranch : IEnemyAggroBranch
{
    [Header("Navigation")]
    [SerializeField]
    [Range(0.01f, 0.5f)]
    private float pathRefreshTime = 0.15f;

    [Header("Pre-Dash Variables")]
    [SerializeField]
    [Min(0.1f)]
    private float minPreDashDistance = 3f;
    [SerializeField]
    [Min(0.1f)]
    private float nonDashDamage = 1f;

    [Header("Dash Variables")]
    [SerializeField]
    [Min(1)]
    private int dashAnticipationFrames = 30;
    [SerializeField]
    [Min(0.1f)]
    private float maxDashDistance = 6f;
    [SerializeField]
    [Min(0.01f)]
    private float dashDamage = 3f;
    [SerializeField]
    [Min(0.01f)]
    private float postDashRecoil = 0.75f;
    [SerializeField]
    [Min(1f)]
    private float dashMovementMultiplier = 2f;
    [SerializeField]
    private LayerMask dashCollisionMask;
    [SerializeField]
    private EnemyLingeringHitbox lingeringBodyHitbox;

    [Header("Dash showcase")]
    [SerializeField]
    private Color dashingColor = Color.red;
    private Color originalColor;
    private MeshRenderer enemyMesh;


    // Main function to do additional initialization for branch
    //  Pre: none
    //  Post: sets branch up
    protected override void initialize() {
        enemyMesh = GetComponent<MeshRenderer>();
        originalColor = enemyMesh.material.color;
        lingeringBodyHitbox.setDamage(nonDashDamage);
    }
    
    
    // Main function to execute the branch
    //  Pre: tgt is the player, cannot equal null
    //  Post: executes aggressive branch
    public override IEnumerator execute(Transform tgt) {
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
        enemyMesh.material.color = dashingColor;

        yield return AI_NavLibrary.waitForFrames(dashAnticipationFrames);

        // Actual dash
        lingeringBodyHitbox.setDamage(dashDamage * enemyStats.getBaseAttack());
        float dashDistanceTimer = 0f;
        float dashSpeed = enemyStats.getMovementSpeed() * dashMovementMultiplier * Time.deltaTime;
        while (dashDistanceTimer < maxDashDistance) {
            yield return 0;

            float distDelta = dashSpeed;
            dashDistanceTimer += dashSpeed;

            RaycastHit hitInfo;
            if (Physics.BoxCast(transform.position, transform.localScale, dashDir, out hitInfo, transform.rotation, distDelta, dashCollisionMask)) {
                distDelta = hitInfo.distance;
            }

            transform.Translate(distDelta * dashDir, Space.World);
        }

        // Recoil wait time
        lingeringBodyHitbox.setDamage(nonDashDamage);
        enemyMesh.material.color = originalColor;
        yield return new WaitForSeconds(postDashRecoil);
    }


    // Main function to reset the branch when the overall tree gets overriden / switch branches
    public override void reset() {
        enemyMesh.material.color = originalColor;
        lingeringBodyHitbox.setDamage(nonDashDamage);
    }
}
