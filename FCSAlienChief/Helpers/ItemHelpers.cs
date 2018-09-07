using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;

namespace FCSAlienChief.Helpers
{
    /// <summary>
    /// A helper class that deals with Items
    /// </summary>
    public class ItemHelpers
    {
        private static TechType _customTechType;

        /// <summary>
        /// Creates the recipe for the GameObject
        /// </summary>
        /// <param name="craftAmount">The Amount of items to return</param>
        /// <param name="ingredients">The List of ingredients</param>
        /// <param name="linkedItemsList"> The list of Linkitems</param>
        /// <returns></returns>
        public static TechData CreateRecipe(int craftAmount, List<Ingredient> ingredients, List<string> linkedItemsList)
        {
            var result = new TechData();

            var linkedItems = new List<TechType>();

            result.craftAmount = craftAmount;

            foreach (var ingredient in ingredients)
            {
                result.Ingredients.Add(new Ingredient(ingredient.techType, ingredient.amount));
            }

            foreach (var linkedItem in linkedItemsList)
            {
                if (!Enum.IsDefined(typeof(TechType), linkedItem))
                {
                    if (TechTypeHandler.TryGetModdedTechType(linkedItem, out TechType customTechType))
                    {
                        _customTechType = customTechType;
                        linkedItems.Add(_customTechType);
                    }
                    else
                    {
                        Log.Warning(linkedItem, $"{linkedItem} must be a TechType");
                        Log.Info(linkedItem, $"{linkedItem} was set to a dummy value and disabled");
                    }
                }
                else
                {
                    linkedItems.Add((TechType)Enum.Parse(typeof(TechType), linkedItem));
                }
            }

            result.LinkedItems = linkedItems;
            return result;
        }
    }
}
