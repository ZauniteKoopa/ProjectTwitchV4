using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarwickPassiveBranch : IBossBehaviorBranch
{
    // Main function to do additional initialization for branch
    //  Pre: none
    //  Post: sets branch up
    protected override void initialize() {

    }


    // Main function to execute the branch
    //  Pre: tgt is the player, cannot equal null
    //  Post: executes aggressive branch
    public override IEnumerator execute(Transform tgt, BossEnemyStatus enemyStatus) {
        yield return 0;
    }


    // Main function to reset the branch when the overall tree gets overriden / switch branches
    public override void reset() {

    }
}
