using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Model
{
    /// <summary>
    /// Defines the Eatable object 
    /// </summary>
    public class EatableEntities
    {
        public TechType TechType { get; set; }
        public string Name { get; set; }
        private float WaterValue { get; set; }
        private float FoodValue { get; set; }
        private float KDecayRate { get; set; }
        public bool Decomposes { get; set; }
        public float TimeDecayStart { get; set; }
        private bool _decayPaused;
        private float _currentWaterValue;
        private float _currentFoodValue;
        public float TimeDecayPause { get; set; }

        public void Initialize(Pickupable food, bool destroy = true)
        {
            Create(food, destroy);
        }

        private void Create(Pickupable pickupable, bool destroy)
        {
            if (pickupable != null)
            {
                TechType = pickupable.GetTechType();
                var food = pickupable.GetComponent<Eatable>();
                Name = pickupable.name;
                WaterValue = food.waterValue;
                FoodValue = food.foodValue;
                KDecayRate = food.kDecayRate;
                Decomposes = food.decomposes;
                TimeDecayStart = food.timeDecayStart;
                _currentFoodValue = FoodValue;
                _currentWaterValue = WaterValue;

                if (destroy)
                {
                    Object.Destroy(pickupable);
                }
            }
            else
            {
                QuickLogger.Error($"Food was null. Could not create {nameof(EatableEntities)}");
            }
        }

        public void IterateRotten()
        {
            if (!IsRotten())
            {
                return;
            }
        }
        
        public bool IsRotten()
        {
            return GetFoodValue() < 0f && GetWaterValue() < 0f;
        }
        
        public float GetFoodValue()
        {
            var result = FoodValue;
            if (Decomposes)
            {
                result =  Mathf.Max(FoodValue - GetDecayValue(), -25f);
                _currentFoodValue = result;
            }
            return result;
        }

        public float GetWaterValue()
        {
            var result = WaterValue;
            if (Decomposes)
            {
                result = Mathf.Max(WaterValue - GetDecayValue(), -25f);
                _currentWaterValue = result;
            }
            return result;
        }

        private float GetDecayValue()
        {
            if (!Decomposes)
            {
                return 0f;
            }
            
            float num = (!_decayPaused) ? DayNightCycle.main.timePassedAsFloat : TimeDecayPause;
            return (num - TimeDecayStart) * KDecayRate;
        }

        public void UnpauseDecay()
        {
            if (!_decayPaused)
            {
                return;
            }
            _decayPaused = false;
            TimeDecayStart += DayNightCycle.main.timePassedAsFloat - TimeDecayPause;
        }

        public void PauseDecay()
        {
            if (_decayPaused)
            {
                return;
            }
            _decayPaused = true;
            TimeDecayPause = DayNightCycle.main.timePassedAsFloat;
        }
    }
}
