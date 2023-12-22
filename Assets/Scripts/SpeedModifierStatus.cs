using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class SpeedModifierStatus
{
    // Variables concerning buffs
    private SortedDictionary<float, int> speedBuffs;

    // Variables concerning debuffs
    private SortedDictionary<float, int> speedDebuffs;


    // Main constructor
    public SpeedModifierStatus() {
        speedBuffs = new SortedDictionary<float, int>();
        speedDebuffs = new SortedDictionary<float, int>();
    }
    
    
    // Main function to apply a speed modifiere
    //  Pre: a float that's greater than 0 and doesn't equal 1
    //  Post: speed modifier is now considered
    public void applySpeedModifier(float speedModifier) {
        Debug.Assert(speedModifier > 0f);

        if (speedModifier > 1.01f || speedModifier < 0.99f) {
            SortedDictionary<float, int> curDict = (speedModifier > 1f) ? speedBuffs : speedDebuffs;
            int freq;

            if (curDict.TryGetValue(speedModifier, out freq)) {
                curDict[speedModifier] = freq + 1;
            } else {
                curDict.Add(speedModifier, 1);
            }
        }
    }


    // Main function to revert speed modifier
    //  Pre: a float that's greater than 0 and doesn't equal 1
    //  Post: speed modifier is now reverted if it exists
    public void revertSpeedModifier(float speedModifier) {
        Debug.Assert(speedModifier > 0f);

        if (speedModifier > 1.01f || speedModifier < 0.99f) {
            SortedDictionary<float, int> curDict = (speedModifier > 1f) ? speedBuffs : speedDebuffs;
            int freq;

            if (curDict.TryGetValue(speedModifier, out freq)) {
                if (freq > 1) {
                    curDict[speedModifier] = freq - 1;
                } else {
                    curDict.Remove(speedModifier);
                }
            }
        }
    }


    // Main function to get speed modifier from this speed modifier status
    //  Pre: none
    //  Post: returns a float representing the speed modifier
    public float getSpeedModifier() {
        float totalBuffModifier = (speedBuffs.Count > 0) ? speedBuffs.Keys.Last() : 1f;
        float totalDebuffModifier = (speedDebuffs.Count > 0) ? speedDebuffs.Keys.First() : 1f;

        return totalBuffModifier * totalDebuffModifier;
    }


    // Main function to clear all debuffs
    public void clearAllDebuffs() {
        speedDebuffs.Clear();
    }


    // Main function to clear all buffs
    public void clearAllBuffs() {
        speedBuffs.Clear();
    }

    
    // Main function to clear all speed modifiers
    public void clearAllModifiers() {
        clearAllBuffs();
        clearAllDebuffs();
    }
}
