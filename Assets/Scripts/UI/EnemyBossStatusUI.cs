using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyBossStatusUI : EnemyStatusUI
{
    // Main variables for phase health bars
    [SerializeField]
    private Image[] phaseBars;
    [SerializeField]
    private Image screenHealthBar;


    // Main function to update health bar
    //  Pre: maxHealth >= curHealth
    //  Post: healthBar is updated
    public override void updateHealthBar(float curHealth, float maxHealth) {
        Debug.Assert(curHealth <= maxHealth);

        base.updateHealthBar(curHealth, maxHealth);
        screenHealthBar.fillAmount = curHealth / maxHealth;
    }


    // Main function to update phase bar
    public void updatePhaseBar(float phaseHealthThreshold) {
        foreach (Image phaseBar in phaseBars) {
            phaseBar.fillAmount = phaseHealthThreshold;
        }
    }
}
