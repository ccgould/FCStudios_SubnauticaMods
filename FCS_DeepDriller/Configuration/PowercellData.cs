using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;

namespace FCS_DeepDriller.Configuration
{
    internal class PowercellData
    {
        [JsonProperty] internal TechType TechType { get; set; }
        [JsonProperty] internal float Charge { get; set; }
        [JsonProperty] internal string PrefabID { get; set; }
        [JsonProperty] internal float Capacity { get; set; }
        [JsonIgnore] internal IBattery Battery { get; set; }
        [JsonProperty] internal string Slot { get; set; }

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

        internal bool IsFull()
        {
            return Charge >= Capacity;
        }
    }
}
