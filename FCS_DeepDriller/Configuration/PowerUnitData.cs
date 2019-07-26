using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;

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

        internal void SaveData()
        {
            Charge = Battery.charge;
            Capacity = Battery.capacity;
        }
    }
}
