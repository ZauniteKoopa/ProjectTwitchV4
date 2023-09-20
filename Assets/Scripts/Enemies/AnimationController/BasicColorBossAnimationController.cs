using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicColorBossAnimationController : IEnemyBossAnimationController
{
    // Color variables
    [SerializeField]
    private Color transitioningColor;
    [SerializeField]
    private Color attackColor;
    private Color normalColor;

    // Main runtime variables
    private MeshRenderer render;


    // Main event handler function to initialize
    protected override void initialize() {
        render = GetComponent<MeshRenderer>();
        normalColor = render.material.color;
    }


    // Main abstract event handler function for when boss is transitioning phases
    protected override void onPhaseTransitionStart() {
        render.material.color = transitioningColor;
    }


    // Main abstract event handler function for when boss finishes transitioning phases
    protected override void onPhaseTransitionEnd() {
        goBackToIdle();
    }


    // Main abstract event handler function for when doing a move
    protected override void onMoveStart(string moveName) {
        render.material.color = attackColor;
    }


    // Main abstract event handler function to go back to idle
    protected override void goBackToIdle() {
        render.material.color = normalColor;
    }
}
