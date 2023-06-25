using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLingeringHitbox : MonoBehaviour
{
    [SerializeField]
    private float hitboxDamage = 2f;


    private void OnTriggerStay(Collider collider) {
        PlayerStatus playerTarget = collider.GetComponent<PlayerStatus>();

        if (playerTarget != null) {
            playerTarget.damage(hitboxDamage, false);
        }
    }


    // Main function to set damage
    public void setDamage(float damage) {
        hitboxDamage = damage;
    }
}
