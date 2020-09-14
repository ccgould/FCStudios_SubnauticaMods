using System;
using System.Collections.Generic;
using FCSCommon.Enums;
using FCSTechFabricator.Enums;
using FCSTechFabricator.Objects;
using Oculus.Newtonsoft.Json;

namespace AlterraGen.Configuration
{
    [Serializable]
    internal class SaveDataEntry
    {
        [JsonProperty] internal string ID { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal ColorVec4 BodyColor { get; set; }
        [JsonProperty] internal Dictionary<TechType,int> Storage { get; set; }
        [JsonProperty] internal float ToConsume { get; set; }
        [JsonProperty] internal FCSPowerStates PowerState { get; set; }
        [JsonProperty] internal float Power { get; set; }
        [JsonProperty] internal float StoredPower { get; set; }
        [JsonProperty] internal bool IsVisible { get; set; }
    }

    [Serializable]
    internal class SaveData
    {
        [JsonProperty] internal List<SaveDataEntry> Entries = new List<SaveDataEntry>();
    }
}
