using FCS_DeepDriller.Buildable;
using FCS_DeepDriller.Enumerators;
using FCSCommon.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FCS_DeepDriller.Configuration
{
    public class DeepDrillerPowerData
    {
        public PowerUnitData Solar { get; set; } = new PowerUnitData();

        public List<PowerUnitData> Batteries { get; set; } = new List<PowerUnitData>(4);


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
            //var charge = Mathf.Clamp(GetCharge(module) - powerDraw, 0, GetCapacity(module));

            switch (module)
            {
                case DeepDrillModules.Battery:

                    foreach (var powerUnitData in Batteries)
                    {
                        if (powerUnitData.Battery.charge >= powerDraw)
                        {
                            ModifyCharge(-Mathf.Abs(powerDraw), powerUnitData);
                            //powerUnitData.Battery.charge = Mathf.Clamp(powerUnitData.Battery.charge - powerDraw, 0,
                            //    powerUnitData.Battery.capacity);
                            break;
                        }
                    }

                    break;

                case DeepDrillModules.Solar:
                    ModifyCharge(-Mathf.Abs(powerDraw), Solar);
                    //Solar.Battery.charge = charge;
                    break;
            }
        }

        public float ModifyCharge(float amount, PowerUnitData unit)
        {
            float num = 0f;

            if (unit.Battery != null)
            {
                if (amount >= 0f)
                {
                    num = Mathf.Min(amount, unit.Battery.capacity - unit.Battery.charge);
                    unit.Battery.charge += num;
                }
                else
                {
                    num = -Mathf.Min(-amount, unit.Battery.charge);
                    unit.Battery.charge += num;
                }
            }

            if (unit.Battery.charge < 1)
            {
                unit.Battery.charge = 0f;
            }

            return num;
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

        public void AddBattery(Pickupable battery, string slot)
        {
            var newBattery = new PowerUnitData();
            newBattery.Initialize(battery, slot);
            Batteries.Add(newBattery);
        }

        public void RemoveBattery(Pickupable battery)
        {
            var id = battery.GetComponent<PrefabIdentifier>().Id;

            var powercellData = Batteries.Single(x => x.PrefabID == id);

            Batteries.Remove(powercellData);
        }

        internal void SaveData()
        {
            foreach (PowerUnitData battery in Batteries)
            {
                battery.SaveData();
                QuickLogger.Debug($"Saved battery in slot ({battery.Slot})");
            }

            Solar.SaveData();
            QuickLogger.Debug($"Saved solar panel");
        }
    }
}
