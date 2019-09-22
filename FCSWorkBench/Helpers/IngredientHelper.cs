using FCSCommon.Extensions;
using FCSCommon.Utilities;
using FCSTechFabricator.Models;
using SMLHelper.V2.Crafting;
using System;
using System.Collections.Generic;

namespace FCSTechFabricator.Helpers
{
    internal static class IngredientHelper
    {
        internal static bool ValidateData(List<IngredientItem> ingredients)
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

        internal static IEnumerable<Ingredient> ConvertToIngredients(List<IngredientItem> ingredients)
        {
            foreach (var ingredient in ingredients)
            {
                yield return new Ingredient(ingredient.Item.ToTechType(), ingredient.Amount);
            }
        }
    }
}
