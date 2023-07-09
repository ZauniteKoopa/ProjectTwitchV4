using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarterPackChest : ItemChest
{
    [SerializeField]
    private LobAction recipeBookPage;
    [SerializeField]
    private LootTable ingredientDrops;
    [SerializeField]
    [Range(1, 5)]
    private int numIngredientDrops = 2;


    // On awake, set up item chest
    private void Awake() {
        addItem(recipeBookPage);
        
        for(int i = 0; i < numIngredientDrops; i++) {
            addItem(ingredientDrops.getLootDrop());
        }
    }
}
