using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IEnemyPassiveBranch : MonoBehaviour
{
    // Main function to run the branch
    public abstract IEnumerator execute();

    // Main function to reset the branch when the overall tree gets overriden / switch branches
    public abstract void reset();

    // Main function to do a hard reset: a reset in hich the enemy respawns from scratch
    //  Pre: enemy respawns from scratch
    //  Post: resets as if nothing happened to this branch (by default, just reset)
    public virtual void hardReset() {
        reset();
    }
}
