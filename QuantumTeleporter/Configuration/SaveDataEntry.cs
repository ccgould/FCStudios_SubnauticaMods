using System;
using System.Collections.Generic;
using FCSCommon.Objects;
using Oculus.Newtonsoft.Json;
using QuantumTeleporter.Enumerators;

namespace QuantumTeleporter.Configuration
{
    [Serializable]
    internal class SaveDataEntry
    {
        [JsonProperty] internal string ID { get; set; }
        [JsonProperty] internal ColorVec4 BodyColor { get; set; }
        [JsonProperty] internal string UnitName { get; set; }
        [JsonProperty] internal bool IsGlobal { get; set; }
        [JsonProperty] internal QTTabs SelectedTab { get; set; }
    }

    [Serializable]
    internal class SaveData
    {
        [JsonProperty] internal List<SaveDataEntry> Entries { get; set; } = new List<SaveDataEntry>();
    }
}
