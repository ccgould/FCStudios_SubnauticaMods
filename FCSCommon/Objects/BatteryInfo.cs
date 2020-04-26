using System;
using System.Collections.Generic;
using System.Text;
using Oculus.Newtonsoft.Json;
using UnityEngine;

namespace FCSCommon.Objects
{
    internal class BatteryInfo
    {
        [JsonProperty] internal TechType TechType { get; private set; }
        [JsonProperty] internal float BatteryCharge { get; private set; }
        [JsonProperty] internal float BatteryCapacity { get; private set; }
        [JsonProperty] internal string Slot { get; private set; }
        [JsonIgnore] internal IBattery Battery { get; private set; }

        public BatteryInfo()
        {

        }

        internal BatteryInfo(TechType techType, IBattery battery, string slot)
        {
            BatteryCapacity = battery.capacity;
            BatteryCharge = battery.charge;
            TechType = techType;
            Battery = battery;
            Slot = slot;
        }

        internal void TakePower(float amount)
        {
            if (Battery.charge >= amount)
            {
                Battery.charge = Mathf.Clamp(Battery.charge - amount, 0, Battery.capacity);
                BatteryCharge = Battery.charge;
            }
        }
    }
}
