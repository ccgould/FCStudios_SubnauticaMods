using FCS_DeepDriller.Buildable;
using FCS_DeepDriller.Enumerators;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FCS_DeepDriller.Configuration
{
    internal class DeepDrillerPowerData
    {
        public PowerUnitData Solar { get; set; } = new PowerUnitData();

        public List<PowerUnitData> Batteries { get; set; } = new List<PowerUnitData>(4);

        public int ActiveSlot { get; set; }

        internal float GetCharge(DeepDrillModules module)
        {
            switch (module)
            {
                case DeepDrillModules.Battery:

                    float count = 0;

                    for (int i = 0; i < Batteries.Count; i++)
                    {
                        count += Batteries[i].Battery.charge;
                    }

                    return count;
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

        internal void ConsumePower(DeepDrillModules module)
        {
            var powerDraw = FCSDeepDrillerBuildable.DeepDrillConfig.PowerDraw;
            var charge = Mathf.Clamp(GetCharge(module) - powerDraw, 0, GetCapacity(module));

            switch (module)
            {
                case DeepDrillModules.Battery:

                    foreach (var powerUnitData in Batteries)
                    {
                        if (powerUnitData.Battery.charge >= powerDraw)
                        {
                            powerUnitData.Battery.charge = Mathf.Clamp(powerUnitData.Battery.charge - powerDraw, 0,
                                powerUnitData.Battery.capacity);
                            break;
                        }
                    }

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
                    for (int i = 0; i < Batteries.Count; i++)
                    {
                        Batteries[i].Battery.charge = 0;
                    }
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

                    float count = 0;

                    for (int i = 0; i < Batteries.Count; i++)
                    {
                        count += Batteries[i].Battery.capacity;
                    }

                    return count;

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

        public void AddBattery(Pickupable battery)
        {
            var newBattery = new PowerUnitData();
            newBattery.Initialize(battery);
            Batteries.Add(newBattery);
        }

        public void RemoveBattery(Pickupable battery)
        {
            var id = battery.GetComponent<PrefabIdentifier>().Id;

            var powercellData = Batteries.Single(x => x.PrefabID == id);

            Batteries.Remove(powercellData);
        }
    }
}
