using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuLoader : MonoBehaviour
{
    [SerializeField]
    private Button nonOnboardingLevelButton = null;

    // On start, set up main menu
    private void Start() {
        nonOnboardingLevelButton.interactable = PersistentLevelData.instance.checkOnboardingClear();
    }
}
