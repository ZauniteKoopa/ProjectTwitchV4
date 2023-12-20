using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodingBotAggroBranch : IEnemyAggroBranch
{
    [Header("Navigation")]
    [SerializeField]
    [Range(0.01f, 0.5f)]
    private float pathRefreshTime = 0.3f;
    
    [Header("Explosion Behavior")]
    [SerializeField]
    [Min(0.1f)]
    private float minExplosionDistance = 3f;
    [SerializeField]
    [Min(0.1f)]
    private float explosionAnticipationTime = 1f;
    [SerializeField]
    [Min(0.1f)]
    private float explosionAttackTime = 0.5f;
    [SerializeField]
    [Min(0.01f)]
    private float explosionDamage = 10f;
    [SerializeField]
    [Min(0.01f)]
    private float postExplosionStunTime = 0.75f;

    [Header("Explosion Showcase")]
    [SerializeField]
    private EnemyHitbox explosionHitbox;
    private MeshRenderer explosionMesh;
    [SerializeField]
    private Color anticipationExplosionColor = Color.yellow;
    [SerializeField]
    private Color hitboxExplosionColor = Color.red;

    
    
    
    // Main function to do additional initialization for branch
    //  Pre: none
    //  Post: sets branch up
    protected override void initialize() {
        explosionMesh = explosionHitbox.GetComponent<MeshRenderer>();
    }
    
    
    // Main function to execute the branch
    //  Pre: tgt is the player, cannot equal null
    //  Post: executes aggressive branch
    public override IEnumerator execute(Transform tgt) {

        // Keep moving until you're close enough to the target
        while (Vector3.ProjectOnPlane(tgt.position - transform.position, Vector3.up).magnitude >= minExplosionDistance) {
            yield return AI_NavLibrary.goToPosition(
                tgt.position,
                navMeshAgent,
                enemyStats,
                pathExpiration: pathRefreshTime,
                interrupted: () => Vector3.ProjectOnPlane(tgt.position - transform.position, Vector3.up).magnitude <= minExplosionDistance
            );
        }

        // Stop - explosion anticipation
        navMeshAgent.velocity = Vector3.zero;
        navMeshAgent.isStopped = true;
        explosionMesh.enabled = true;
        explosionMesh.material.color = anticipationExplosionColor;
        yield return new WaitForSeconds(explosionAnticipationTime);

        // actual explosion
        explosionHitbox.doDamage(explosionDamage * enemyStats.getBaseAttack());
        explosionMesh.material.color = hitboxExplosionColor;
        audioManager.playAttackSoundEffect();
        yield return new WaitForSeconds(explosionAttackTime);
        explosionMesh.enabled = false;

        // Post explosion stun
        yield return new WaitForSeconds(postExplosionStunTime);
    }


    // Main function to reset the branch when the overall tree gets overriden / switch branches
    public override void reset() {
        explosionMesh.enabled = false;
    }
}
