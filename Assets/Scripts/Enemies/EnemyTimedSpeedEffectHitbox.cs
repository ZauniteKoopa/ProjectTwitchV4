using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTimedSpeedEffectHitbox : EnemyHitbox
{
    private float speedEffectDuration;
    private float speedEffectFactor;


    // Main function to apply hitbox effect
    protected override void applyHitboxEffect(float damage, PlayerStatus player) {
        player.setTimedSpeedModifier(speedEffectFactor, speedEffectDuration);
    }


    // Main function to setup
    public void setUp(float effectFactor, float effectDuration) {
        Debug.Assert(effectFactor > 0f && effectDuration > 0f);

        speedEffectDuration = effectDuration;
        speedEffectFactor = effectFactor;
    }
}
