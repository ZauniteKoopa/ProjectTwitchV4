using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAutoLock : AbstractLock
{
    // Abstract function to handle the event for when the lock is unlocked
    //  Pre: unlockEvent has been triggered
    //  Post: lock components are changed to reflect their unlocked state
    protected override void onUnlock() {}


    // Main function to reset the locks
    //  Pre: none
    //  Post: locks will be reset in their original positions in their deactivated state
    public override void reset() {}
}
