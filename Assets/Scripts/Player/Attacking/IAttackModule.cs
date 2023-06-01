using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Class to represent interface between attack modules and movement controllers
//  when attacking, movement should be affected
public abstract class IAttackModule : MonoBehaviour {

    // Main unity events to consider
    public UnityEvent abilityOneTrigger;
    public UnityEvent abilityTwoTrigger;
    

    // Function to return movement speed factor affected by this attack module
    //  Pre: none
    //  Post: returns a float that tells how much movement speed should be reduced by currently
    public abstract float getMovementSpeedFactor();


    // Function to get the new forward calculated by this attack module
    //  Pre: newForward needs to be any vector3
    //  Post: returns whether forward should be overriden and puts overriden forward into newForward
    public abstract bool getNewForward(out Vector3 newForward);


    
    // Function to see if player is still shooting
    //  Pre: none
    //  Post: returns if player is shooting
    public abstract bool isShooting();



    // Main function to see if you're in the middle of dashing or not
    public abstract bool isDashing();
}
