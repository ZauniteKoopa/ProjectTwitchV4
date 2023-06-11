using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PoisonSideEffects/PrimaryAttackSideEffect")]
public class PrimaryAttackSideEffect : SideEffect
{
    [Header("Primary Attack")]
    [SerializeField]
    private IPrimaryAttack primaryAttackPrefab;
    [SerializeField]
    [Min(1)]
    private int primaryAttackStartFrames = 10;
    [SerializeField]
    [Min(1)]
    private int primaryAttackEndFrames = 20;
    [SerializeField]
    [Min(0.1f)]
    private float primaryAttackMultiplier = 1.0f;
    [SerializeField]
    [Min(1f)]
    private float attackRange = 6f;
    [SerializeField]
    private AudioClip[] primaryAttackSoundEffects = null;


    // Main function to fire the projectile towards attackDir direction starting from attacker position
    //  Pre: attackDir is the direction you attack to, attacker is the transform of the unit that's attacking
    //  Post: ALWAYS fires the attack. (Please check conditions before doing this)
    public override void firePrimaryAttack(Vector3 attackDir, Transform attacker, float damage, PoisonVial parentPoison) {
        Debug.Assert(attacker != null && primaryAttackPrefab != null);

        IPrimaryAttack curBolt = Object.Instantiate(primaryAttackPrefab, attacker.position, Quaternion.identity);
        curBolt.setUp(attackDir, damage * primaryAttackMultiplier, parentPoison, attackRange);
    }


    // Main function to get the primary start frames
    public override int getPrimaryAttackStartFrames() {
        return primaryAttackStartFrames;
    }


    // Main function to get the primary start frames
    public override int getPrimaryAttackEndFrames() {
        return primaryAttackEndFrames;
    }


    // Main function to get attack range
    public override float getAttackRange() {
        return attackRange;
    }


    // Main function to get a random primary attack sound effect clip
    public override AudioClip getPrimaryAttackSound() {
        Debug.Assert(primaryAttackSoundEffects != null && primaryAttackSoundEffects.Length > 0);
        return primaryAttackSoundEffects[Random.Range(0, primaryAttackSoundEffects.Length)];
    }
}
