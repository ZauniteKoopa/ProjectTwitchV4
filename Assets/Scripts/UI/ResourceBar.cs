using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceBar : MonoBehaviour
{
    [SerializeField]
    private Image resourceBarFill;


    // Main function to set resource bar
    public void setFill(float curValue, float fullValue) {
        resourceBarFill.fillAmount = curValue / fullValue;
    }


    // Main function to setActive
    public void setActive(bool willActive) {
        gameObject.SetActive(willActive);
    }
}
