using FCSCommon.Enums;
using Oculus.Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace FCS_DeepDriller.Configuration
{
    [Serializable]
    internal class DeepDrillerSaveDataEntry
    {
        [JsonProperty] internal List<SlotData> Modules { get; set; }

        [JsonProperty] internal float Health { get; set; }

        [JsonProperty] internal string Id { get; set; }

        [JsonProperty] internal FCSPowerStates PowerState { get; set; }

        [JsonProperty] internal IEnumerable<KeyValuePair<TechType, int>> Items { get; set; }

        [JsonProperty] internal DeepDrillerPowerData PowerData { get; set; }
        public TechType FocusOre { get; set; }
        public bool IsFocused { get; set; }
    }

    [Serializable]
    internal class DeepDrillerSaveData
    {
        [JsonProperty] internal List<DeepDrillerSaveDataEntry> Entries = new List<DeepDrillerSaveDataEntry>();
    }
}
