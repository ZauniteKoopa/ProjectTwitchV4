using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    [SerializeField]
    private TMP_Text pageNumber;
    [SerializeField]
    private Image recipePageDisplay;
    [SerializeField]
    [Range(0f, 0.5f)]
    private float recipePageColorEffect = 0.2f;

    private RecipeBook curRecipeBook;
    private Color defaultRecipePageColor;
    private bool initialized = false;


    // Main function to display recipe book
    public void display(RecipeBook recipeBook) {
        if (!initialized) {
            initialized = true;
            defaultRecipePageColor = recipePageDisplay.color;
        }

        curRecipeBook = recipeBook;
        int totalRecipesCount = recipeBook.getTotalFoundRecipes();

        nextPageButton.interactable = totalRecipesCount > 0;
        prevPageButton.interactable = totalRecipesCount > 0;
        pageNumber.text = curRecipeBook.getCurrentPage() + " / " + totalRecipesCount;

        if (totalRecipesCount <= 0) {
            sideEffectDisplay.displayEmpty();
            ingredientsDisplay.displayEmptyComp();
            recipePageDisplay.color = defaultRecipePageColor;

        } else {
            displayRecipe(recipeBook.getRecipeAtCurrentPage());

        }
    }


    // Main event handler when clicking on the next button
    public void onNextButtonPress() {
        displayRecipe(curRecipeBook.getNextRecipe());
        pageNumber.text = curRecipeBook.getCurrentPage() + " / " + curRecipeBook.getTotalFoundRecipes();
    }


    // Main event handler when clicking on the previous button
    public void onPrevButtonPress() {
        displayRecipe(curRecipeBook.getPrevRecipe());
        pageNumber.text = curRecipeBook.getCurrentPage() + " / " + curRecipeBook.getTotalFoundRecipes();
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
        recipePageDisplay.color = Color.Lerp(
            defaultRecipePageColor,
            PoisonVial.poisonVialConstants.getPureColor(recipe.resultingSideEffect.getType()),
            recipePageColorEffect
        );

        if (recipe.ingredients != null) {
            ingredientsDisplay.displayComposition(recipe.ingredients);
        }
    }
}
