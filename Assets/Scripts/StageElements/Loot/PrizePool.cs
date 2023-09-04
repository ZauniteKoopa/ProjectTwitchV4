using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// EndDungeonPrizes - what you will find at the end of 1 dungeon
[System.Serializable]
public class EndReward {
    public LobAction[] rewards;
    public string rewardName;
    public RewardPrerequisite rewardPrerequisite = RewardPrerequisite.NONE;
}


// Enum that's based on requirements
public enum RewardPrerequisite {
    NONE,
    CAN_ADD_RECIPE,
    CAN_ADD_ING_SLOT
}


// Prize Pool class
[CreateAssetMenu(menuName = "PrizePool")]
public class PrizePool : ScriptableObject {

    [Header("Surplus State")]
    [SerializeField]
    private EndReward[] surplusEndRewards;
    [SerializeField]
    private float[] surplusEndRewardProbability;

    [Header("Scarcity State")]
    [SerializeField]
    private EndReward[] scarcityEndRewards;
    [SerializeField]
    private float[] scarcityEndRewardProbability;

    [Header("Main probability parameters")]
    [SerializeField]
    [Range(0f, 1f)]
    private float minIngredientProbability = 0.3f;
    [SerializeField]
    [Range(0f, 1f)]
    private float maxIngredientProbability = 0.7f;



    // Main function to get distinct end rewards
    //  Pre: numRewards is greater or equal to number of all possible end rewards
    //  Post: returns a list of length numRewards with rewards that are differrent from each other
    public List<EndReward> getDistinctEndRewards(int numRewards, TwitchInventory playerInventory) {
        Debug.Assert(numRewards <= getMinPossibleDistinctRewards() && numRewards >= 0);
        Debug.Assert(minIngredientProbability <= maxIngredientProbability);

        // Early return to avoid infinite loop in playtesting
        if (numRewards > getMinPossibleDistinctRewards()) {
            Debug.LogError("BAD PRIZE POOL CONFIGURATION. NUM REWARDS ASKED FOR EXCEEDS MIN POSSIBLE DISTINCT REWARDS");
            return null;
        }

        // Prepare counters
        HashSet<EndReward> endRewards = new HashSet<EndReward>();
        int numIngredientRewards = 0;
        int numNonIngredientRewards = 0;
        int maxIngredientRewards = getCurMaxPossibleDistinctRewards(scarcityEndRewards, playerInventory);
        int maxNonIngredientRewards = getCurMaxPossibleDistinctRewards(scarcityEndRewards, playerInventory);
        float playerIngredientInvState = playerInventory.getIngredientScarcitySurplusState();

        // Main loop
        while (endRewards.Count < numRewards) {
            // Roll the dice to see if you'll get an ingredient in the end rewards. Then check if you have reached the limit concerning that type of award
            bool willGetIngredient = Random.Range(0f, 1f) <= Mathf.Lerp(maxIngredientProbability, minIngredientProbability, playerIngredientInvState);
            willGetIngredient = (willGetIngredient) ? numIngredientRewards < maxIngredientRewards : numNonIngredientRewards >= maxNonIngredientRewards;

            // Actually get an endReward from the selected endReward pool
            EndReward[] curEndRewards = (willGetIngredient) ? scarcityEndRewards : surplusEndRewards;
            float[] curEndRewardProbs = (willGetIngredient) ? scarcityEndRewardProbability : surplusEndRewardProbability;
            endRewards.Add(getDistinctEndReward(playerInventory, curEndRewards, curEndRewardProbs, endRewards));

            // Update counters
            if (willGetIngredient) {
                numIngredientRewards++;
            } else {
                numNonIngredientRewards++;
            }
        }

        // Convert that to a list
        List<EndReward> rewardsList = new List<EndReward>();
        foreach (EndReward reward in endRewards) {
            rewardsList.Add(reward);
        }

        Debug.Assert(rewardsList.Count == numRewards);
        return rewardsList;
    }


    // Main private helper function to get distinct end rewards given specified probabilities
    public EndReward getDistinctEndReward(
        TwitchInventory playerInventory,
        EndReward[] givenEndRewards,
        float[] givenEndRewardProbability,
        HashSet<EndReward> hitEndRewards
    ) {
        Debug.Assert(givenEndRewards.Length == givenEndRewardProbability.Length);

        // Keep finding rewards until you find numRewards rewards
        float initialDiceRoll = getDiceRoll(givenEndRewardProbability);

        // Get the reward index from the dice roll
        int rewardIndex = 0;
        float indicatorCheck = givenEndRewardProbability[0];
        while (rewardIndex < givenEndRewards.Length && initialDiceRoll > indicatorCheck) {
            rewardIndex++;

            if (rewardIndex < givenEndRewards.Length) {
                indicatorCheck += givenEndRewardProbability[rewardIndex];
            }
        }

        // If reward already found in the hashset, go through the list until you find one that isn't
        bool positiveDirection = Random.Range(0, 2) == 0;
        while (hitEndRewards.Contains(givenEndRewards[rewardIndex])
                || !metEndRewardPreRequisite(playerInventory, givenEndRewards[rewardIndex].rewardPrerequisite))
        {
            rewardIndex += (positiveDirection) ? 1 : (givenEndRewards.Length - 1);
            rewardIndex %= givenEndRewards.Length;
        }
        
        return givenEndRewards[rewardIndex];
    }


    // Main function to get a dice roll among all of the probabilities
    private float getDiceRoll(float[] givenEndRewardProbability) {
        float totalSum = 0f;

        foreach (float prob in givenEndRewardProbability) {
            totalSum += prob;
        }

        return Random.Range(0f, totalSum);
    }


    // Main function to check if the inventory has met the prerequisite
    private bool metEndRewardPreRequisite(TwitchInventory playerInventory, RewardPrerequisite preReq) {
        switch (preReq) {
            case RewardPrerequisite.CAN_ADD_RECIPE:
                return playerInventory.canAddNewRecipes();

            case RewardPrerequisite.CAN_ADD_ING_SLOT:
                return playerInventory.canAddIngredientSlots();

            case RewardPrerequisite.NONE:
                return true;

            default:
                Debug.LogError("ERROR: UNKNOWN PREREQUISITE FOUND");
                return true;
        }
    }


    // Main function to check the min possible rewards for the prize pool
    private int getMinPossibleDistinctRewards() {
        return getMinPossibleDistinctRewards(scarcityEndRewards) + getMinPossibleDistinctRewards(surplusEndRewards);
    }


    // Main function to check the min possible rewards for the prize pool
    private int getMinPossibleDistinctRewards(EndReward[] givenEndRewards) {
        int count = 0;

        foreach (EndReward reward in givenEndRewards) {
            if (reward.rewardPrerequisite == RewardPrerequisite.NONE) {
                count++;
            }
        }

        return count;
    }


    // Main function to get the curMaxPossibleDistinctRewards
    private int getCurMaxPossibleDistinctRewards(EndReward[] givenEndRewards, TwitchInventory playerInv) {
        int count = 0;

        foreach (EndReward reward in givenEndRewards) {
            if (metEndRewardPreRequisite(playerInv, reward.rewardPrerequisite)) {
                count++;
            }
        }

        return count;
    }

}
