using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IBlockerSensor : MonoBehaviour
{
    // Main function to check if the sensor senses something
    //  Pre: none, make sure collision layers are specified to reduce performance cost
    //  Post: return if something is touching this sensor
    public abstract bool isBlocked();
}
