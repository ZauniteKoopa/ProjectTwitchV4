using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu(menuName = "PoisonSideEffects/SideEffect")]
public class SideEffect : ScriptableObject
{
    // Primary attack variables
    [Header("Primary Attack")]
    [SerializeField]
    private IPrimaryAttack primaryAttackPrefab;
    [Min(1)]
    public int primaryAttackStartFrames = 1;
    [Min(1)]
    public int primaryAttackEndFrames = 1;



    // Main function to fire the projectile towards attackDir direction starting from attacker position
    //  Pre: attackDir is the direction you attack to, attacker is the transform of the unit that's attacking
    //  Post: ALWAYS fires the attack. (Please check conditions before doing this)
    public void firePrimaryAttack(Vector3 attackDir, Transform attacker, float damage, PoisonVial parentPoison) {
        Debug.Assert(attacker != null && primaryAttackPrefab != null);

        IPrimaryAttack curBolt = Object.Instantiate(primaryAttackPrefab, attacker.position, Quaternion.identity);
        curBolt.setUp(attackDir, damage, parentPoison);
    }
}
