using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using TMPro;

public class EnemyStatusUI : MonoBehaviour
{
    [SerializeField]
    private Image healthBar;
    [SerializeField]
    private Image poisonHalo;
    [SerializeField]
    private TMP_Text poisonHaloCount;


    // Main function to update health bar
    //  Pre: maxHealth >= curHealth
    //  Post: healthBar is updated
    public void updateHealthBar(float curHealth, float maxHealth) {
        Debug.Assert(curHealth <= maxHealth);

        healthBar.fillAmount = curHealth / maxHealth;
    }


    // Main function to update poison halo
    //  Pre: poisonStacks >= 0
    //  Post: update poison stacks display
    public void updatePoisonHalo(int poisonStacks) {
        Debug.Assert(poisonStacks >= 0);

        bool poisoned = poisonStacks > 0;
        poisonHalo.gameObject.SetActive(poisoned);

        poisonHaloCount.text = "" + poisonStacks;
    }
}
