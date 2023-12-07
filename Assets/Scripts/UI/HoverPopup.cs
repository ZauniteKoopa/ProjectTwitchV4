using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class HoverPopup : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Main gameobject to set active when hovering
    [SerializeField]
    private GameObject popup;


    // On start, error check
    private void Start() {
        if (popup == null) {
            Debug.LogError("No hover popup assigned to this hover popup script", transform);
        }

        popup.SetActive(false);
    }


    // When pointer enters
    public void OnPointerEnter(PointerEventData eventData) {
        popup.SetActive(true);
    }


    // When pointer exits
    public void OnPointerExit(PointerEventData eventData) {
        popup.SetActive(false);
    }

    
    // Main function to reset hover popup (when closing the menu)
    public void reset() {
        popup.SetActive(false);
    }
}
