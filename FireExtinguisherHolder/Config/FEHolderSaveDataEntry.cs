using Oculus.Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MAC.FireExtinguisherHolder.Config
{
    [Serializable]
    internal class FEHolderSaveDataEntry
    {
        [JsonProperty] internal float Fuel { get; set; }
        [JsonProperty] internal string ID { get; set; }
        [JsonProperty] internal bool HasTank { get; set; }
    }

    [Serializable]
    internal class FEHolderSaveData
    {
        [JsonProperty] internal List<FEHolderSaveDataEntry> Entries = new List<FEHolderSaveDataEntry>();
    }
}
