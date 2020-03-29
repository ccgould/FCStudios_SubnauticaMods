using System;
using System.Collections.Generic;
using FCSCommon.Objects;
using FCSTechFabricator.Objects;
using Oculus.Newtonsoft.Json;

namespace ARS_SeaBreezeFCS32.Configuration
{
    [Serializable]
    internal class SaveDataEntry
    {
        [JsonProperty] internal string ID { get; set; }
        [JsonProperty] internal float RemainingTime { get; set; }
        [JsonProperty] internal int FreonCount { get; set; }
        [JsonProperty] internal List<EatableEntities> FridgeContainer { get; set; }
        [JsonProperty] internal bool HasBreakerTripped { get; set; }
        [JsonProperty] internal string UnitName { get; set; }
        [JsonProperty] internal ColorVec4 BodyColor { get; set; }
    }
    
    [Serializable]
    internal class SaveData
    {
        [JsonProperty] internal List<SaveDataEntry> Entries = new List<SaveDataEntry>();
    }
}
