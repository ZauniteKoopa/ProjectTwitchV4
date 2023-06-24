using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    protected HashSet<PlayerStatus> inRange = new HashSet<PlayerStatus>();


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
            player.damage(damage, false);
        }
    }

    
}
