using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;

namespace FCSCommon.Objects
{
    /// <summary>
    /// Defines the Eatable object 
    /// </summary>
    public class EatableEntities
    {
        public TechType TechType { get; set; }

        public string PrefabID { get; set; }

        public string Name { get; set; }

        public float WaterValue { get; set; }

        public float FoodValue { get; set; }

        public float KDecayRate { get; set; }

        public bool Decomposes { get; set; }

        [JsonIgnore]
        public Eatable Food { get; set; }

        public void Initialize(Pickupable food)
        {
            if (food != null)
            {
                TechType = food.GetTechType();
                PrefabID = food.GetComponent<PrefabIdentifier>().Id;
                Food = food.GetComponent<Eatable>();
                Name = food.name;
                KDecayRate = Food.kDecayRate;
                Decomposes = Food.decomposes;
            }
            else
            {
                QuickLogger.Error($"Food was null. Could not create {nameof(EatableEntities)}");
            }
        }

        public void SaveData()
        {
            WaterValue = Food.GetWaterValue();
            FoodValue = Food.GetFoodValue();
        }
    }
}
