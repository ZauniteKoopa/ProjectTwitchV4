using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManicGas : DeployableHitbox
{
    private PlayerStatus playerUnit = null;
    [SerializeField]
    private EnemyStatusSensor enemyStatusSensor = null;
    [SerializeField]
    [Range(1f, 2.5f)]
    private float attackBuff = 1.5f;
    [SerializeField]
    [Range(0.01f, 1f)]
    private float defenseDebuff = 0.5f;
    [SerializeField]
    [Min(0.1f)]
    private float gasDuration = 8f;


    private void Awake() {
        enemyStatusSensor.enemyEnterEvent.AddListener(onEntityEnter);
        enemyStatusSensor.enemyExitEvent.AddListener(onEntityExit);
    }


    // Main abstract protected function to override that represents what this hitbox does in its lifespan
    //  Pre: none
    //  Post: hitbox will stay for a duration, doing whatever it wants. by the end of it, it should kill itself
    protected override IEnumerator lifespan(PoisonVial p) {
        yield return new WaitForSeconds(gasDuration);

        transform.Translate(Vector3.up * -800000f);
        yield return new WaitForSeconds(0.25f);
        Object.Destroy(gameObject);
    }


    // On Entity enter
    private void onEntityEnter(IUnitStatus entity) {
        entity.applyDefenseModifier(defenseDebuff);
        entity.applyAttackModifier(attackBuff);
    }


    // On entity exit
    private void onEntityExit(IUnitStatus entity) {
        entity.revertDefenseModifier(defenseDebuff);
        entity.revertAttackModifier(attackBuff);
    }


    // On player enter
    private void OnTriggerEnter(Collider collider) {
        PlayerStatus curTgt = collider.GetComponent<PlayerStatus>();
        if (curTgt != null && playerUnit == null) {
            playerUnit = curTgt;
            onEntityEnter(curTgt);
        }
    }


    // On player exit
    private void OnTriggerExit(Collider collider) {
        PlayerStatus curTgt = collider.GetComponent<PlayerStatus>();
        if (curTgt != null && playerUnit == curTgt) {
            onEntityExit(playerUnit);
            playerUnit = null;
        }
    }
}
