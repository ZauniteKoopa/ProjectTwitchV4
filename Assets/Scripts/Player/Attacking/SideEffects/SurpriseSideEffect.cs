using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PoisonSideEffects/SurpriseSideEffect")]
public class SurpriseSideEffect : SideEffect
{
    [Header("Surprise Hitbox")]
    public DeployableHitbox surpriseDeployable = null;
}
