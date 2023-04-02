using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwitchAttackModule : IAttackModule
{
    // Function to return movement speed factor affected by this attack module
    //  Pre: none
    //  Post: returns a float that tells how much movement speed should be reduced by currently
    public override float getMovementSpeedFactor() {
        return 1.0f;
    }


    // Function to get the new forward calculated by this attack module
    //  Pre: newForward needs to be any vector3
    //  Post: returns whether forward should be overriden and puts overriden forward into newForward
    public override bool getNewForward(out Vector3 newForward) {
        newForward = transform.forward;
        return false;
    }
}
