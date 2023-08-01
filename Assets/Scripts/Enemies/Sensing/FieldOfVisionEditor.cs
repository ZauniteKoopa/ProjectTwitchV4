using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (FieldOfVision))]
public class FieldOfVisionEditor : Editor
{
    void OnSceneGUI() {
        FieldOfVision fov = (FieldOfVision)target;
        float viewRadius = fov.transform.lossyScale.x / 2f;

        // Draw circle
        Handles.color = Color.white;
        Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.forward, 360f, viewRadius);

        // Draw angle
        Vector3 viewAngleA = fov.dirFromAngle(-fov.viewAngle / 2f, false);
        Vector3 viewAngleB = fov.dirFromAngle(fov.viewAngle / 2f, false);
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleA * viewRadius);
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleB * viewRadius);
    }
}
