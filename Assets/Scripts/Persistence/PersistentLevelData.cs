using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentLevelData : MonoBehaviour
{
    public static PersistentLevelData instance = null;

    // Level cleared status
    private bool onboardingCleared = false;

    // Start is called before the first frame update
    void Awake()
    {
        transform.parent = null;
        if (instance == null) {
            DontDestroyOnLoad(gameObject);
            loadSaveData();
            instance = this;

        } else if (instance != this) {
            Destroy(gameObject);
        }
    }

    // Public void method to save level state
    public void saveOnboardingClear() {
        onboardingCleared = true;
    }

    // Method to check if you can even access the level
    public bool checkOnboardingClear () {
        return onboardingCleared;
    }


    // On application quit, save data
    private void OnApplicationQuit() {
        SaveSystem.savePlayerProgress(onboardingCleared);
    }


    // On start, load save data
    private void loadSaveData() {
        PlayerProgressSaveData saveData = SaveSystem.loadPlayerProgress();
        onboardingCleared = (saveData != null) ? saveData.clearedOnboarding : false;
    }
}
