using FCSCommon.Objects;
using Oculus.Newtonsoft.Json;

namespace FCSTechFabricator.Objects
{
    public class PlayerToolData
    {
        [JsonProperty] public TechType TechType { get; set; }
        [JsonProperty] public BatteryInfo BatteryInfo { get; set; }
        [JsonProperty] public bool HasBattery => BatteryInfo != null;
    }
}
