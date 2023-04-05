using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;

public enum PoisonVialStat {
    POTENCY,
    POISON,
    REACTIVITY,
    STICKINESS
}

public class PoisonVial
{
    // Static const variables
    private static readonly int STARTING_AMMO = 40;
    private static readonly int PRIMARY_BOLT_AMMO_COST = 1;

    // Main vial stats
    private Dictionary<PoisonVialStat, int> vialStats;
    private int ammo;
    private SideEffect sideEffect;

    // Main public events to listen to
    public UnityEvent contaminateExecuteEvent;


    // Main constructor
    //  Pre: side effect doesn't equal null and startingStat is one of the stats in the enum
    //  Post: creates a poison vial with 1 in the startingStat and 0 everything else
    public PoisonVial(PoisonVialStat startingStat, SideEffect effect) {
        Debug.Assert(effect != null);

        sideEffect = effect;
        vialStats = new Dictionary<PoisonVialStat, int>();
        ammo = STARTING_AMMO;

        foreach (PoisonVialStat stat in System.Enum.GetValues(typeof(PoisonVialStat))) {
            int statVal = (startingStat == stat) ? 1 : 0;
            vialStats.Add(stat, statVal);
        }
    }


    // Main accessor function to get the ammo of this vial
    public float getAmmo() {
        return ammo;
    }


    // Main accessor function to get the start-up frames for the primary attack
    public int getPrimaryAttackStartFrames() {
        return sideEffect.primaryAttackStartFrames;
    }


    // Main accessor function to get the start-up frames for the primary attack
    public int getPrimaryAttackEndFrames() {
        return sideEffect.primaryAttackEndFrames;
    }


    // Main fucntion to actually create and fire a projectile at attackDir direction from attacker's position
    //  Pre: attackDir is the direction of attack, attacker is the transform of the one attacking
    //  Post: returns true if projectile fires. Returns false if it didn't due to ammo constraints
    public bool firePrimaryAttack(Vector3 attackDir, Transform attacker) {
        // If not enough ammo, return
        if (ammo < PRIMARY_BOLT_AMMO_COST) {
            return false;
        }

        ammo -= PRIMARY_BOLT_AMMO_COST;
        sideEffect.firePrimaryAttack(attackDir, attacker);
        return true;
    }

}
