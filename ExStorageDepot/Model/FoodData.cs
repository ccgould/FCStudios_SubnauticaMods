using Oculus.Newtonsoft.Json;

namespace ExStorageDepot.Model
{
    internal class FoodData
    {
        [JsonProperty] internal float FoodValue { get; set; }
        [JsonProperty] internal float WaterValue { get; set; }

    }
}
