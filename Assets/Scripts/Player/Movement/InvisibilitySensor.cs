using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvisibilitySensor : EnemyStatusSensor
{
    private MeshRenderer render = null;

    // Start is called before the first frame update
    void Awake()
    {
        render = GetComponent<MeshRenderer>();
    }

    // Main function to display the sensor
    public void displaySensor(bool displayed) {
        if (render != null) {
            render.enabled = displayed;
        }
    }
}
