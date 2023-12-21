using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentSaveOperations : MonoBehaviour
{
    public void saveOnboardingClear() {
        if (PersistentLevelData.instance != null) {
            PersistentLevelData.instance.saveOnboardingClear();
        }
    }
}
