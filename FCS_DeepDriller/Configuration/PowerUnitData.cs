using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using UnityEngine;

namespace FCS_DeepDriller.Configuration
{
    public class PowerUnitData
    {
        public TechType TechType { get; set; }
        public float Charge { get; set; }
        public string PrefabID { get; set; }
        public float Capacity { get; set; }
        public string Slot { get; set; }

        [JsonIgnore]
        public IBattery Battery { get; set; }

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

            solarB._capacity = capacity;
            solarB._charge = charge;

            Battery = solarB.GetComponent<IBattery>();

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
