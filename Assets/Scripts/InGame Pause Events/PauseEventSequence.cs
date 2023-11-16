using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseEventSequence : IGamePauseEvent
{
    // Main reference variables
    [SerializeField]
    private IGamePauseEvent[] pauseEvents;
    private int curEvent = 0;


    // Main function to start up event with no consideration for pause
    protected override void startEventHelper() {
        Debug.Assert(pauseEvents.Length > 0);
        Debug.Assert(pauseEvents[0] != null);

        pauseEvents[0].pauseEventFinished.AddListener(onCurrentEventFinished);
        pauseEvents[0].startEvent();
    }


    // Main private event handler for when current event has finished
    private void onCurrentEventFinished() {
        // Clean up previous pause event and increment counter
        pauseEvents[curEvent].pauseEventFinished.RemoveListener(onCurrentEventFinished);
        curEvent++;

        // If you've finished the last event, end the event. Else keep moving forward
        if (curEvent == pauseEvents.Length) {
            endEvent();
        } else {
            Debug.Assert(pauseEvents[curEvent] != null);
            pauseEvents[curEvent].pauseEventFinished.AddListener(onCurrentEventFinished);
            pauseEvents[curEvent].startEvent();
        }
    }


    // Main function to end event with no consideration for pause
    protected override void endEventHelper() {}
}
