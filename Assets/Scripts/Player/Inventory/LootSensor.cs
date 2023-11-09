using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LootSensor : MonoBehaviour
{
    HashSet<PrizeLoot> inRange = new HashSet<PrizeLoot>();
    [SerializeField]
    private TwitchInventory inventory;
    [SerializeField]
    private PlayerStatus status;
    private PrizeLoot targetLoot = null;
    private bool showingGlow = true;


    // On update
    private void Update() {
        if (showingGlow) {
            PrizeLoot prevLoot = targetLoot;
            targetLoot = getClosestLoot();

            if (prevLoot != targetLoot && prevLoot != null) {
                prevLoot.removeGlow();
            }

            if (targetLoot != null) {
                targetLoot.glow();
            }

        } else {
            if (targetLoot != null) {
                targetLoot.removeGlow();
            }
        }
    }
    
    
    
    // On trigger enter, add Ingredient to hashset
    private void OnTriggerEnter(Collider collider) {
        PrizeLoot loot = collider.GetComponent<PrizeLoot>();

        if (loot != null && !inRange.Contains(loot)) {
            inRange.Add(loot);
        }
    }


    // On trigger exit, add Ingredient to hashset
    private void OnTriggerExit(Collider collider) {
        PrizeLoot loot = collider.GetComponent<PrizeLoot>();

        if (loot != null && inRange.Contains(loot)) {
            inRange.Remove(loot);
        }
    }

    // Event handler method for when mouse position changes
    public void onPickupPress(InputAction.CallbackContext context) {
        if (context.started && targetLoot != null && !PauseConstraints.isPaused()) {
            targetLoot.collect(status, inventory);
        }
    }


    // Main function to get the best ingredient
    private PrizeLoot getClosestLoot() {
        float minDistance = -1f;
        PrizeLoot bestLoot = null;

        foreach (PrizeLoot curLoot in inRange) {
            if (curLoot != null) {
                Vector3 distanceVector = new Vector3(curLoot.transform.position.x - transform.position.x, 0f, curLoot.transform.position.z - transform.position.z);
                float distance = distanceVector.magnitude;

                // Case in which you've found a prioritized target already
                if (distance < minDistance || minDistance < 0f) {
                    minDistance = distance;
                    bestLoot = curLoot;
                }
            }
        }

        return bestLoot;
    }


    // Main function to set the UI state
    public void showUIGlow(bool willShow) {
        showingGlow = willShow;
    }
}
