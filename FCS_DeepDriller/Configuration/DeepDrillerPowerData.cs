using FCS_DeepDriller.Buildable;
using FCS_DeepDriller.Enumerators;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using UnityEngine;

namespace FCS_DeepDriller.Configuration
{
    internal class DeepDrillerPowerData
    {
        public PowerUnitData Solar { get; set; } = new PowerUnitData();
        public PowerUnitData BatterySlot1 { get; set; }
        public PowerUnitData BatterySlot2 { get; set; }
        public PowerUnitData BatterySlot3 { get; set; }
        public PowerUnitData BatterySlot4 { get; set; }
        public int ActiveSlot { get; set; }

        internal float GetCharge(DeepDrillModules module)
        {
            switch (module)
            {
                case DeepDrillModules.Battery:
                    return BatterySlot1.Battery.charge + BatterySlot2.Battery.charge + BatterySlot3.Battery.charge + BatterySlot4.Battery.charge;
                case DeepDrillModules.Solar:
                    return Solar.Battery.charge;
                default:
                    return 0;
            }
        }

        internal void SetSolarCharge(float charge)
        {
            Solar.Battery.charge = charge;
        }

        internal void RemovePower(DeepDrillModules module)
        {
            var powerDraw = FCSDeepDrillerBuildable.DeepDrillConfig.PowerDraw;
            var charge = Mathf.Clamp(GetCharge(module) - powerDraw, 0, GetCapacity(module));

            switch (module)
            {
                case DeepDrillModules.Battery:

                    switch (ActiveSlot)
                    {

                    }

                    BatterySlot1.Battery.charge = 0;
                    BatterySlot2.Battery.charge = 0;
                    BatterySlot3.Battery.charge = 0;
                    BatterySlot4.Battery.charge = 0;
                    break;

                case DeepDrillModules.Solar:
                    Solar.Battery.charge = charge;
                    break;
            }
        }

        internal void DestroyPower(DeepDrillModules module)
        {
            var powerDraw = FCSDeepDrillerBuildable.DeepDrillConfig.PowerDraw;

            switch (module)
            {
                case DeepDrillModules.Battery:
                    BatterySlot1.Battery.charge = 0;
                    BatterySlot2.Battery.charge = 0;
                    BatterySlot3.Battery.charge = 0;
                    BatterySlot4.Battery.charge = 0;
                    break;

                case DeepDrillModules.Solar:
                    Solar.Battery.charge = 0;
                    break;
            }
        }

        internal float GetCapacity(DeepDrillModules module)
        {
            QuickLogger.Debug($"Module: {module}");
            switch (module)
            {
                case DeepDrillModules.Battery:
                    return BatterySlot1.Battery.capacity + BatterySlot2.Battery.capacity + BatterySlot3.Battery.capacity + BatterySlot4.Battery.capacity;
                case DeepDrillModules.Solar:
                    return Solar.Battery.capacity;
                default:
                    return 0;
            }
        }

        internal class PowerUnitData
        {
            public TechType TechType { get; set; }
            public float Charge { get; set; }
            public string PrefabID { get; set; }
            public float Capacity { get; set; }

            [JsonIgnore]
            public IBattery Battery { get; set; }

            internal void Initialize(Pickupable battery)
            {
                if (battery != null)
                {
                    TechType = battery.GetTechType();
                    PrefabID = battery.GetComponent<PrefabIdentifier>().Id;
                    Battery = battery.GetComponent<IBattery>();
                }
                else
                {
                    QuickLogger.Error("Battery was null. Could not create PowercellData");
                }
            }

            internal void SaveData()
            {
                Charge = Battery.charge;
                Capacity = Battery.capacity;
            }
        }
    }
}
