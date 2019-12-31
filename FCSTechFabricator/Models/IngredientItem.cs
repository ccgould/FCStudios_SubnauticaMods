using Oculus.Newtonsoft.Json;

namespace FCSTechFabricator.Models
{
    /// <summary>
    /// Represents an ingredient that will be used in the BluePrint
    /// </summary>
    public class IngredientItem
    {
        /// <summary>
        /// The amount of this item to use in the formula
        /// </summary>
        [JsonProperty] public int Amount { get; set; }

        /// <summary>
        /// The TechType string of the ingredient
        /// </summary>
        [JsonProperty] public string Item { get; set; }
    }
}
