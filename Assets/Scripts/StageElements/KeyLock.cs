using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyLock : AbstractLock
{
    private bool locked = true;
    [SerializeField]
    [Min(1)]
    private int keysRequired = 1;
    [SerializeField]
    private MeshRenderer[] lockIndicators;
    [SerializeField]
    private Color openColor = Color.black;
    [SerializeField]
    private Color closedColor = Color.black;
    [SerializeField]
    private Color unlockedColor = Color.green;


    // On awake, initialize: set all locks to black and listen to user events
    private void Awake() {
        onPlayerKeyCountChange(0);
        PlayerStatus player = FindObjectOfType<PlayerStatus>();
        if (player != null) {
            player.keyCountEvent.AddListener(onPlayerKeyCountChange);
        }
    }
    
    
    // Main function to reset the locks
    //  Pre: none
    //  Post: locks will be reset in their original positions in their deactivated state
    public override void reset() {
        locked = true;
    }


    // Abstract function to handle the event for when the lock is unlocked
    //  Pre: unlockEvent has been triggered
    //  Post: lock components are changed to reflect their unlocked state
    protected override void onUnlock() {
        Debug.Assert(locked);

        locked = false;

        for (int l = 0; l < lockIndicators.Length; l++) {
            lockIndicators[l].material.color = unlockedColor;
        }
    }


    // Main trigger box function
    private void OnTriggerEnter(Collider collider) {
        if (locked) {
            PlayerStatus playerStatus = collider.GetComponent<PlayerStatus>();

            if (playerStatus != null && playerStatus.takeKey(keysRequired)) {
                unlock();
            }
        }
    }


    // Main function to render the locks given how many keys the players have
    private void onPlayerKeyCountChange(int numKeys) {
        if (locked) {
            for (int l = 0; l < lockIndicators.Length; l++) {
                lockIndicators[l].material.color = (l < numKeys) ? openColor : closedColor;
            }
        }
    }
}
