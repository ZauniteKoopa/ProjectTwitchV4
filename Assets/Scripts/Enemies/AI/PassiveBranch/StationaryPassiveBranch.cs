using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationaryPassiveBranch : IEnemyPassiveBranch
{
    // Main function to run the branch
    public override IEnumerator execute() {
        yield return 0;
    }

    // Main function to reset the branch when the overall tree gets overriden / switch branches
    public override void reset() {}
}
