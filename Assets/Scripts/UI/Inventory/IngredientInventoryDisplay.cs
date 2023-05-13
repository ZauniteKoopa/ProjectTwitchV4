using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using TMPro;

[System.Serializable]
public class PoisonVialStatDelegate : UnityEvent<PoisonVialStat> {}

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
    private bool initialized = false;

    public PoisonVialStatDelegate removeIngredientEvent;


    // On awake, set up map
    private void Awake() {
        if (!initialized) {
            addMapping(PoisonVialStat.POTENCY, potencyIcon);
            addMapping(PoisonVialStat.POISON, poisonIcon);
            addMapping(PoisonVialStat.REACTIVITY, reactivityIcon);
            addMapping(PoisonVialStat.STICKINESS, stickinessIcon);

            if (inventoryLabel == null) {
                Debug.LogError("Inventory label is null");
            }

            initialized = true;
        }
    }


    
    // Main function to add an icon to the mapping
    private void addMapping(PoisonVialStat stat, IngredientIcon icon) {
        if (icon == null) {
            Debug.LogError("AN ICON WASN'T MAPPED TO THE FOLLOWING STAT: " + stat);
        }

        iconMap.Add(stat, icon);
        icon.successfulRemoveEvent.AddListener(onRemoveIngredient);
    }


    // Main function to display the ingredient inventory
    //  Pre: ingredientInventory != null && inventoryCount >= maxSize
    public void display(Dictionary<PoisonVialStat, int> ingredientInventory, int maxSize) {
        if (!initialized) {
            Awake();
        }
        
        Debug.Assert(ingredientInventory != null && ingredientInventory.Count == iconMap.Count);
        
        int inventoryCount = 0;

        foreach(KeyValuePair<PoisonVialStat, int> entry in ingredientInventory) {
            Debug.Assert(iconMap.ContainsKey(entry.Key));

            iconMap[entry.Key].SetUpIcon(entry.Key, entry.Value, selectedCanvasLayer.transform);
            inventoryCount += entry.Value;
        }

        Debug.Assert(inventoryCount <= maxSize);
        inventoryLabel.text = inventoryCount + "/" + maxSize;
        inventoryLabel.color = (inventoryCount >= maxSize) ? filledColor : normalColor;
    }


    // Main event handler function for when a remove ingredient button has been clicked
    public void onRemoveIngredient(PoisonVialStat stat) {
        removeIngredientEvent.Invoke(stat);
    }
}
