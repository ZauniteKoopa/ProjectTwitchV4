using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AcidicBurrow : DeployableHitbox
{
    [SerializeField]
    [Min(0f)]
    private float explosionDamage = 7.5f;
    [SerializeField]
    private MeleePoisonHitbox explosionHitbox;
    [SerializeField]
    [Min(0.01f)]
    private float maxBurrowDuration = 8f;
    [SerializeField]
    [Min(0.01f)]
    private float maxHitboxDuration = 0.1f;
    [SerializeField]
    [Range(0.01f, 1f)]
    private float speedDebuff = 0.5f;


    private bool burrowed = false;
    private PlayerInput playerInput = null;
    private MeshRenderer meshRenderer = null;
    private float curExplosionRadius = 0f;
    private PoisonVial curPoison;
    private EnemyStatusSensor enemyStatusSensor = null;


    // On awake, get component
    private void Awake() {
        playerInput = GetComponent<PlayerInput>();
        meshRenderer = GetComponent<MeshRenderer>();
        enemyStatusSensor = GetComponent<EnemyStatusSensor>();

        curExplosionRadius = transform.localScale.x;
        enemyStatusSensor.enemyEnterEvent.AddListener(onEnemyEnter);
        enemyStatusSensor.enemyExitEvent.AddListener(onEnemyExit);
    }


    // Main abstract protected function to override that represents what this hitbox does in its lifespan
    //  Pre: none
    //  Post: hitbox will stay for a duration, doing whatever it wants. by the end of it, it should kill itself
    protected override IEnumerator lifespan(PoisonVial poison) {
        float timer = 0f;
        curPoison = poison;

        while (timer < maxBurrowDuration && !burrowed) {
            yield return 0;
            timer += Time.deltaTime;
        }


        if (burrowed) {
            meshRenderer.enabled = false;
            yield return new WaitForSeconds(maxHitboxDuration);
        }

        transform.Translate(Vector3.up * 500000f);
        yield return 0;
        yield return 0;
        yield return 0;
        yield return 0;
        Object.Destroy(gameObject);
    }


    // Main function to handle when you press the action button
    public void onSecondaryActionPress(InputAction.CallbackContext context) {
        if (context.started && !burrowed) {
            burrowed = true;

            source.position = new Vector3(transform.position.x, source.position.y, transform.position.z);
            explosionHitbox.setUp(Vector3.forward, explosionDamage, curPoison, curExplosionRadius);
            explosionHitbox.gameObject.SetActive(true);
        }
    }


    // Main function to handle when enemies enter zone
    private void onEnemyEnter(EnemyStatus enemy) {
        enemy.applySpeedModifier(speedDebuff);
    }


    // Main function to handle when enemies exit zone
    private void onEnemyExit(EnemyStatus enemy) {
        enemy.revertSpeedModifier(speedDebuff);
    }
}
