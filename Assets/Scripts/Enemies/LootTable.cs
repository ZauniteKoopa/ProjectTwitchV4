using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu(menuName = "LootTable")]
public class LootTable : ScriptableObject
{
    [SerializeField]
    private LobAction[] lootDrops;
    [SerializeField]
    private float[] lootDropChances;
    [SerializeField]
    [Range(1, 6)]
    private int lootVariance = 4;
    private MultiConditionalProbCalculator<LobAction> probCalculator;


    // Main function to get a randomized ingredient drop from this loot table
    //  Pre: lootDrops and lootDropChances are matching and have more than 1 entry
    //  Post: returns a valid lob action with any loot attached
    // public LobAction getLootDrop() {
    //     Debug.Assert(lootDrops.Length == lootDropChances.Length);
    //     Debug.Assert(lootDrops.Length > 0);

    //     // Get the sum of all ingredient frop chances
    //     float chanceTotal = 0f;
    //     foreach (float dropChance in lootDropChances) {
    //         chanceTotal += dropChance;
    //     }

    //     // Roll the dice and choose which drop you get
    //     float diceRoll = Random.Range(0f, chanceTotal);
    //     int curIndex = 0;
    //     float maxChanceRequirement = lootDropChances[0];

    //     while (curIndex < lootDropChances.Length && diceRoll > maxChanceRequirement) {
    //         curIndex++;
    //         Debug.Assert(curIndex < lootDropChances.Length);
    //         maxChanceRequirement += lootDropChances[curIndex];
    //     }

    //     return lootDrops[curIndex];
    // }


    // Main function to get a randomized ingredient drop from this loot table
    //  Pre: lootDrops and lootDropChances are matching and have more than 1 entry
    //  Post: returns a valid lob action with any loot attached
    public LobAction getLootDrop() {
        if (probCalculator == null) {
            probCalculator = new MultiConditionalProbCalculator<LobAction>(
                lootDrops,
                lootDropChances,
                lootVariance
            );
        }

        return probCalculator.chooseRandomOutcome();
    }
}
