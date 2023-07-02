using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulsatingCask : DeployableHitbox
{
    // Basic variables
    [Header("Initial Damage Period")]
    [SerializeField]
    [Min(0.01f)]
    private float initialDamageDuration = 0.05f;
    [SerializeField]
    [Min(0.1f)]
    private float initialDamage = 3f;
    [SerializeField]
    [Min(0)]
    private int initialDamageStacks = 1;

    [Header("Grabbing Sequence")]
    [SerializeField]
    [Min(0.01f)]
    private float grabbingDuration = 1f;
    [SerializeField]
    [Range(0.01f, 1f)]
    private float grabbingSlowModifier = 0.4f;
    [SerializeField]
    [Min(0.01f)]
    private float pullingDuration = 0.2f;
    [SerializeField]
    [Min(0.01f)]
    private float stunDuration = 0.5f;
    [SerializeField]
    [Min(0.01f)]
    private float pullingDamage = 5f;
    [SerializeField]
    [Min(0)]
    private int addedPullingStacks = 1;
    [SerializeField]
    [Min(0.01f)]
    private float pullDistance = 3f;
    [SerializeField]
    private LayerMask pullingCollisionMask;

    [SerializeField]
    private AudioSource speaker;
    [SerializeField]
    private AudioClip splatSound;
    
    // Hashsets for enemy management
    private PoisonVial poison;
    private bool inInitialStage = true;
    private HashSet<EnemyStatus> enemyHit = new HashSet<EnemyStatus>();
    private HashSet<EnemyStatus> grabbed = new HashSet<EnemyStatus>();
    
    
    // Main abstract protected function to override that represents what this hitbox does in its lifespan
    //  Pre: none
    //  Post: hitbox will stay for a duration, doing whatever it wants. by the end of it, it should kill itself
    protected override IEnumerator lifespan(PoisonVial p) {
        poison = p;
        GetComponent<MeshRenderer>().material.color = p.getColor();

        // Initial damage
        yield return new WaitForSeconds(initialDamageDuration);
        inInitialStage = false;

        // Get all grabbing targets
        yield return new WaitForSeconds(grabbingDuration);

        // Disable collider
        GetComponent<Collider>().enabled = false;
        GetComponent<Renderer>().enabled = false;

        // Play splat sound
        speaker.clip = splatSound;
        speaker.Play();

        // Set up pull
        Dictionary<EnemyStatus, Vector3> pullDirections = new Dictionary<EnemyStatus, Vector3>();
        float pullSpeed = pullDistance / pullingDuration;

        foreach (EnemyStatus tgt in grabbed) {
            tgt.revertSpeedModifier(grabbingSlowModifier);
            tgt.stun(true);
            pullDirections.Add(tgt, Vector3.ProjectOnPlane(transform.position - tgt.transform.position, Vector3.up).normalized);
        }

        // Actually pull
        float timer = 0f;
        while (timer < pullingDuration) {
            yield return 0;
            timer += Time.deltaTime;

            foreach (EnemyStatus tgt in grabbed) {
                float curDistPulled = pullSpeed * Time.deltaTime;
                RaycastHit hitInfo;
                if (Physics.Raycast(tgt.transform.position, pullDirections[tgt], out hitInfo, curDistPulled, pullingCollisionMask)) {
                    curDistPulled = hitInfo.distance - (0.5f * tgt.transform.lossyScale.x);
                }

                tgt.transform.Translate(pullSpeed * Time.deltaTime * pullDirections[tgt], Space.World);
            }
        }

        // After effects of pull
        foreach (EnemyStatus tgt in grabbed) {
            tgt.poisonDamage(pullingDamage, false, poison, addedPullingStacks);
        }

        yield return new WaitForSeconds(stunDuration);
        foreach (EnemyStatus tgt in grabbed) {
            tgt.stun(false);
        }
        Object.Destroy(gameObject);
    }


    // On trigger enter
    private void OnTriggerEnter(Collider collider) {
        EnemyStatus enemyTgt = collider.GetComponent<EnemyStatus>();

        if (enemyTgt != null) {
            // If initial stage, apply damage
            if (inInitialStage && !enemyHit.Contains(enemyTgt)) {
                enemyHit.Add(enemyTgt);
                enemyTgt.poisonDamage(initialDamage, false, poison, initialDamageStacks);
            }

            // add to enemies that are grabbed. They cannot escape being grabbed after this
            grabbed.Add(enemyTgt);
            enemyTgt.applySpeedModifier(grabbingSlowModifier);
        }
    }
}
