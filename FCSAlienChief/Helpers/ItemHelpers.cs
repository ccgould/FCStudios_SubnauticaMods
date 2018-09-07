using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMLHelper.V2.Crafting;

namespace FCSAlienChief.Helpers
{
    public class ItemHelpers
    {
        public static TechData CreateRecipe(int craftAmount, List<Ingredient> ingredients)
        {
            TechData result = new TechData();

            result.craftAmount = craftAmount;

            foreach (var ingredient in ingredients)
            {
                result.Ingredients.Add(new Ingredient(ingredient.techType, ingredient.amount));
            }
            return result;
        }
    }
}
