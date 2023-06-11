using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PoisonSideEffects/MaxStackSideEffect")]
public class MaxStackSideEffect : SideEffect
{
    [SerializeField]
    [Range(0.01f, 1f)]
    private float maxStackPoisonDecayRateModifier = 0.5f;
    private const int POISON_MAX_STACKS = 6;

    // Main function to get modified decay rate
    public override float getPoisonDecayRateModifier(int numStacks) {
        return (numStacks >= POISON_MAX_STACKS) ? maxStackPoisonDecayRateModifier : 1.0f;
    }
}
