using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Function to keep world UI sprites from facing the camera, no matter what
public class WorldUISprite : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        PlayerCameraController.faceCamera(transform);
    }
}
