using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public enum StatusEffectType {
    NONE,
    STUN,
    SPEED,
    ARMOR
}

[System.Serializable]
public class LingeringStatusEffect {
    public StatusEffectType statusEffectType;

    [Min(0.01f)]
    public float effectMagnitude = 0.5f;

    [Min(0.01f)]
    public float duration = 0.01f;
}

[CreateAssetMenu(menuName = "PoisonSideEffects/SideEffect")]
public class SideEffect : ScriptableObject
{
    // General Descriptions
    public string displayName;
    [SerializeField]
    [TextArea]
    protected string description;
    public Sprite spriteIcon;
    [SerializeField]
    private PoisonVialStat sideEffectType;

    [Header("Max stack effects")]
    public bool maxStackEffect = false;
    [SerializeField]
    [Range(0f, 1f)]
    private float defenseReduction = 0f;
    [SerializeField]
    private int additionalLoot = 0;
    public LobAction specialLoot;
    private const int MAX_POISON_STACKS = 6;
    public LingeringStatusEffect lingeringStatusEffect = null;


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
    public virtual void fireSecondaryAttack(Vector3 tgtPos, Transform attacker, PoisonVial parentPoison, float attackModifier) {
        Debug.Assert(attacker != null && parentPoison != null);
        PoisonVial.poisonVialConstants.fireDefaultSecondaryAttack(tgtPos, attacker, parentPoison, attackModifier);
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


    // Main function to check if this secondary attack has an additional secondary attack action
    public virtual bool hasAdditionalSecondaryAttackAction() {
        return false;
    }


    // Main function to check if secondary attack interrupts ambush
    public virtual bool caskLobInterruptsAmbush() {
        return true;
    }


    // Main function to display side effect info using a side effect display
    //  Pre: sideEffectDisplay != null
    public void displaySideEffectInfo(SideEffectDisplay display) {
        Debug.Assert(display != null);
        display.displayItem(
            spriteIcon,
            displayName,
            description + " <b>" + PoisonVial.poisonVialConstants.getBaseSideEffectDescription(sideEffectType) + "</b>"
        );
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


    // Main function to get the main defense reduction factor for max stack effect
    public float getDefenseReductionFactor(int numPoisonStacks) {
        return (numPoisonStacks >= MAX_POISON_STACKS) ? (1f - defenseReduction) : 1f;
    }


    // Main function to get the number of additional loot from this corpse
    public int getAdditionalLoot(int numPoisonStacks) {
        return (numPoisonStacks >= MAX_POISON_STACKS) ? additionalLoot : 0;
    }


    // Main virtual function to get the description of teh secondary attack
    public virtual string getVenomCaskDescription() {
        return PoisonVial.poisonVialConstants.defaultVenomCaskDescription;
    }
}
