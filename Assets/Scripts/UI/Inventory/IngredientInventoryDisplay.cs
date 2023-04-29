using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

public class IngredientInventoryDisplay : MonoBehaviour
{
    [SerializeField]
    private IngredientIcon potencyIcon;
    [SerializeField]
    private IngredientIcon poisonIcon;
    [SerializeField]
    private IngredientIcon reactivityIcon;
    [SerializeField]
    private IngredientIcon stickinessIcon;
    [SerializeField]
    private TMP_Text inventoryLabel;
    [SerializeField]
    private Color normalColor = Color.white;
    [SerializeField]
    private Color filledColor = Color.yellow;
    [SerializeField]
    private Canvas selectedCanvasLayer = null;

    private Dictionary<PoisonVialStat, IngredientIcon> iconMap = new Dictionary<PoisonVialStat, IngredientIcon>();


    // On awake, set up map
    private void Awake() {
        addMapping(PoisonVialStat.POTENCY, potencyIcon);
        addMapping(PoisonVialStat.POISON, poisonIcon);
        addMapping(PoisonVialStat.REACTIVITY, reactivityIcon);
        addMapping(PoisonVialStat.STICKINESS, stickinessIcon);

        if (inventoryLabel == null) {
            Debug.LogError("Inventory label is null");
        }

        // Testing stuff
        Dictionary<PoisonVialStat, int> testIngredientInv = new Dictionary<PoisonVialStat, int>();
        testIngredientInv.Add(PoisonVialStat.POTENCY, 2);
        testIngredientInv.Add(PoisonVialStat.POISON, 1);
        testIngredientInv.Add(PoisonVialStat.REACTIVITY, 0);
        testIngredientInv.Add(PoisonVialStat.STICKINESS, 0);

        display(testIngredientInv, 5);
    }


    
    // Main function to add an icon to the mapping
    private void addMapping(PoisonVialStat stat, IngredientIcon icon) {
        if (icon == null) {
            Debug.LogError("AN ICON WASN'T MAPPED TO THE FOLLOWING STAT: " + stat);
        }

        iconMap.Add(stat, icon);
    }


    // Main function to display the ingredient inventory
    //  Pre: ingredientInventory != null && inventoryCount >= maxSize
    public void display(Dictionary<PoisonVialStat, int> ingredientInventory, int maxSize) {
        Debug.Assert(ingredientInventory != null && ingredientInventory.Count == iconMap.Count);

        int inventoryCount = 0;

        foreach(KeyValuePair<PoisonVialStat, int> entry in ingredientInventory) {
            // do something with entry.Value or entry.Key
            Debug.Assert(iconMap.ContainsKey(entry.Key));

            iconMap[entry.Key].SetUpIcon(entry.Key, entry.Value, selectedCanvasLayer.transform);
            inventoryCount += entry.Value;
        }

        Debug.Assert(inventoryCount <= maxSize);
        inventoryLabel.text = inventoryCount + "/" + maxSize;
        inventoryLabel.color = (inventoryCount >= maxSize) ? filledColor : normalColor;
    }
}
