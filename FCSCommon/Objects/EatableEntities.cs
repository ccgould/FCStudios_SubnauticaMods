using FCSCommon.Utilities;
using UnityEngine;

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


        public void Initialize(Pickupable food)
        {
            Create(food);
        }

        public void Initialize(Pickupable food, bool destroy)
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
