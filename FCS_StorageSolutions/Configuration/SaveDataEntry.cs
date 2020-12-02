using System;
using System.Collections.Generic;
using FCS_AlterraHub.Objects;
using Oculus.Newtonsoft.Json;

namespace FCS_StorageSolutions.Configuration
{
    [Serializable]
    internal class AlterraStorageDataEntry
    {
        [JsonProperty] internal string ID { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal ColorVec4 BodyColor { get; set; }
        [JsonProperty] internal bool IsVisible { get; set; }
        [JsonProperty] internal byte[] Data { get; set; }
    }

    [Serializable]
    internal class SaveData
    {
        [JsonProperty] internal List<AlterraStorageDataEntry> AlterraStorageDataEntries = new List<AlterraStorageDataEntry>();
    }
}
