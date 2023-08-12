using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecipeDisplay : MonoBehaviour
{
    [SerializeField]
    private SideEffectDisplay sideEffectDisplay;
    [SerializeField]
    private PoisonCompositionDisplay ingredientsDisplay;
    [SerializeField]
    private GameObject noRecipeLabel;


    // Main function to display main recipe
    public void displayRecipe(Recipe recipe) {
        gameObject.SetActive(true);
        recipe.resultingSideEffect.displaySideEffectInfo(sideEffectDisplay);
        ingredientsDisplay.gameObject.SetActive(recipe.ingredients != null);

        if (noRecipeLabel != null) {
            noRecipeLabel.SetActive(recipe.ingredients == null);
        }

        if (recipe.ingredients != null) {
            ingredientsDisplay.displayComposition(recipe.ingredients);
        }
    }


    // Main function to display empty
    public void displayEmpty() {
        gameObject.SetActive(false);
    }
}
