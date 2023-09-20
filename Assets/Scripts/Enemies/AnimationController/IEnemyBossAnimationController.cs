using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class IEnemyBossAnimationController : MonoBehaviour
{
    [SerializeField]
    private BossEnemyStatus bossStatus;


    // Main function to handle on awake - connect to events
    private void Awake() {
        // Connect to boss status event
        bossStatus.enemyPhaseTransitionBeginEvent.AddListener(onPhaseTransitionStart);
        bossStatus.enemyPhaseTransitionEndEvent.AddListener(onPhaseTransitionEnd);

        initialize();
    }


    // Main event handler function to initialize
    protected abstract void initialize();


    // Main abstract event handler function for when boss is transitioning phases
    protected abstract void onPhaseTransitionStart();


    // Main abstract event handler function for when boss finishes transitioning phases
    protected abstract void onPhaseTransitionEnd();


    // Main abstract event handler function for when doing a move
    protected abstract void onMoveStart(string moveName);


    // Main abstract event handler function to go back to idle
    protected abstract void goBackToIdle();
}
