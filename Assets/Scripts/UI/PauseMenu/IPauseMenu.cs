using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public abstract class IPauseMenu : MonoBehaviour
{
    public UnityEvent pausedEvent;
    public UnityEvent unpausedEvent;


    // Main event handler function for when the pause button is pressed
    //  Pre: none
    //  Post: Handles for when either pause key is pressed, or when unpause UI button pressed
    public abstract void pause();


    // Main event handler function for exiting the game
    //  Pre: none
    //  Post: will exit the application IFF application is paused on this menu
    public abstract void onExitApplication();


    // Accessor function to check if you're in the pause state
    //  Pre: None
    //  Post: returns whether you're in pause state
    public abstract bool inPauseState();


    // Main event handler function for pause button input
    public void onPauseButtonPress(InputAction.CallbackContext value) {
        if (value.started) {
            pause();
        }
    }

}
