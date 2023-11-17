using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationalPassiveBranch : IEnemyPassiveBranch
{
    [SerializeField]
    [Range(-120f, 120f)]
    private float rotationSpeed = 10f;


    // Main function to run the branch
    public override IEnumerator execute() {
        while (true) {
            yield return 0;

            float rotDist = rotationSpeed * Time.deltaTime;
            transform.Rotate(rotDist * Vector3.up);
        }
    }

    // Main function to reset the branch when the overall tree gets overriden / switch branches
    public override void reset() {}
}
