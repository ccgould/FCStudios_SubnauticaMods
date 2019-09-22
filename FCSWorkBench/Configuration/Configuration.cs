using FCSTechFabricator.Models;
using Oculus.Newtonsoft.Json;
using Oculus.Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace FCSTechFabricator.Configuration
{
    internal class Configuration
    {
        [JsonProperty] internal List<ConfigItem> Blueprints { get; set; } = new List<ConfigItem>();

        public List<IngredientItem> GetData(string classId)
        {
            foreach (ConfigItem configItem in Blueprints)
            {
                if (configItem.ItemName.Equals(classId))
                {
                    return configItem.IngredientItem;
                }
            }

            return null;
        }
    }

    internal class ConfigItem
    {
        [JsonProperty] internal string ItemName { get; set; }
        [JsonProperty] internal List<IngredientItem> IngredientItem { get; set; } = new List<IngredientItem>();
    }
}
