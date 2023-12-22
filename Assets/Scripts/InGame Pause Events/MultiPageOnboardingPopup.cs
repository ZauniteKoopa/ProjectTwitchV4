using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class OnboardingPage {
    [TextArea]
    public string instructions;
    public Sprite image1;
    public Sprite image2;
    
}

public class MultiPageOnboardingPopup : MonoBehaviour
{
    [SerializeField]
    private TMP_Text instructionText;
    [SerializeField]
    private Button closeButton;
    [SerializeField]
    private OnboardingPage[] pages;
    [SerializeField]
    private bool requiredCompletionForExit = true;
    [SerializeField]
    private AudioSource pageTurnSpeaker = null;
    private int curPage = 0;
    private int numPagesRead = 0;

    [Header("One Image Configuration")]
    [SerializeField]
    private GameObject oneImageConfiguration;
    [SerializeField]
    private Image primaryImage;


    [Header("Two Image Configuration")]
    [SerializeField]
    private GameObject twoImageConfiguration;
    [SerializeField]
    private Image dualPrimaryImage1;
    [SerializeField]
    private Image dualPrimaryImage2;


    // On awake, error check
    private void Awake() {
        if (pages.Length <= 0) {
            Debug.LogError("No pages found for onboarding popup");
        }

        if (twoImageConfiguration == null || dualPrimaryImage1 == null || dualPrimaryImage2 == null) {
            Debug.LogError("Two Image Configuration for onboarding popup not set correctly");
        }

        if (oneImageConfiguration == null || primaryImage == null) {
            Debug.LogError("One Image Configuration for onboarding popup not set correctly");
        }

        if (closeButton == null || instructionText == null) {
            Debug.LogError("General onboarding popup not set correctly");
        }

        if (requiredCompletionForExit) {
            closeButton.gameObject.SetActive(false);
        }

        setOnboardingPage(pages[0]);
    }


    // Main event handler for when you go back
    public void goBack() {
        if (curPage > 0) {
            curPage--;
            setOnboardingPage(pages[curPage]);

            if (pageTurnSpeaker != null) {
                pageTurnSpeaker.Play();
            }
        }
    }


    // Main event handler for when you go forward
    public void goForward() {
        if (curPage < pages.Length - 1) {
            curPage++;
            numPagesRead = Mathf.Max(curPage, numPagesRead);
            setOnboardingPage(pages[curPage]);

            if (numPagesRead == pages.Length - 1) {
                closeButton.gameObject.SetActive(true);
            }

            if (pageTurnSpeaker != null) {
                pageTurnSpeaker.Play();
            }
        }
    }


    // Main helper function to set up an onboarding page
    private void setOnboardingPage(OnboardingPage page) {
        if (page.image1 != null && page.image2 != null) {
            oneImageConfiguration.SetActive(false);

            twoImageConfiguration.SetActive(true);
            dualPrimaryImage1.sprite = page.image1;
            dualPrimaryImage2.sprite = page.image2;

        } else {
            twoImageConfiguration.SetActive(false);

            oneImageConfiguration.SetActive(true);
            primaryImage.sprite = page.image1;
        }

        instructionText.text = page.instructions;

    }
    
}
