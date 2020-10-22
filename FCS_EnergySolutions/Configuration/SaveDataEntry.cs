using System;
using System.Collections.Generic;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Objects;
using Oculus.Newtonsoft.Json;

namespace FCS_EnergySolutions.Configuration
{
    [Serializable]
    internal class AlterraGenDataEntry
    {
        [JsonProperty] internal string ID { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal ColorVec4 BodyColor { get; set; }
        [JsonProperty] internal Dictionary<TechType, int> Storage { get; set; }
        [JsonProperty] internal float ToConsume { get; set; }
        [JsonProperty] internal FCSPowerStates PowerState { get; set; }
        [JsonProperty] internal float Power { get; set; }
        [JsonProperty] internal float StoredPower { get; set; }
        [JsonProperty] internal bool IsVisible { get; set; }
    }

    [Serializable]
    internal class SaveData
    {
        [JsonProperty] internal List<AlterraGenDataEntry> AlterraGenEntries = new List<AlterraGenDataEntry>();
    }
}
