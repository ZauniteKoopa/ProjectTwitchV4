using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PoisonSideEffects/SecondaryAttackSideEffect")]
public class SecondaryAttackSideEffect : SideEffect
{
    [Header("Secondary Attack")]
    [SerializeField]
    private LobAction secondaryAttackPrefab;
    [SerializeField]
    [Min(0.01f)]
    private float secondaryAttackSpeed = 20f;
    [SerializeField]
    [Min(1)]
    private int secondaryAttackStartFrames = 15;
    [SerializeField]
    [Min(1)]
    private int secondaryAttackEndFrames = 10;
    [SerializeField]
    [Min(1)]
    private int secondaryAttackCost = 3;
    [SerializeField]
    [Min(0.1f)]
    public float secondaryAttackCooldown = 6f;
    [SerializeField]
    private bool interruptsAmbush = true;


    // Main function to fire secondary attack
    //  Pre: tgtPos is position within game, poisonVial is the poison associated with lob, and attack is the one sending out attack
    //  Post: fires secondary attack
    public override void fireSecondaryAttack(Vector3 tgtPos, Transform attacker, PoisonVial parentPoison, float attackModifier) {
        Debug.Assert(attacker != null && parentPoison != null && secondaryAttackPrefab != null);
        
        LobAction curLob = Object.Instantiate(secondaryAttackPrefab, attacker.position, Quaternion.identity);
        curLob.lob(attacker.position, tgtPos, secondaryAttackSpeed, parentPoison);
    }


    // Main function to get the primary start frames
    public override int getSecondaryAttackStartFrames() {
        return secondaryAttackStartFrames;
    }


    // Main function to get the primary start frames
    public override int getSecondaryAttackEndFrames() {
        return secondaryAttackEndFrames;
    }

    // Main function to get the secondary attack cost
    public override int getSecondaryAttackCost() {
        return secondaryAttackCost;
    }


    // Main function to get the secondary attack cooldown
    public override float getSecondaryAttackCooldown() {
        return secondaryAttackCooldown;
    }


    // Main function to check if secondary attack interrupts ambush
    public override bool caskLobInterruptsAmbush() {
        return interruptsAmbush;
    }
}
