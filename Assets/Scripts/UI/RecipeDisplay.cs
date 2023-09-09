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
    [SerializeField]
    private Image recipePageDisplay;
    [SerializeField]
    [Range(0f, 0.5f)]
    private float recipePageColorEffect = 0.2f;

    private Color defaultRecipePageColor;
    private bool initialized = false;


    // Main function to display main recipe
    public void displayRecipe(Recipe recipe) {
        if (!initialized) {
            initialized = true;
            defaultRecipePageColor = recipePageDisplay.color;
        }

        gameObject.SetActive(true);
        recipe.resultingSideEffect.displaySideEffectInfo(sideEffectDisplay);
        ingredientsDisplay.gameObject.SetActive(recipe.ingredients != null);

        if (noRecipeLabel != null) {
            noRecipeLabel.SetActive(recipe.ingredients == null);
        }

        if (recipe.ingredients != null) {
            ingredientsDisplay.displayComposition(recipe.ingredients);
        }

        recipePageDisplay.color = Color.Lerp(
            defaultRecipePageColor,
            PoisonVial.poisonVialConstants.getPureColor(recipe.resultingSideEffect.getType(), false),
            recipePageColorEffect
        );
    }


    // Main function to display empty
    public void displayEmpty() {
        gameObject.SetActive(false);
    }
}
