using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearDeployableBolt : LinearPoisonBolt
{
    [SerializeField]
    private DeployableHitbox deployable;


    // Main protected helper function for when you actually hit something
    protected override void onProjectileCollision() {
        DeployableHitbox curDeployable = Object.Instantiate(deployable, transform.position, Quaternion.identity);
        curDeployable.deploy(poison);
    }
}
