using Oculus.Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MAC.OxStation.Config
{
    [Serializable]
    internal class SaveDataEntry
    {
        [JsonProperty] internal string ID;
        [JsonProperty] internal float OxygenLevel { get; set; }
        [JsonProperty] internal float HealthLevel { get; set; }
        [JsonProperty] internal string BeaconID { get; set; }
    }

    [Serializable]
    internal class SaveData
    {
        [JsonProperty] internal List<SaveDataEntry> Entries = new List<SaveDataEntry>();
    }
}
