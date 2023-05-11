using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ingredient : MonoBehaviour
{
    public PoisonVialStat statType;
    [SerializeField]
    private GameObject controlsIndicator;
    private bool destroyed = false;
    
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


    private IEnumerator destroySequence() {
        transform.Translate(10000000f * Vector3.up);
        yield return 0;
        Object.Destroy(gameObject);
    }
}
