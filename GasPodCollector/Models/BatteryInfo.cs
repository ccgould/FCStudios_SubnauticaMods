using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oculus.Newtonsoft.Json;
using UnityEngine;

namespace GasPodCollector.Models
{
    internal class BatteryInfo
    {
        [JsonProperty] internal TechType TechType { get; private set; }
        [JsonProperty] internal float BatteryCharge { get; private set; }
        [JsonProperty] internal float BatteryCapacity { get; private set; }
        [JsonIgnore] internal IBattery Battery { get; private set; }

        internal BatteryInfo(TechType techType, IBattery battery)
        {
            BatteryCapacity = battery.capacity;
            BatteryCharge = battery.charge;
            TechType = techType;
            Battery = battery;
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
