using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerProgressSaveData
{
    public bool clearedOnboarding;

    // Main Constructor
    public PlayerProgressSaveData(bool clearedTutorial) {
        this.clearedOnboarding = clearedTutorial;
    }
}
