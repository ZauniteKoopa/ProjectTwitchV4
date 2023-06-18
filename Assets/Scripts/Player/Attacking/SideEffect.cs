using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu(menuName = "PoisonSideEffects/SideEffect")]
public class SideEffect : ScriptableObject
{
    // General Descriptions
    public string displayName;
    [SerializeField]
    [TextArea]
    private string description;
    public Sprite spriteIcon;
    [SerializeField]
    private PoisonVialStat sideEffectType;


    [Header("Post Contaminate Hitbox")]
    public PostContaminateHitbox postContaminateHitbox = null;



    // Main function to fire the projectile towards attackDir direction starting from attacker position
    //  Pre: attackDir is the direction you attack to, attacker is the transform of the unit that's attacking
    //  Post: ALWAYS fires the attack. (Please check conditions before doing this)
    public virtual void firePrimaryAttack(Vector3 attackDir, Transform attacker, float damage, PoisonVial parentPoison) {
        Debug.Assert(attacker != null);
        PoisonVial.poisonVialConstants.fireDefaultPrimaryPoisonAttack(attackDir, attacker, damage, parentPoison);
    }


    // Main function to get the primary start frames
    public virtual int getPrimaryAttackStartFrames() {
        return PoisonVial.poisonVialConstants.primaryAttackStartFrames;
    }


    // Main function to get the primary start frames
    public virtual int getPrimaryAttackEndFrames() {
        return PoisonVial.poisonVialConstants.primaryAttackEndFrames;
    }


    // Main function to get attack range
    public virtual float getAttackRange() {
        return PoisonVial.poisonVialConstants.attackRange;
    }


    // Main function to fire secondary attack
    //  Pre: tgtPos is position within game, poisonVial is the poison associated with lob, and attack is the one sending out attack
    //  Post: fires secondary attack
    public virtual void fireSecondaryAttack(Vector3 tgtPos, Transform attacker, PoisonVial parentPoison) {
        Debug.Assert(attacker != null && parentPoison != null);
        PoisonVial.poisonVialConstants.fireDefaultSecondaryAttack(tgtPos, attacker, parentPoison);
    }


    // Main function to get the primary start frames
    public virtual int getSecondaryAttackStartFrames() {
        return PoisonVial.poisonVialConstants.secondaryAttackStartFrames;
    }


    // Main function to get the secondary end frames
    public virtual int getSecondaryAttackEndFrames() {
        return PoisonVial.poisonVialConstants.secondaryAttackEndFrames;
    }


    // Main function to get the secondary attack cost
    public virtual int getSecondaryAttackCost() {
        return PoisonVial.poisonVialConstants.secondaryAttackCost;
    }


    // Main function to get the secondary attack cooldown
    public virtual float getSecondaryAttackCooldown() {
        return PoisonVial.poisonVialConstants.secondaryAttackCooldown;
    }


    // Main function to display side effect info using a side effect display
    //  Pre: sideEffectDisplay != null
    public void displaySideEffectInfo(SideEffectDisplay display) {
        Debug.Assert(display != null);
        display.displayItem(spriteIcon, displayName, description);
    }


    // Main function to access the side effect's type
    public PoisonVialStat getType() {
        return sideEffectType;
    }


    // Main function to get a random primary attack sound effect clip
    public virtual AudioClip getPrimaryAttackSound() {
        return PoisonVial.poisonVialConstants.getDefaultPrimaryAttackSound();
    }


    // Main function to get modified decay rate
    public virtual float getPoisonDecayRateModifier(int numStacks) {
        return 1.0f;
    }


    // Main function to get the primary attack animation
    public virtual PrimaryAttackAnimation getPrimaryAttackAnimation() {
        return PrimaryAttackAnimation.NORMAL;
    }
}
