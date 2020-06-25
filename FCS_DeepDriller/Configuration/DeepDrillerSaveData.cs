using FCSCommon.Enums;
using Oculus.Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace FCS_DeepDriller.Configuration
{
    [Serializable]
    internal class DeepDrillerSaveDataEntry
    {
        [JsonProperty] internal float Health { get; set; }

        [JsonProperty] internal string Id { get; set; }

        [JsonProperty] internal FCSPowerStates PowerState { get; set; }

        [JsonProperty] internal IEnumerable<KeyValuePair<TechType, int>> Items { get; set; }

        [JsonProperty] internal DeepDrillerPowerData PowerData { get; set; }
        
        [JsonProperty] internal HashSet<TechType> FocusOres { get; set; }

        [JsonProperty] internal bool IsFocused { get; set; }
        
        [JsonProperty] internal string Biome { get; set; }
    }

    [Serializable]
    internal class DeepDrillerSaveData
    {
        [JsonProperty] internal float SaveVersion { get; set; } = 1.0f;
        [JsonProperty] internal List<DeepDrillerSaveDataEntry> Entries = new List<DeepDrillerSaveDataEntry>();
    }
}
