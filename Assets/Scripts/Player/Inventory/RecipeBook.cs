using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


public class Recipe {
    public SideEffect resultingSideEffect;
    public Dictionary<PoisonVialStat, int> ingredients;
}

public class RecipeBook
{
    private int curPage = 0;
    private SortedDictionary<PoisonVialStat, List<Recipe>> recipes;


    // Main constructor
    public RecipeBook() {
        recipes = new SortedDictionary<PoisonVialStat, List<Recipe>>();
        recipes.Add(PoisonVialStat.POTENCY, new List<Recipe>());
        recipes.Add(PoisonVialStat.POISON, new List<Recipe>());
        recipes.Add(PoisonVialStat.REACTIVITY, new List<Recipe>());
        recipes.Add(PoisonVialStat.STICKINESS, new List<Recipe>());
    }


    // Private helper function to convert curPage to an actual recipe
    //  Pre: you have at least 1 recipe and page >= 0
    //  Post: returns the recipe that's mapped to that page
    private Recipe convertPageToRecipe(int page) {
        Debug.Assert(page >= 0 && getTotalFoundRecipes() > 0 && page < getTotalFoundRecipes());

        foreach(KeyValuePair<PoisonVialStat, List<Recipe>> entry in recipes) {
            if (page < entry.Value.Count) {
                return entry.Value[page];
            } else {
                page -= entry.Value.Count;
            }           
        }

        Debug.LogError("INVALID PAGE FOUND IN RECIPE BOOK?????");
        return null;
    }


    // Main private helper function to convert from side effect to page number
    //  Pre: s != null
    //  Post: returns a positive number representing the actual page where the side effect is if found. Else, return -1
    private int findSideEffect(SideEffect s) {
        Debug.Assert(s != null);

        List<Recipe> recipeSection = recipes[s.getType()];

        // Find the section's start
        int sectionStart = 0;
        foreach(KeyValuePair<PoisonVialStat, List<Recipe>> entry in recipes) {
            if (entry.Value != recipeSection) {
                sectionStart += entry.Value.Count;
            } else {
                break;
            }
        }

        // Find the specific page in that section
        int sectionPage = 0;
        while (sectionPage < recipeSection.Count && recipeSection[sectionPage].resultingSideEffect != s) {
            sectionPage++;
        }
        
        return (sectionPage >= recipeSection.Count) ? -1 : sectionStart + sectionPage;
    }


    // Main function to get the total number of recipes
    public int getTotalFoundRecipes() {
        int count = 0;

        foreach(KeyValuePair<PoisonVialStat, List<Recipe>> entry in recipes) {
            count += entry.Value.Count;       
        }

        return count;
    }


    // Main function to get a recipe from the current page of the book
    public Recipe getRecipeAtCurrentPage() {
        return convertPageToRecipe(curPage);
    }


    // Main function to flip the page right and then return the recipe at that page. 
    public Recipe getNextRecipe() {
        curPage = (curPage + 1) % getTotalFoundRecipes();
        return getRecipeAtCurrentPage();
    }


    // Main function to flip the page left and then return the recipe at that page. 
    public Recipe getPrevRecipe() {
        int totalRecipes = getTotalFoundRecipes();
        curPage = (curPage  - 1 + totalRecipes) % totalRecipes;
        return getRecipeAtCurrentPage();
    }


    // Main function to clear the recipe book
    public void clear() {
        curPage = 0;

        recipes[PoisonVialStat.POTENCY].Clear();
        recipes[PoisonVialStat.POISON].Clear();
        recipes[PoisonVialStat.REACTIVITY].Clear();
        recipes[PoisonVialStat.STICKINESS].Clear();
    }


    // Main function to add the side effect
    //  Pre: recipe != null and none of its contents are null
    //  Post: adds recipe to the appropriate section and sets the curPage to that section
    public void addNewRecipe(Recipe recipe) {
        Debug.Assert(recipe != null && recipe.resultingSideEffect != null);

        int sectionIndex = 0;
        List<Recipe> recipeSection = recipes[recipe.resultingSideEffect.getType()];

        // See if you can find another copy of this recipe is found
        while (sectionIndex < recipeSection.Count && recipeSection[sectionIndex].resultingSideEffect != recipe.resultingSideEffect) {
            sectionIndex++;
        }

        // If you can't find a copy of this, add it. Else, replace it with the new recipe ONLY IFF newRecipe.ingredients != null
        bool updated = false;

        if (sectionIndex >= recipeSection.Count) {
            recipeSection.Add(recipe);
            updated = true;

        } else if (recipe.ingredients != null && recipeSection[sectionIndex].ingredients == null) {
            recipeSection[sectionIndex] = recipe;
            updated = true;
        }

        // Set curPage to the new side effect IFF something updated
        if (updated) {
            curPage = findSideEffect(recipe.resultingSideEffect);
        }
    }


    // Main function to jump to a side effect
    //  Post: returns null if side effect not found. else return the recipe at current page
    public Recipe jumpToSideEffect(SideEffect s) {
        Debug.Assert(s != null);

        int tempPage = findSideEffect(s);
        if (tempPage < 0) {
            return null;
        }

        curPage = tempPage;
        return getRecipeAtCurrentPage();
    }
}
