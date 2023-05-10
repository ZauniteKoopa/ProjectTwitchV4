using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ingredient : MonoBehaviour
{
    public PoisonVialStat statType;
    
    public void glow() {
        Debug.Log(gameObject.name + " NOW GLOWS");
    }


    public void destroyObj() {
        Object.Destroy(gameObject);
    }
}
