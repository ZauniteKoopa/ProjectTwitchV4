using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class IStaticEffect : MonoBehaviour
{
    // Main event to listen to when effect ends
    public UnityEvent effectEndEvent;

    public bool instantiateNewObjectOnAppear = true;

    // Main function to activate static visual effect
    public abstract void executeEffect();
}
