using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OneTimeTriggerBox : MonoBehaviour
{
    // Main unity event when player enters this trigger event
    public UnityEvent playerEnterTriggerEvent;


    // On trigger enter, if player enters, disable collider and invoke event
    private void OnTriggerEnter(Collider collider) {
        PlayerStatus playerStatus = collider.GetComponent<PlayerStatus>();

        if (playerStatus != null) {
            GetComponent<Collider>().enabled = false;
            playerEnterTriggerEvent.Invoke();
        }
    }
}
