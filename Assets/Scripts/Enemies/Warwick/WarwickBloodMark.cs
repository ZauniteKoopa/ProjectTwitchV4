using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarwickBloodMark : MonoBehaviour
{
    [SerializeField]
    private Image bloodMark;
    [SerializeField]
    private Light bloodSpotLight;
    [SerializeField]
    private Color huntedColor = Color.red;
    [SerializeField]
    private Color trackingColor = Color.yellow;
    [SerializeField]
    [Min(0.01f)]
    private float markHeight = 6f;
    private Transform target = null;

    // Update is called once per frame. Always be on top of target
    void Update()
    {
        if (target != null) {
            transform.position = target.position + (markHeight * Vector3.up);
        }
    }


    // Main function to set up target
    public void setTarget(Transform tgt) {
        target = tgt;
    }


    // Main function to set up progress
    public void setTrackingProgress(float curProgress, float goal) {
        Debug.Assert(goal > 0f && curProgress >= 0f);

        float progressFill = curProgress / goal;
        bloodMark.fillAmount = progressFill;

        Color curColor = (progressFill < 0.9999f) ? trackingColor : huntedColor;
        bloodSpotLight.color = curColor;
        bloodMark.color = curColor;
    }


    // Main function to set active
    public void setActive(bool willActive) {
        gameObject.SetActive(willActive);
    }
}
