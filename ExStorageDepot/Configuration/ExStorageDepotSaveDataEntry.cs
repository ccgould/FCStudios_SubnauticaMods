using Oculus.Newtonsoft.Json;

namespace ExStorageDepot.Configuration
{
    internal class ExStorageDepotSaveDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        [JsonProperty] internal string UnitName { get; set; }
    }
}
