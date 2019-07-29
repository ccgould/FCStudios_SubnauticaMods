using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using UnityEngine;

namespace FCS_DeepDriller.Configuration
{
    internal class PowerUnitData
    {
        [JsonProperty] internal TechType TechType { get; set; }
        [JsonProperty] internal float Charge { get; set; }
        [JsonProperty] internal string PrefabID { get; set; }
        [JsonProperty] internal float Capacity { get; set; }
        [JsonProperty] internal string Slot { get; set; }
        [JsonIgnore] internal IBattery Battery { get; set; }

        internal void Initialize(Pickupable battery, string slot = "none")
        {
            if (battery != null)
            {
                TechType = battery.GetTechType();
                PrefabID = battery.GetComponent<PrefabIdentifier>().Id;
                Battery = battery.GetComponent<IBattery>();
                Slot = slot;
            }
            else
            {
                QuickLogger.Error("Battery was null. Could not create PowercellData");
            }
        }

        internal void InitializeSolar(float charge, float capacity)
        {
            var go = new GameObject("DD_SolarBattery");
            var solarB = go.AddComponent<Battery>();
            go.AddComponent<PrefabIdentifier>();
            solarB._capacity = capacity;
            solarB._charge = charge;

            Battery = solarB.GetComponent<IBattery>();

            if (Battery == null)
            {
                QuickLogger.Error("Solar Battery is null");
            }

            QuickLogger.Debug("Created Solar Battery for Deep Driller");
        }

        internal void SaveData()
        {
            if (Battery == null)
            {
                QuickLogger.Error("Battery was null on save");
                Charge = 0f;
                Capacity = 0f;
                return;
            }

            Charge = Battery.charge;
            Capacity = Battery.capacity;
        }
    }
}
