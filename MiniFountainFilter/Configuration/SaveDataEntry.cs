using FCSCommon.Objects;
using Oculus.Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AE.MiniFountainFilter.Configuration
{
    [Serializable]
    internal class SaveDataEntry
    {
        [JsonProperty] internal string ID { get; set; }
        [JsonProperty] internal int ContainerAmount { get; set; }
        [JsonProperty] internal float TankLevel { get; set; }
        [JsonProperty] internal ColorVec4 BodyColor { get; set; }
        [JsonProperty] internal bool IsInSub { get; set; }
    }

    [Serializable]
    internal class SaveData
    {
        [JsonProperty] internal List<SaveDataEntry> Entries { get; set; } = new List<SaveDataEntry>();
    }
}
