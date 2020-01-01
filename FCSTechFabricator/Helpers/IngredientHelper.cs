using FCSCommon.Extensions;
using FCSCommon.Utilities;
using FCSTechFabricator.Models;
using SMLHelper.V2.Crafting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FCSTechFabricator.Helpers
{
    public static class IngredientHelper
    {
        private static bool ValidateData(List<IngredientItem> ingredients)
        {
            if (ingredients == null)
            {
                QuickLogger.Error("Failed to get ingredients");
                return false;
            }

            foreach (var ingredient in ingredients)
            {
                if (ingredient.Item.Equals("None", StringComparison.OrdinalIgnoreCase)) continue;

                if (ingredient.Item.ToTechType().Equals(TechType.None))
                {
                    return false;
                }

                if (ingredient.Amount <= 0)
                {
                    QuickLogger.Error($"Battery Configuration Error: Valid ingredient amount cannot be less than or equal to 0 applying 1 as default");
                    ingredient.Amount = 1;
                }
            }

            return true;
        }

        private static IEnumerable<Ingredient> ConvertToIngredients(List<IngredientItem> ingredients)
        {
            foreach (var ingredient in ingredients)
            {
                yield return new Ingredient(ingredient.Item.ToTechType(), ingredient.Amount);
            }
        }

        private static TechData DefaultBlueprint()
        {
            return new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechType.Titanium, 1)
                }
            };
        }

        public static TechData GetCustomRecipe(string ClassID)
        {
            var inventoryItem = QPatch.ModConfiguration.GetTechData(ClassID);
            TechData customFabRecipe = null;

            if (ValidateData(inventoryItem))
            {
                // Create and associate recipe to the new TechType
                customFabRecipe = new TechData()
                {
                    craftAmount = 1,
                    Ingredients = ConvertToIngredients(inventoryItem).ToList()
                };
            }
            else
            {
                customFabRecipe = DefaultBlueprint();
            }

            return customFabRecipe;
        }
    }
}
