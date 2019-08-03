using Oculus.Newtonsoft.Json;

namespace ExStorageDepot.Model
{
    internal class BatteryData
    {
        [JsonProperty] internal float Charge { get; set; }
        [JsonProperty] internal float Capacity { get; set; }
        [JsonProperty] internal TechType TechType { get; set; }
    }
}
