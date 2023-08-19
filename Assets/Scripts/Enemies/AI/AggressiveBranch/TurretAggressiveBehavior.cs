using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretAggressiveBehavior : IEnemyAggroBranch
{
    [SerializeField]
    [Min(0.01f)]
    private float attackInterval = 0.75f;
    [SerializeField]
    [Min(1)]
    private int anticipationFrames = 10;
    [SerializeField]
    [Min(1)]
    private int attackFrames = 10;
    [SerializeField]
    [Min(0.5f)]
    private float turretRange = 100f;
    [SerializeField]
    [Min(0.01f)]
    private float turretProjectileDamage = 5f;
    [SerializeField]
    [Min(0f)]
    private float aimAngleVariance = 0f;
    [SerializeField]
    private Color anticipationColor = Color.yellow;
    [SerializeField]
    private LinearProjectile turretProjectile;


    private MeshRenderer meshRender;
    private Color originalColor;
    
    
    // Main function to do additional initialization for branch
    //  Pre: none
    //  Post: sets branch up
    protected override void initialize() {
        meshRender = GetComponent<MeshRenderer>();
        originalColor = meshRender.material.color;
    }
    
    
    // Main function to execute the branch
    //  Pre: tgt is the player, cannot equal null
    //  Post: executes aggressive branch
    public override IEnumerator execute(Transform tgt) {
        // Interval between attacks
        float timer = 0f;
        while (timer < attackInterval) {
            yield return 0;

            timer += Time.deltaTime;
            transform.forward = Vector3.ProjectOnPlane(tgt.position - transform.position, Vector3.up);
        }

        // Anticipation
        Vector3 projDir = Quaternion.AngleAxis(Random.Range(-aimAngleVariance, aimAngleVariance), Vector3.up) * Vector3.ProjectOnPlane(tgt.position - transform.position, Vector3.up).normalized;
        transform.forward = projDir;
        meshRender.material.color = anticipationColor;
        yield return AI_NavLibrary.waitForFrames(anticipationFrames);

        // Attack
        meshRender.material.color = originalColor;
        LinearProjectile curProjectile = Object.Instantiate(turretProjectile, transform.position, Quaternion.identity);
        curProjectile.setUp(projDir, turretProjectileDamage * enemyStats.getBaseAttack(), null, turretRange);
        yield return AI_NavLibrary.waitForFrames(attackFrames);
    }


    // Main function to reset the branch when the overall tree gets overriden / switch branches
    public override void reset() {}
}
