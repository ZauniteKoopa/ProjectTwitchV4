using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class HittableDelegate : UnityEvent<IHittable> {}

public abstract class IHittable : MonoBehaviour
{
    // Main event for when this object is destroyed
    public HittableDelegate destroyedEvent;
    public bool targetable = false;
    

    // Main function to hit the object with an attack
    //  Pre: none
    //  Post: activates an object on hit
    public abstract void hit();


    // Main function to reset IHittable
    //  Pre: none
    //  Post: resets anything that was hit
    public abstract void reset();
}
