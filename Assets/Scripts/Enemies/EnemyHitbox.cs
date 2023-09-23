using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    protected HashSet<PlayerStatus> inRange = new HashSet<PlayerStatus>();
    [SerializeField]
    private LayerMask hitboxCollisionMask;


    // Main on trigger enter
    private void OnTriggerEnter(Collider collider) {
        PlayerStatus tgt = collider.GetComponent<PlayerStatus>();

        if (tgt != null) {
            inRange.Add(tgt);
        }
    }


    // Main on trigger exit
    private void OnTriggerExit(Collider collider) {
        PlayerStatus tgt = collider.GetComponent<PlayerStatus>();

        if (tgt != null) {
            inRange.Remove(tgt);
        }
    }


    // Main function to do damage to all inrange players
    public void doDamage(float damage) {
        foreach (PlayerStatus player in inRange) {
            // Set up raycast
            Vector3 rayCenter = transform.position;
            Vector3 rayDir = player.transform.position - transform.position;
            float rayDist = rayDir.magnitude;
            rayDir.Normalize();

            // If no obstructions, do damage
            if (!Physics.Raycast(rayCenter, rayDir, rayDist, hitboxCollisionMask)) {
                applyHitboxEffect(damage, player);
            }
        }
    }


    // Main function to apply hitbox effect
    protected virtual void applyHitboxEffect(float damage, PlayerStatus player) {
        player.damage(damage, false);
    }
}
