using FCSCommon.Extensions;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using System;
using System.Collections.Generic;

namespace FCSPowerStorage.Configuration
{

    /// <summary>
    /// Class that stores custom properties from the configuration file
    /// </summary>
    public class BatteryConfiguration
    {
        /// <summary>
        /// The maximum charge of the battery
        /// </summary>
        public float Capacity { get; set; } = 2000;

        /// <summary>
        /// The charge speed of this battery
        /// </summary>
        public float ChargeSpeed { get; set; } = 0.005f;

        /// <summary>
        /// Enables/Disables the ability for the
        /// </summary>
        public bool DisableOnMinEnergyRequired { get; set; }

        /// <summary>
        /// Minimum amount of power required for charging
        /// </summary>
        public int MinEnergyRequired { get; set; } = 10;

        /// <summary>
        /// Amount to activate the power storage in case of low power
        /// </summary>
        public int AutoActivateAt { get; set; } = 1;

        /// <summary>
        /// A list of custom ingredients for the use in the BluePrint
        /// </summary>
        public List<IngredientItem> Ingredients { get; set; } = new List<IngredientItem>();

        public bool ValidateData()
        {
            foreach (var ingredient in Ingredients)
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

        public IEnumerable<Ingredient> ConvertToIngredients()
        {
            foreach (var ingredient in Ingredients)
            {
                yield return new Ingredient(ingredient.Item.ToTechType(), ingredient.Amount);
            }
        }
    }
}
