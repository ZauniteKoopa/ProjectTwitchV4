using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public abstract class IEnemyBehavior : MonoBehaviour
{
    // Event for when brain has been reset
    public UnityEvent behaviorResetEvent;
    public UnityEvent passiveBranchActiveEvent;
    public UnityEvent aggressiveBranchActiveEvent;

    // Main event handler function for when an enemy sensed a player
    //  Pre: player != null, enemy saw player
    public abstract void onSensedPlayer(Transform player);

    // Main event handler function for when an enemy lost sight of a player
    //  Pre: enemy lost sight of player and gave up chasing
    public abstract void onLostPlayer();

    // Main function to handle reset
    public abstract void reset();

    // Main function to handle death event of this unit
    public abstract void onDeath(IUnitStatus corpse);

    // Main function to access whether or not you're in the passive state or aggressive state
    public abstract bool inAggroState();


    // Main function to look at a specific direction
    //  Pre: lookDirection is the look direction that the enemy will be looking at
    //  Post: player will stop all coroutines to look at something for a specified number of seconds before going back to work
    public abstract void lookAt(Vector3 lookAtDirection);
}
