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
    CAN_ADD_RECIPE
}


// Prize Pool class
[CreateAssetMenu(menuName = "PrizePool")]
public class PrizePool : ScriptableObject {
    [SerializeField]
    private EndReward[] possibleEndRewards;
    [SerializeField]
    private float[] endRewardsProbability;


    // Main function to get distinct end rewards
    //  Pre: numRewards is greater or equal to number of possibleEndRewards
    //  Post: returns a list of length numRewards with rewards that are differrent from each other
    public List<EndReward> getDistinctEndRewards(int numRewards, TwitchInventory playerInventory) {
        Debug.Assert(numRewards <= getMinPossibleDistinctRewards());
        Debug.Assert(possibleEndRewards.Length == endRewardsProbability.Length);

        // Early return to avoid infinite loop in playtesting
        if (numRewards > getMinPossibleDistinctRewards()) {
            Debug.LogError("BAD PRIZE POOL CONFIGURATION. NUM REWARDS ASKED FOR EXCEEDS MIN POSSIBLE DISTINCT REWARDS");
            return null;
        }

        // Keep finding rewards until you find numRewards rewards
        HashSet<EndReward> endRewards = new HashSet<EndReward>();
        float initialDiceRoll = getDiceRoll();

        while (endRewards.Count < numRewards) {
            // Get the reward index from the dice roll
            int rewardIndex = 0;
            float indicatorCheck = endRewardsProbability[0];
            while (rewardIndex < possibleEndRewards.Length && initialDiceRoll > indicatorCheck) {
                rewardIndex++;

                if (rewardIndex < possibleEndRewards.Length) {
                    indicatorCheck += endRewardsProbability[rewardIndex];
                }
            }

            // If reward already found in the hashset, go through the list until you find one that isn't
            bool positiveDirection = Random.Range(0, 2) == 0;
            while (endRewards.Contains(possibleEndRewards[rewardIndex])
                    || !metEndRewardPreRequisite(playerInventory, possibleEndRewards[rewardIndex].rewardPrerequisite))
            {
                rewardIndex += (positiveDirection) ? 1 : (possibleEndRewards.Length - 1);
                rewardIndex %= possibleEndRewards.Length;
            }

            // Add to hashset
            endRewards.Add(possibleEndRewards[rewardIndex]);
        }


        // Convert that to a list
        List<EndReward> rewardsList = new List<EndReward>();
        foreach (EndReward reward in endRewards) {
            rewardsList.Add(reward);
        }

        Debug.Assert(rewardsList.Count == numRewards);
        return rewardsList;
    }


    // Main function to get a dice roll among all of the probabilities
    private float getDiceRoll() {
        float totalSum = 0f;

        foreach (float prob in endRewardsProbability) {
            totalSum += prob;
        }

        return Random.Range(0f, totalSum);
    }


    // Main function to check if the inventory has met the prerequisite
    private bool metEndRewardPreRequisite(TwitchInventory playerInventory, RewardPrerequisite preReq) {
        switch (preReq) {
            case RewardPrerequisite.CAN_ADD_RECIPE:
                return playerInventory.canAddNewRecipes();

            case RewardPrerequisite.NONE:
                return true;

            default:
                Debug.LogError("ERROR: UNKNOWN PREREQUISITE FOUND");
                return true;
        }
    }


    // Main function to check the min possible rewards for the prize pool
    private int getMinPossibleDistinctRewards() {
        int count = 0;

        foreach (EndReward reward in possibleEndRewards) {
            if (reward.rewardPrerequisite == RewardPrerequisite.NONE) {
                count++;
            }
        }

        return count;
    }


}
