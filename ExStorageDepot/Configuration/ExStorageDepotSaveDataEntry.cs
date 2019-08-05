using ExStorageDepot.Model;
using Oculus.Newtonsoft.Json;
using System.Collections.Generic;

namespace ExStorageDepot.Configuration
{
    internal class ExStorageDepotSaveDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        [JsonProperty] internal string UnitName { get; set; }
        [JsonProperty] internal List<ItemData> StorageItems { get; set; }
    }
}
