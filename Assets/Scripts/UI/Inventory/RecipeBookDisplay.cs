using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecipeBookDisplay : MonoBehaviour
{
    [SerializeField]
    private SideEffectDisplay sideEffectDisplay;
    [SerializeField]
    private PoisonCompositionDisplay ingredientsDisplay;
    [SerializeField]
    private Button nextPageButton;
    [SerializeField]
    private Button prevPageButton;
    [SerializeField]
    private GameObject noRecipeLabel;

    private RecipeBook curRecipeBook;


    // Main function to display recipe book
    public void display(RecipeBook recipeBook) {
        curRecipeBook = recipeBook;
        int totalRecipesCount = recipeBook.getTotalFoundRecipes();

        nextPageButton.interactable = totalRecipesCount > 0;
        prevPageButton.interactable = totalRecipesCount > 0;

        if (totalRecipesCount <= 0) {
            sideEffectDisplay.displayEmpty();
            ingredientsDisplay.displayEmptyComp();

        } else {
            displayRecipe(recipeBook.getRecipeAtCurrentPage());

        }
    }


    // Main event handler when clicking on the next button
    public void onNextButtonPress() {
        displayRecipe(curRecipeBook.getNextRecipe());
    }


    // Main event handler when clicking on the previous button
    public void onPrevButtonPress() {
        displayRecipe(curRecipeBook.getPrevRecipe());
    }


    // Main function to display a specific side effect if found
    public void displaySpecificSideEffect(SideEffect sideEffect) {
        Recipe r = curRecipeBook.jumpToSideEffect(sideEffect);

        if (r != null) {
            displayRecipe(r);
        }
    }


    // Private helper function to display a recipe
    private void displayRecipe(Recipe recipe) {
        recipe.resultingSideEffect.displaySideEffectInfo(sideEffectDisplay);

        ingredientsDisplay.gameObject.SetActive(recipe.ingredients != null);
        noRecipeLabel.SetActive(recipe.ingredients == null);

        if (recipe.ingredients != null) {
            ingredientsDisplay.displayComposition(recipe.ingredients);
        }
    }
}
