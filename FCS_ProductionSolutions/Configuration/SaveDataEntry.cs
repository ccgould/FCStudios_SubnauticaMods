using System;
using System.Collections.Generic;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Objects;
using FCS_ProductionSolutions.DeepDriller.Configuration;
using FCS_ProductionSolutions.DeepDriller.Structs;
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
    internal class DeepDrillerSaveDataEntry
    {
        [JsonProperty] internal float Health { get; set; }

        [JsonProperty] internal string Id { get; set; }

        [JsonProperty] internal FCSPowerStates PowerState { get; set; }

        [JsonProperty] internal Dictionary<TechType, int> Items { get; set; }

        [JsonProperty] internal DeepDrillerPowerData PowerData { get; set; }

        [JsonProperty] internal HashSet<TechType> FocusOres { get; set; }

        [JsonProperty] internal bool IsFocused { get; set; }

        [JsonProperty] internal string Biome { get; set; }

        [JsonProperty] internal float OilTimeLeft { get; set; }

        [JsonProperty] internal bool SolarExtended { get; set; }

        [JsonProperty] internal ColorVec4 BodyColor { get; set; }

        [JsonProperty] internal bool PullFromRelay { get; set; }
        [JsonProperty] internal IEnumerable<UpgradeSave> Upgrades { get; set; }
        [JsonProperty] internal bool IsRangeVisible { get; set; }
        [JsonProperty] internal bool AllowedToExport { get; set; }
        [JsonProperty] internal bool IsBlackListMode { get; set; }
        public ColorVec4 SecColor { get; set; }
    }

    [Serializable]
    internal class SaveData
    {
        [JsonProperty] internal List<HydroponicHarvesterDataEntry> HydroponicHarvesterEntries = new List<HydroponicHarvesterDataEntry>();
        [JsonProperty] internal List<MatterAnalyzerDataEntry> MatterAnalyzerEntries = new List<MatterAnalyzerDataEntry>();
        [JsonProperty] internal List<DeepDrillerSaveDataEntry> DeepDrillerMk2Entries = new List<DeepDrillerSaveDataEntry>();
        [JsonProperty] internal List<TechType> HydroponicHarvesterKnownTech { get; set; } = new List<TechType>();
    }
}
