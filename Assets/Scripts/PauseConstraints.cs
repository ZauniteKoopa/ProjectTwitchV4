using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PauseConstraints
{
    private static TwitchInventory inventoryModule;
    private static int numPausers = 0;

    // Main function to connect twitch inventory
    public static void setInventoryModule(TwitchInventory inv) {
        Debug.Assert(inv != null);
        inventoryModule = inv;
    }


    // Main function to check if you're currently paused
    public static bool isPaused() {
        return inventoryModule.isInventoryMenuOpen() || numPausers > 0;
    }


    // Main function to externally pause the game
    public static void externalPause(bool pauseState) {
        numPausers += (pauseState) ? 1 : -1;
    }


    // Main sequence function to fun in realtime 
    public static IEnumerator waitForSecondsRealtimeWithPause(float numSecs) {
        float timer = 0f;

        while (timer < numSecs) {
            yield return 0;
            timer += Time.unscaledDeltaTime;

            while (isPaused()) {
                yield return 0;
            }
        }       
    }


    // Main function to wait for number of frams
    public static IEnumerator waitForFramesRealtimeWithPause(int numFrames) {
        for (int f = 0; f < numFrames; f++) {
            yield return 0;

            while (isPaused()) {
                yield return 0;
            }
        }
    }
}
