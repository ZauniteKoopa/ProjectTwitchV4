using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IPrimaryAttack : MonoBehaviour
{
    // Main function to set up the projectile
    //  Pre: dir is the direction the projectile will move towards, dmg > 0, PoisonVial is the poison associated with this attack
    //  Post: sets up primary attack 
    public abstract void setUp(Vector3 dir, float dmg, PoisonVial poison, float range = -1f);
}
