using System.Collections.Generic;

public enum Ingredient
{
    Potato,
    Tomato,
    Pepper,
    Mushroom,
    DraculaWing,
    MerewolfSteak,
    ManticoreTail
}

public enum Recipe
{
    PepperSteak,
    DragulaCacciotare,
    ManticoreRisotto
}

public static class Recipes
{
    public static readonly Dictionary<Recipe, List<Ingredient>> RecipeIngredients = new()
    {
        { Recipe.PepperSteak, new List<Ingredient> { Ingredient.Pepper, Ingredient.MerewolfSteak } },
        { Recipe.DragulaCacciotare, new List<Ingredient> { Ingredient.Pepper, Ingredient.DraculaWing } },
        { Recipe.ManticoreRisotto, new List<Ingredient> { Ingredient.Pepper, Ingredient.ManticoreTail } }
    };

    public static List<Ingredient> GetIngredientsForRecipe(Recipe recipe)
    {
        if (RecipeIngredients.TryGetValue(recipe, out List<Ingredient> ingredients))
        {
            return ingredients;
        }
        return new List<Ingredient>();
    }

    public static Ingredient GetProteinForRecipe(Recipe recipe)
    {
        List<Ingredient> ingredients = GetIngredientsForRecipe(recipe);
        foreach (Ingredient ingredient in ingredients)
        {
            if (IsProtein(ingredient))
            {
                return ingredient;
            }
        }
        return default; // or throw an exception if you prefer
    }

    public static Ingredient GetVegetableForRecipe(Recipe recipe)
    {
        List<Ingredient> ingredients = GetIngredientsForRecipe(recipe);
        foreach (Ingredient ingredient in ingredients)
        {
            if (IsVegetable(ingredient))
            {
                return ingredient;
            }
        }
        return default; // or throw an exception if you prefer
    }


    public static bool IsProtein(Ingredient ingredient)
    {
        return ingredient == Ingredient.MerewolfSteak ||
               ingredient == Ingredient.DraculaWing ||
               ingredient == Ingredient.ManticoreTail;
    }

    public static bool IsVegetable(Ingredient ingredient)
    {
        return ingredient == Ingredient.Potato ||
               ingredient == Ingredient.Tomato ||
               ingredient == Ingredient.Pepper ||
               ingredient == Ingredient.Mushroom;
    }
}