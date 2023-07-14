using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// EndDungeonPrizes - what you will find at the end of 1 dungeon
[System.Serializable]
public class EndReward {
    public LobAction[] rewards;
    public string rewardName;
}


// Prize Pool class
[CreateAssetMenu(menuName = "PrizePool")]
public class PrizePool : ScriptableObject {
    [SerializeField]
    private EndReward[] possibleEndRewards;


    // Main function to get distinct end rewards
    //  Pre: numRewards is greater or equal to number of possibleEndRewards
    //  Post: returns a list of length numRewards with rewards that are differrent from each other
    public List<EndReward> getDistinctEndRewards(int numRewards) {
        Debug.Assert(numRewards <= possibleEndRewards.Length);

        // Keep finding rewards until you find numRewards rewards
        HashSet<EndReward> endRewards = new HashSet<EndReward>();
        while (endRewards.Count < numRewards) {

            // Pick a random reward
            int rewardIndex = Random.Range(0, possibleEndRewards.Length);

            // If reward already found in the hashset, go through the list until you find one that isn't
            while (endRewards.Contains(possibleEndRewards[rewardIndex])) {
                rewardIndex = (rewardIndex + 1) % possibleEndRewards.Length;
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


}
