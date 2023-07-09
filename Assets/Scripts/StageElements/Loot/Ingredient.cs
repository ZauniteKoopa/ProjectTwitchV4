using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ingredient : MonoBehaviour
{
    public PoisonVialStat statType;
    [SerializeField]
    private GameObject controlsIndicator;
    [SerializeField]
    private float solidPeriod = 3f;
    [SerializeField]
    private float fadingPeriod = 1.5f;
    private const float BLINKING_TIME = 0.1f;
    private bool destroyed = false;
    
    private void Awake() {
        StartCoroutine(lifeCycle());
    }

    
    public void glow() {
        if (!controlsIndicator.activeInHierarchy) {
            controlsIndicator.SetActive(true);
        }
    }


    public void removeGlow() {
        if (controlsIndicator.activeInHierarchy) {
            controlsIndicator.SetActive(false);
        }
    }


    public void destroyObj() {
        if (!destroyed) {
            destroyed = true;
            StartCoroutine(destroySequence());
        }
    }


    private IEnumerator lifeCycle() {
        yield return new WaitForSeconds(solidPeriod);

        WaitForSeconds waitBlink = new WaitForSeconds(BLINKING_TIME);
        float timer = 0f;

        MeshRenderer render = GetComponent<MeshRenderer>();
        Color solidColor = render.material.color;
        Color blinkColor = new Color(solidColor.r, solidColor.g, solidColor.b, 0.4f);
        bool isSolid = true;

        while (timer < fadingPeriod) {
            yield return waitBlink;
            timer += BLINKING_TIME;

            isSolid = !isSolid;
            render.material.color = (isSolid) ? solidColor : blinkColor;
        }

        destroyObj();
    }


    private IEnumerator destroySequence() {
        transform.Translate(10000000f * Vector3.up);
        yield return 0;
        Object.Destroy(gameObject);
    }
}
