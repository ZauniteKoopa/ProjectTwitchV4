using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class PoisonCompositionDisplay : MonoBehaviour
{
    [SerializeField]
    private Image[] potencySlots;
    [SerializeField]
    private Image[] poisonSlots;
    [SerializeField]
    private Image[] reactivitySlots;
    [SerializeField]
    private Image[] stickinessSlots;

    private Dictionary<PoisonVialStat, Image[]> poisonSlotsMap = new Dictionary<PoisonVialStat, Image[]>();

    private Color filledColor = Color.red;
    private bool initialized = false;



    // On awake, setup the map and error check
    private void Awake() {
        if (!initialized) {
            addStatMapping(PoisonVialStat.POTENCY, potencySlots);
            addStatMapping(PoisonVialStat.POISON, poisonSlots);
            addStatMapping(PoisonVialStat.REACTIVITY, reactivitySlots);
            addStatMapping(PoisonVialStat.STICKINESS, stickinessSlots);
        }
    }



    // Main function to add slots to mapping
    private void addStatMapping(PoisonVialStat stat, Image[] slots) {
        if (slots == null || slots.Length != 3) {
            Debug.LogError("SLOTS HAVE NOT BEEN SET UP FOR A STAT");
        }

        poisonSlotsMap.Add(stat, slots);
    }


    // Public function to display stat composition
    //  Pre: poisonComp is not null and is the size of 4, individualStats >= 3
    public void displayComposition(Dictionary<PoisonVialStat, int> poisonComp) {
        Debug.Assert(poisonComp != null && poisonComp.Count == 4);

        if (!initialized) {
            Awake();
        }

        foreach(KeyValuePair<PoisonVialStat, int> entry in poisonComp) {
            // do something with entry.Value or entry.Key
            Debug.Assert(entry.Value <= 3 && poisonSlotsMap.ContainsKey(entry.Key));

            Image[] statSlots = poisonSlotsMap[entry.Key];
            for (int s = 0; s < statSlots.Length; s++) {
                statSlots[s].color = (s < entry.Value) ? filledColor : Color.black;
            }
        }
    }


    // Main function to display empty
    public void displayEmptyComp() {
        if (!initialized) {
            Awake();
        }
        
        foreach(KeyValuePair<PoisonVialStat, Image[]> entry in poisonSlotsMap) {
            for (int s = 0; s < entry.Value.Length; s++) {
                entry.Value[s].color = Color.black;
            }
        }
    }

}
