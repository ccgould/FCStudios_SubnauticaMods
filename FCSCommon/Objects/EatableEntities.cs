using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using UnityEngine;

namespace FCSCommon.Objects
{
    /// <summary>
    /// Defines the Eatable object 
    /// </summary>
    internal class EatableEntities
    {
        [JsonProperty] internal TechType TechType { get; set; }

        [JsonProperty] internal string PrefabID { get; set; }

        [JsonProperty] internal string Name { get; set; }
 
        [JsonProperty] internal float WaterValue { get; set; }

        [JsonProperty] internal float FoodValue { get; set; }

        [JsonProperty] internal float KDecayRate { get; set; }

        [JsonProperty] internal bool Decomposes { get; set; }


        internal void Initialize(Pickupable food)
        {
            Create(food);
        }

        internal void Initialize(Pickupable food, bool destroy)
        {
            Create(food, destroy);
        }

        private void Create(Pickupable food, bool destroy = true)
        {
            if (food != null)
            {
                TechType = food.GetTechType();
                PrefabID = food.GetComponent<PrefabIdentifier>().Id;
                var Food = food.GetComponent<Eatable>();
                Name = food.name;
                WaterValue = Food.GetWaterValue();
                FoodValue = Food.GetFoodValue();
                KDecayRate = Food.kDecayRate;
                Decomposes = Food.decomposes;

                if (destroy)
                {
                    GameObject.Destroy(food);
                }
            }
            else
            {
                QuickLogger.Error($"Food was null. Could not create {nameof(EatableEntities)}");
            }
        }
    }
}
