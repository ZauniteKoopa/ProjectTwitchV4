using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class DamagePopup : MonoBehaviour
{
    private float curDamage = 0.0f;
    private Transform curUnit = null;
    private readonly object popupLock = new object();

    private TextPopup popup = null;
    private bool launched = false;


    // On launch, activate the popup with the current damage
    //  Pre: startDamage can be any number, affectedUnit should be non-null (where the popup will be placed)
    //  Post: text popup will be activated with the starting damage 
    public void launch(float startDamage, Transform affectedUnit) {
        Debug.Assert(affectedUnit != null);

        if (!launched) {
            lock (popupLock) {
                curDamage = startDamage;
                curUnit = affectedUnit;
                launched = true;

                popup = GetComponent<TextPopup>();
                float displayedDamage = Mathf.Round(curDamage * 10f) / 10f;
                popup.SetUpPopup("" + displayedDamage, affectedUnit);
            }
        }
        
    }


    // Main function to increment the current damage popup
    //  Pre: damageDelta is how much you want to change the current damage, positive you add, negative you subtract, must already be launched, popup != null
    //  Post: the text popup will change the value accordingly
    public void updateDamage(float damageDelta) {
        lock (popupLock) {
            Debug.Assert(launched && popup != null);

            curDamage += damageDelta;
            float displayedDamage = Mathf.Round(curDamage * 10f) / 10f;
            popup.SetUpPopup("" + displayedDamage, curUnit);
        }
    }

}
