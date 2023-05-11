using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class IngredientSensor : MonoBehaviour
{
    HashSet<Ingredient> inRange = new HashSet<Ingredient>();
    [SerializeField]
    [Range(0f, 90f)]
    private float prioritizedAngle = 45f;
    [SerializeField]
    private TwitchInventory inventory;
    private Ingredient targetIngredient = null;


    // On update
    private void Update() {
        Ingredient prevIngredient = targetIngredient;
        targetIngredient = getClosestIngredient();

        if (prevIngredient != targetIngredient && prevIngredient != null) {
            prevIngredient.removeGlow();
        }

        if (targetIngredient != null) {
            targetIngredient.glow();
        }
    }
    
    
    
    // On trigger enter, add ingredient to hashset
    private void OnTriggerEnter(Collider collider) {
        Ingredient ing = collider.GetComponent<Ingredient>();

        if (ing != null && !inRange.Contains(ing)) {
            inRange.Add(ing);
        }
    }


    // On trigger exit, add ingredient to hashset
    private void OnTriggerExit(Collider collider) {
        Ingredient ing = collider.GetComponent<Ingredient>();

        if (ing != null && inRange.Contains(ing)) {
            inRange.Remove(ing);
        }
    }

    // Event handler method for when mouse position changes
    public void onPickupPress(InputAction.CallbackContext context) {
        if (context.started && targetIngredient != null) {

            // Try to pick up the ingredient if you can
            if (!inventory.addIngredient(targetIngredient)) {
                Debug.Log("NO ROOM IN INVENTORY");
            }
        }
    }


    // Main function to get the best ingredient
    private Ingredient getClosestIngredient() {
        float minDistance = -1f;
        Ingredient bestIng = null;

        foreach (Ingredient ingredient in inRange) {
            Vector3 distanceVector = new Vector3(ingredient.transform.position.x - transform.position.x, 0f, ingredient.transform.position.z - transform.position.z);
            float distance = distanceVector.magnitude;

            // Case in which you've found a prioritized target already
            if (distance < minDistance || minDistance < 0f) {
                minDistance = distance;
                bestIng = ingredient;
            }
        }

        return bestIng;
    }
}
