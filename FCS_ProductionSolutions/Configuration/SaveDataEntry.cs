using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Objects;
using FCS_ProductionSolutions.DeepDriller.Configuration;
using FCS_ProductionSolutions.DeepDriller.Structs;
using FCS_ProductionSolutions.HydroponicHarvester.Enumerators;
using FCS_ProductionSolutions.HydroponicHarvester.Mono;
using FCS_ProductionSolutions.Mods.AutoCrafter;
using Oculus.Newtonsoft.Json;

namespace FCS_ProductionSolutions.Configuration
{
    [Serializable]
    internal class HydroponicHarvesterDataEntry
    {
        [JsonProperty] internal string ID { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal Vec4 Body { get; set; }
        [JsonProperty] internal Dictionary<TechType, int> Storage { get; set; }
        [JsonProperty] internal bool IsVisible { get; set; }
        [JsonProperty] internal bool IsInBase { get; set; }
        [JsonProperty] internal SpeedModes SpeedMode { get; set; }
        [JsonProperty] internal List<SlotsData> SlotData { get; set; }
        [JsonProperty] internal bool SetBreaker { get; set; }
    }

    internal class ReplicatorDataEntry
    {
        [JsonProperty] internal string ID { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal Vec4 Body { get; set; }
        [JsonProperty] internal bool IsVisible { get; set; }
        [JsonProperty] internal TechType TargetItem { get; set; }
        [JsonProperty] internal float Progress { get; set; }
        [JsonProperty] internal int ItemCount { get; set; }
        [JsonProperty] internal SpeedModes Speed { get; set; }
    }

    internal class MatterAnalyzerDataEntry
    {
        [JsonProperty] internal float RPM;
        [JsonProperty] internal string ID { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal Vec4 Body { get; set; }
        [JsonProperty] internal Dictionary<TechType, int> Storage { get; set; }
        [JsonProperty] internal bool IsVisible { get; set; }
        [JsonProperty] internal TechType CurrentTechType { get; set; }
        [JsonProperty] internal float CurrentScanTime { get; set; }
        [JsonProperty] internal float CurrentMaxScanTime { get; set; }
        [JsonProperty] internal TechType PickTechType { get; set; }
        [JsonProperty] internal bool IsLandPlant { get; set; }
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

        [JsonProperty] internal Vec4 Body { get; set; }

        [JsonProperty] internal bool PullFromRelay { get; set; }
        [JsonProperty] internal IEnumerable<UpgradeSave> Upgrades { get; set; }
        [JsonProperty] internal bool IsRangeVisible { get; set; }
        [JsonProperty] internal bool AllowedToExport { get; set; }
        [JsonProperty] internal bool IsBlackListMode { get; set; }
        [JsonProperty] internal Vec4 Sec { get; set; }
        [JsonProperty] internal bool IsBrakeSet { get; set; }
    }

    [Serializable]
    internal class DSSAutoCrafterDataEntry
    {
        [JsonProperty] internal string ID { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal Vec4 Body { get; set; }
        [JsonProperty] internal Vec4 SecondaryBody { get; set; }
        [JsonProperty] internal ObservableCollection<CraftingOperation> CurrentProcess { get; set; }
        public bool IsRunning { get; set; }
        [JsonProperty] internal AutoCrafterMode CurrentCrafterMode { get; set; }
        [JsonProperty] internal HashSet<TechType> KnownCrafts { get; set; }
        [JsonProperty] internal List<TechType> StoredItems { get; set; }
        [JsonProperty] internal List<string> ConnectedDevices { get; set; }
        [JsonProperty] internal StandByModes StandyMode { get; set; }
    }

    [Serializable]
    internal class SaveData
    {
        [JsonProperty] internal List<HydroponicHarvesterDataEntry> HydroponicHarvesterEntries = new List<HydroponicHarvesterDataEntry>();
        [JsonProperty] internal List<MatterAnalyzerDataEntry> MatterAnalyzerEntries = new List<MatterAnalyzerDataEntry>();
        [JsonProperty] internal List<DeepDrillerSaveDataEntry> DeepDrillerMk2Entries = new List<DeepDrillerSaveDataEntry>();
        [JsonProperty] internal List<ReplicatorDataEntry> ReplicatorEntries = new List<ReplicatorDataEntry>();
        [JsonProperty] internal List<DSSAutoCrafterDataEntry> DSSAutoCrafterDataEntries = new List<DSSAutoCrafterDataEntry>();

        [JsonProperty] internal List<DNASampleData> HydroponicHarvesterKnownTech { get; set; } = new List<DNASampleData>();
    }
}
