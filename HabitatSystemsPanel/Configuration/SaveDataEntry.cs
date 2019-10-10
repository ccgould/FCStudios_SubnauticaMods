using Oculus.Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AE.HabitatSystemsPanel.Configuration
{
    [Serializable]
    internal class SaveDataEntry
    {
        [JsonProperty] internal string ID { get; set; }
    }

    [Serializable]
    internal class SaveData
    {
        [JsonProperty] internal List<SaveDataEntry> Entries { get; set; } = new List<SaveDataEntry>();
    }
}
