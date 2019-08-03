using Oculus.Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ExStorageDepot.Configuration
{
    [Serializable]
    internal class ExStorageDepotSaveData
    {
        [JsonProperty] internal List<ExStorageDepotSaveDataEntry> Entries = new List<ExStorageDepotSaveDataEntry>();
    }
}
