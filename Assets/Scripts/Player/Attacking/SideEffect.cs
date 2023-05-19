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

    // Primary attack variables
    [Header("Primary Attack")]
    [SerializeField]
    private IPrimaryAttack primaryAttackPrefab;
    [SerializeField]
    [Min(1)]
    public int primaryAttackStartFrames = 1;
    [SerializeField]
    [Min(1)]
    public int primaryAttackEndFrames = 1;
    [SerializeField]
    [Min(0.1f)]
    private float primaryAttackMultiplier = 1.0f;
    [SerializeField]
    [Min(1f)]
    public float attackRange = 7f;
    [SerializeField]
    private AudioClip[] primaryAttackSoundEffects = null;

    [Header("Secondary Attack")]
    [SerializeField]
    private LobAction secondaryAttackPrefab;
    [SerializeField]
    [Min(0.01f)]
    private float secondaryAttackSpeed = 8f;
    [SerializeField]
    [Min(1)]
    public int secondaryAttackStartFrames = 1;
    [SerializeField]
    [Min(1)]
    public int secondaryAttackEndFrames = 1;
    [SerializeField]
    [Min(1)]
    public int secondaryAttackCost = 3;
    [SerializeField]
    [Min(0.1f)]
    public float secondaryAttackCooldown = 2f;


    [Header("Post Contaminate Hitbox")]
    public PostContaminateHitbox postContaminateHitbox = null;



    // Main function to fire the projectile towards attackDir direction starting from attacker position
    //  Pre: attackDir is the direction you attack to, attacker is the transform of the unit that's attacking
    //  Post: ALWAYS fires the attack. (Please check conditions before doing this)
    public void firePrimaryAttack(Vector3 attackDir, Transform attacker, float damage, PoisonVial parentPoison) {
        Debug.Assert(attacker != null && primaryAttackPrefab != null);

        IPrimaryAttack curBolt = Object.Instantiate(primaryAttackPrefab, attacker.position, Quaternion.identity);
        curBolt.setUp(attackDir, damage * primaryAttackMultiplier, parentPoison, attackRange);
    }


    // Main function to fire secondary attack
    //  Pre: tgtPos is position within game, poisonVial is the poison associated with lob, and attack is the one sending out attack
    //  Post: fires secondary attack
    public void fireSecondaryAttack(Vector3 tgtPos, Transform attacker, PoisonVial parentPoison) {
        Debug.Assert(attacker != null && parentPoison != null && secondaryAttackPrefab != null);

        LobAction curLob = Object.Instantiate(secondaryAttackPrefab, attacker.position, Quaternion.identity);
        curLob.lob(attacker.position, tgtPos, secondaryAttackSpeed, parentPoison);
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
    public AudioClip getPrimaryAttackSound() {
        Debug.Assert(primaryAttackSoundEffects != null && primaryAttackSoundEffects.Length > 0);
        return primaryAttackSoundEffects[Random.Range(0, primaryAttackSoundEffects.Length)];
    }
}
