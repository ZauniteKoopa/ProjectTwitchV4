using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PauseConstraints
{
    private static TwitchInventory inventoryModule;
    private static readonly float FRAME_TIME_DELTA = 0.01f;
    private static bool pausedByExternalSource = false;

    // Main function to connect twitch inventory
    public static void setInventoryModule(TwitchInventory inv) {
        Debug.Assert(inv != null);
        inventoryModule = inv;
    }


    // Main function to check if you're currently paused
    public static bool isPaused() {
        return inventoryModule.isInventoryMenuOpen() || pausedByExternalSource;
    }


    // Main function to externally pause the game
    public static void externalPause(bool pauseState) {
        pausedByExternalSource = pauseState;
    }


    // Main sequence function to fun in realtime 
    public static IEnumerator waitForSecondsRealtimeWithPause(float numSecs) {
        WaitForSecondsRealtime waitFrame = new WaitForSecondsRealtime(FRAME_TIME_DELTA);

        yield return new WaitForSecondsRealtime(numSecs);
        while (isPaused()) {
            yield return waitFrame;
        }
    }
}