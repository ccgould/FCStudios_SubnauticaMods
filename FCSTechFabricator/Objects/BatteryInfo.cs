using Oculus.Newtonsoft.Json;
using UnityEngine;

namespace FCSTechFabricator.Objects
{
    public class BatteryInfo
    {
        [JsonProperty] public TechType TechType { get; private set; }
        [JsonProperty] public float BatteryCharge { get; private set; }
        [JsonProperty] public float BatteryCapacity { get; private set; }
        [JsonProperty] public string Slot { get; private set; }
        [JsonIgnore] public IBattery Battery { get; private set; }

        public BatteryInfo()
        {

        }

        public BatteryInfo(TechType techType, IBattery battery, string slot)
        {
            BatteryCapacity = battery.capacity;
            BatteryCharge = battery.charge;
            TechType = techType;
            Battery = battery;
            Slot = slot;
        }

        public void TakePower(float amount)
        {
            if (Battery.charge >= amount)
            {
                Battery.charge = Mathf.Clamp(Battery.charge - amount, 0, Battery.capacity);
                BatteryCharge = Battery.charge;
            }
        }
    }
}
