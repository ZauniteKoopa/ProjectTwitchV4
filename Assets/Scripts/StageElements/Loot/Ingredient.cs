using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ingredient : PrizeLoot
{
    [SerializeField]
    private PoisonVialStat statType;
    [SerializeField]
    private float solidPeriod = 3f;
    [SerializeField]
    private float fadingPeriod = 1.5f;
    private const float BLINKING_TIME = 0.1f;

    
    private void Awake() {
        StartCoroutine(lifeCycle());
    }


    private IEnumerator lifeCycle() {
        yield return new WaitForSeconds(solidPeriod);

        WaitForSeconds waitBlink = new WaitForSeconds(BLINKING_TIME);
        float timer = 0f;

        MeshRenderer render = GetComponent<MeshRenderer>();
        Color solidColor = render.material.color;
        Color blinkColor = new Color(solidColor.r, solidColor.g, solidColor.b, 0.4f);
        bool isSolid = true;

        while (timer < fadingPeriod) {
            yield return waitBlink;
            timer += BLINKING_TIME;

            isSolid = !isSolid;
            render.material.color = (isSolid) ? solidColor : blinkColor;
        }

        destroyObj();
    }


    // Abstract function on what to do with the player if player collected
    //  Pre: player != null
    protected override bool activate(PlayerStatus player, TwitchInventory inv) {
        return inv.addIngredient(statType);
    }
}
