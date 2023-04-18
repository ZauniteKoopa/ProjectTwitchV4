using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IAimAssist : MonoBehaviour
{
    // Main function to adjust the aim direction so that it can accurately hit an enemy
    //  Pre: aimDirection is the direction of the attack, playerPosition is the position of the player
    //  Post: returns adjusted aimDirection IFF there's an enemy in that general direction
    public abstract Vector3 adjustAim(Vector3 aimDirection, Vector3 playerPosition);


    // Main function to reset aim assist
    //  Pre: none
    //  Post: aim assist is perfectly reset
    public abstract void reset();
}
