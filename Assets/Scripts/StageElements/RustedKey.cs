using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RustedKey : MonoBehaviour
{
    private bool collected = false;
    [SerializeField]
    private Light lightObject;

    // Main sensing function
    private void OnTriggerEnter(Collider collider) {
        PlayerStatus playerCollector = collider.GetComponent<PlayerStatus>();

        if (playerCollector != null && !collected) {
            collected = true;
            playerCollector.addKey();
            StartCoroutine(collectionSequence());
        }
    }


    private IEnumerator collectionSequence() {
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        GetComponent<AudioSource>().Play();
        
        if (lightObject != null) {
            lightObject.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(2f);

        Object.Destroy(gameObject);

    }
}
