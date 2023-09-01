using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionalProbCalculator
{
    // Main private instance variables (for every <numAttempts> you get <numHits>)
    private int numHits;
    private int numAttempts;

    // Runtime variables for 1 rotation
    private int curAttempt = 0;
    private int curHitsMade = 0;


    // Main constructor
    //  Pre: all variables are positive numbers
    public ConditionalProbCalculator(int hits, int attempts, int probVariance = 1) {
        Debug.Assert(hits > 0 && attempts > 0 && probVariance > 0);

        numHits = hits * probVariance;
        numAttempts = attempts * probVariance;
    }


    // Main function to see if you rolled correctly
    // returns true if you rolled a hit. You are guaranteed <numHits> for every <numAttempts>
    public bool rolledHit() {
        // Roll the dice
        int diceRoll = Random.Range(0, numAttempts - curAttempt);
        bool getHit = (diceRoll < numHits - curHitsMade);

        // Update runtime rotation functions
        if (getHit) {
            curHitsMade++;
        }
        curAttempt++;

        // If reached max number of attempts of this current rotation, reset
        if (curAttempt == numAttempts) {
            Debug.Assert(curHitsMade == numHits);

            curHitsMade = 0;
            curAttempt = 0;
        }

        return getHit;
    }
}
