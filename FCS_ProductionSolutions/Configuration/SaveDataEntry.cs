using System;
using System.Collections.Generic;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Objects;
using Oculus.Newtonsoft.Json;

namespace FCS_ProductionSolutions.Configuration
{
    [Serializable]
    internal class HydroponicHarvesterDataEntry
    {
        [JsonProperty] internal string ID { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal ColorVec4 BodyColor { get; set; }
        [JsonProperty] internal Dictionary<TechType, int> Storage { get; set; }
        [JsonProperty] internal bool IsVisible { get; set; }
        [JsonProperty] internal bool IsInBase { get; set; }
    }

    internal class MatterAnalyzerDataEntry
    {
        [JsonProperty] internal string ID { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal ColorVec4 BodyColor { get; set; }
        [JsonProperty] internal Dictionary<TechType, int> Storage { get; set; }
        [JsonProperty] internal bool IsVisible { get; set; }
        [JsonProperty] internal TechType CurrentTechType { get; set; }
        [JsonProperty] internal float CurrentScanTime { get; set; }
        [JsonProperty] internal float CurrentMaxScanTime { get; set; }
    }

    [Serializable]
    internal class SaveData
    {
        [JsonProperty] internal List<HydroponicHarvesterDataEntry> HydroponicHarvesterEntries = new List<HydroponicHarvesterDataEntry>();
        [JsonProperty] internal List<MatterAnalyzerDataEntry> MatterAnalyzerEntries = new List<MatterAnalyzerDataEntry>();
        [JsonProperty] internal List<TechType> HydroponicHarvesterKnownTech { get; set; } = new List<TechType>();
    }
}
