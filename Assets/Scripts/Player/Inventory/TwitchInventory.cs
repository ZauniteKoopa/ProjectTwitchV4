using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwitchInventory : MonoBehaviour
{
    // Poison Vial Info
    [SerializeField]
    private SideEffect startingSideEffect;
    private PoisonVial primaryVial;

    // Start is called before the first frame update
    void Awake()
    {
        primaryVial = new PoisonVial(PoisonVialStat.POTENCY, startingSideEffect);
    }


    // Main function to check if you're carrying a vial or not
    public bool carryingPrimaryVial() {
        return primaryVial != null;
    }


    // Main function to get primary variable frame data
    public int getPrimaryStartFrame() {
        return primaryVial.getPrimaryAttackStartFrames();
    }


    // Main function to get primary variable frame data
    public int getPrimaryEndFrame() {
        return primaryVial.getPrimaryAttackEndFrames();
    }


    // Main function to fire the primary vial if it's possible
    //  Pre: the attackDirection is the direction of attack
    //  Post: returns true if successful. false otherwise
    public bool firePrimaryBolt(Vector3 attackDir, Transform attacker) {
        if (primaryVial == null) {
            return false;
        }

        // Fire bullet and then check ammo afterwards
        bool success = primaryVial.firePrimaryAttack(attackDir, attacker);
        if (success && primaryVial.getAmmo() <= 0) {
            primaryVial = null;
        }

        return success;
    }
    
}
