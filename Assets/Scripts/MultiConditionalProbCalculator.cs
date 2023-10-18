using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiConditionalProbCalculator<T>
{
    // Instance variables
    private int maxTally;
    private Dictionary<T, float> probabilityChances;
    private Dictionary<T, int> probabilityTallies;


    // Main constructor
    //  Pre: probOutcomes is the list of outcomes to return
    //       probChances is the matching probabilities for those outcomes (same length as probOutcomes)
    //       probVariance is an integer discussing how varied the probability will be (higher the number, higher the variance, less predictable)
    public MultiConditionalProbCalculator(T[] probOutcomes, float[] probChances, int probVariance) {
        Debug.Assert(probOutcomes.Length == probChances.Length);
        Debug.Assert(probOutcomes.Length > 0);
        
        maxTally = probVariance;
        probabilityChances = new Dictionary<T, float>();
        probabilityTallies = new Dictionary<T, int>();

        for(int i = 0; i < probChances.Length; i++) {
            probabilityChances.Add(probOutcomes[i], probChances[i]);
            probabilityTallies.Add(probOutcomes[i], 0);
        }
    }


    // Main function to choose a random outcome
    public T chooseRandomOutcome() {
        // Check if you need to reset
        if (allTalliesMaxed()) {
            resetProbabilities();
        }

        // Get the sum of all ingredient frop chances
        float chanceTotal = getMaxProbability();
        Debug.Assert(chanceTotal > 0f);

        // Roll the dice and choose which drop you get
        float diceRoll = Random.Range(0f, chanceTotal);
        float curProbThreshold = 0f;

        foreach(KeyValuePair<T, float> outcomeProb in probabilityChances) {
            if (canAchieveOutcome(outcomeProb.Key)) {
                curProbThreshold += outcomeProb.Value;

                if (diceRoll <= curProbThreshold) {
                    probabilityTallies[outcomeProb.Key]++;
                    return outcomeProb.Key;
                }
            }
        }

        Debug.LogError("PROBABILITY BROKE SOMEHOW");
        return default(T);
    }


    // Main private helper function to reset
    private void resetProbabilities() {
        List<T> keys = new List<T>();
        foreach(T key in probabilityTallies.Keys) {
            keys.Add(key);
        }

        foreach(T key in keys) {
            probabilityTallies[key] = 0;
        }
    }


    // Main function to check if all tallies are met
    private bool allTalliesMaxed() {
        foreach(KeyValuePair<T, int> tally in probabilityTallies) {
            if (probabilityTallies[tally.Key] < maxTally) {
                return false;
            }
        }

        return true;
    }


    // Main function to get the current max probability
    private float getMaxProbability() {
        float curMax = 0f;

        foreach(KeyValuePair<T, float> outcomeProb in probabilityChances) {
            if (canAchieveOutcome(outcomeProb.Key)) {
                curMax += outcomeProb.Value;
            }
        }

        return curMax;
    }


    // Main helper function to check if you can get outcome
    private bool canAchieveOutcome(T outcome) {
        Debug.Assert(probabilityTallies.ContainsKey(outcome));
        return probabilityTallies[outcome] < maxTally;
    }
}
