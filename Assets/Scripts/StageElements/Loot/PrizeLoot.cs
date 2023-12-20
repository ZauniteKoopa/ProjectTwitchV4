using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PrizeLoot : MonoBehaviour
{
    [SerializeField]
    private GameObject controlsIndicator;
    [SerializeField]
    private AudioSource speaker;
    [SerializeField]
    private bool playAudioOnImmediatePickup = false;
    private bool destroyed = false;

    [SerializeField]
    [Range(-120f, 120f)]
    private float rotationSpeed = 60f;
    [SerializeField]
    private bool willRotate = true;


    private void Update() {
        if (willRotate) {
            float rotDist = rotationSpeed * Time.deltaTime;
            transform.Rotate(rotDist * Vector3.up);
        }
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


    protected void destroyObj() {
        if (!destroyed) {
            destroyed = true;
            StartCoroutine(destroySequence());
        }
    }


    private IEnumerator destroySequence() {
        transform.Translate(500f * Vector3.up);

        if (speaker != null && speaker.clip != null) {
            speaker.Play();
            yield return new WaitForSeconds(speaker.clip.length);
        } else {
            yield return 0;
        }

        Object.Destroy(gameObject);
    }


    // Public function to collect
    public void collect(PlayerStatus player, TwitchInventory inv) {
        bool destroyObject = activate(player, inv);

        if (playAudioOnImmediatePickup && speaker != null && speaker.clip != null) {
            speaker.Play();
        }

        // Destroy object
        if (destroyObject) {
            destroyObj();
        }
    }


    // Abstract function on what to do with the player if player collected
    //  Pre: player != null
    //  Post: returns a boolean that checks if the activation is successful (and thus the loot destroys itself)
    protected abstract bool activate(PlayerStatus player, TwitchInventory inv);
}
