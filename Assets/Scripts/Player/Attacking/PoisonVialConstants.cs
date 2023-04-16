using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PoisonVialConstants : ScriptableObject
{
    [Header("Pure Vial Colors")]
    public Color purePotencyColor = Color.red;
    public Color pureReactivityColor = Color.yellow;
    public Color pureStickinessColor = Color.blue;
    public Color purePoisonColor = Color.green;


    [Header("In-Progress Vial Colors")]
    public Color tempPotencyColor = Color.red;
    public Color tempReactivityColor = Color.yellow;
    public Color tempStickinessColor = Color.blue;
    public Color tempPoisonColor = Color.green;
    public Color baseVialColor = Color.grey;


    [Header("Side Effects")]
    public SideEffect defaultSideEffect;


    
    // Main function to get the pure color
    public Color getPureColor(PoisonVialStat stat) {
        switch(stat)
        {
            case PoisonVialStat.POTENCY:
                return purePotencyColor;
            
            case PoisonVialStat.POISON:
                return purePoisonColor;

            case PoisonVialStat.REACTIVITY:
                return pureReactivityColor;

            case PoisonVialStat.STICKINESS:
                return pureStickinessColor;

            default:
                throw new System.Exception("INVALID STAT FOUND");
        }
    }


    // Main function to get the temp color
    public Color getTempColor(PoisonVialStat stat) {
        switch(stat)
        {
            case PoisonVialStat.POTENCY:
                return tempPotencyColor;
            
            case PoisonVialStat.POISON:
                return tempPoisonColor;

            case PoisonVialStat.REACTIVITY:
                return tempReactivityColor;

            case PoisonVialStat.STICKINESS:
                return tempStickinessColor;

            default:
                throw new System.Exception("INVALID STAT FOUND");
        }
    }
}
