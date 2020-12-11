using System;
using System.Collections.Generic;
using FCS_AlterraHub.Objects;
using FCSCommon.Interfaces;
using Oculus.Newtonsoft.Json;

namespace FCS_LifeSupportSolutions.Configuration
{
    [Serializable]
    internal class EnergyPillVendingMachineEntry : ISaveDataEntry
    {
        public string Id { get; set; }
        public string BaseId { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal ColorVec4 BodyColor { get; set; }
        [JsonProperty] internal ColorVec4 SecondaryBodyColor { get; set; }
    }    
    
    internal class MiniMedBayEntry : ISaveDataEntry
    {
        public string Id { get; set; }
        public string BaseId { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal ColorVec4 BodyColor { get; set; }
        [JsonProperty] internal ColorVec4 SecondaryBodyColor { get; set; }
        [JsonProperty] internal int FirstAidCount { get; set; }
        [JsonProperty] internal float TimeToSpawn { get; set; }
    }

    [Serializable]
    internal class SaveData
    {
        [JsonProperty] internal List<EnergyPillVendingMachineEntry> EnergyPillVendingMachineEntries = new List<EnergyPillVendingMachineEntry>();
        [JsonProperty] internal List<MiniMedBayEntry> MiniMedBayEntries = new List<MiniMedBayEntry>();
    }
}
