using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticPauseEvent : IGamePauseEvent
{
    // Main function to start up event with no consideration for pause
    protected override void startEventHelper() {
        gameObject.SetActive(true);
    }


    // Main function to end event with no consideration for pause
    protected override void endEventHelper() {
        gameObject.SetActive(false);
    }
}
