using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public abstract class IEnemyBehavior : MonoBehaviour
{
    // Event for when brain has been reset
    public UnityEvent behaviorResetEvent;

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
}
