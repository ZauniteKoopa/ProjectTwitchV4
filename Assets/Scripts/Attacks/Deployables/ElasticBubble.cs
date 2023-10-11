using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElasticBubble : DeployableHitbox
{
    // Elastic bubble variables
    [SerializeField]
    [Min(0.01f)]
    private float bubbleDuration = 6f;
    [SerializeField]
    [Min(0.01f)]
    private float minBubbleSize = 3f;
    [SerializeField]
    [Range(0.01f, 1f)]
    private float shrinkStartTime = 0.5f;
    [SerializeField]
    [Min(0.01f)]
    private float stunTime = 1.75f;
    [SerializeField]
    [Min(0)]
    private int initialEnemyStacks = 2;
    [SerializeField]
    [Min(0.01f)]
    private float initialDamageDuration = 0.05f;
    [SerializeField]
    [Min(0.1f)]
    private float initialDamage = 3f;
    [SerializeField]
    private EnemyStatusSensor enemyStatusSensor;

    // Main runtime variables
    private Collider enemySensorCollider;
    private PoisonVial curPoison;
    private HashSet<EnemyStatus> inside = new HashSet<EnemyStatus>();
    private bool dealsInitialDamage = true;


    // On awake, connect to enemy status sensor
    private void Awake() {
        Debug.Assert(initialDamageDuration < bubbleDuration);

        enemyStatusSensor.enemyEnterEvent.AddListener(onEnemyEnter);
        enemyStatusSensor.enemyExitEvent.AddListener(onEnemyExit);
        enemySensorCollider = enemyStatusSensor.GetComponent<Collider>();
    }


    // Lifespan of deployable
    protected override IEnumerator lifespan(PoisonVial p) {
        curPoison = p;
        float maxBubbleScale = transform.localScale.x;
        float fullBubbleDuration = (bubbleDuration - initialDamageDuration) * (shrinkStartTime);
        float shrinkingBubbleDuration = (bubbleDuration - initialDamageDuration) * (1 - shrinkStartTime);

        // Wait initial damage period
        yield return new WaitForSeconds(initialDamageDuration);
        dealsInitialDamage = false;

        // wait full bubble duration
        yield return new WaitForSeconds(fullBubbleDuration);

        // shrink bubble
        float shrinkTimer = 0f;
        while (shrinkTimer < shrinkingBubbleDuration) {
            yield return 0;

            shrinkTimer += Time.deltaTime;
            float curScale = Mathf.Lerp(maxBubbleScale, minBubbleSize, shrinkTimer / shrinkingBubbleDuration);
            transform.localScale = new Vector3(curScale, maxBubbleScale, curScale);
        }

        // After shrinking phase, disable collider and all of the walls
        enemySensorCollider.enabled = false;
        for (int c = 0; c < transform.childCount; c++) {
            transform.GetChild(c).gameObject.SetActive(false);
        }

        // Wait out all stunning coroutines before stopping
        yield return new WaitForSeconds(stunTime * 1.5f);
        Object.Destroy(gameObject);
    }


    // Main function for when enemy enters zone
    private void onEnemyEnter(EnemyStatus enemy) {
        if (!inside.Contains(enemy)) {
            inside.Add(enemy);

            float curDamage = (dealsInitialDamage) ? initialDamage : 0f;
            enemy.poisonDamage(curDamage, false, curPoison, initialEnemyStacks);

            StartCoroutine(enemyStunSequence(enemy));
        }
    }

    // Main function for when enemy exits zone
    private void onEnemyExit(EnemyStatus enemy) {
        if (inside.Contains(enemy)) {
            inside.Remove(enemy);

            StartCoroutine(enemyStunSequence(enemy));
        }
    }


    // Main stun sequence for enemy
    private IEnumerator enemyStunSequence(EnemyStatus enemy) {
        enemy.stun(true);

        yield return new WaitForSeconds(stunTime);

        enemy.stun(false);
    }

}
