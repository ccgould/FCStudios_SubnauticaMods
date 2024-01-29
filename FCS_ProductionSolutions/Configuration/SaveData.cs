using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Enumerators;
using FCS_AlterraHub.Models.Interfaces;
using FCS_ProductionSolutions.Configuration.Structs;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Models;
using FCS_ProductionSolutions.ModItems.Buildables.HydroponicHarvester.Enums;
using FCS_ProductionSolutions.ModItems.Buildables.HydroponicHarvester.Model;
using FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Enumerators;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace FCS_ProductionSolutions.Configuration;

public class SaveData
{
    [JsonProperty]
    public HashSet<object> Data { get; set; } = new ();

    public class CubeGeneratorSaveData : ISaveDataEntry
    {
        public string Id { get; set; }
        public string BaseId { get; set; }
        public ColorTemplateSave ColorTemplate { get; set; }
        public float Progress { get; set; }
        public int State { get; set; }
        public float GenerationProgress { get; set; }
        public float CoolDownProgress { get; set; }
        [JsonProperty] internal IonCubeGenSpeedModes CurrentSpeedMode { get; set; }
        public int NumberOfCubes { get; set; }
    }

    public class DeepDrillerSaveDataEntry : ISaveDataEntry
    {
        [JsonProperty] public float Health { get; set; }

        [JsonProperty] public string Id { get; set; }

        [JsonProperty] public FCSPowerStates PowerState { get; set; }

        [JsonProperty] public Dictionary<TechType, int> Items { get; set; }

        [JsonProperty] internal DeepDrillerPowerData PowerData { get; set; }

        [JsonProperty] public HashSet<TechType> FocusOres { get; set; }

        [JsonProperty] public bool IsFocused { get; set; }

        [JsonProperty] public string Biome { get; set; }

        [JsonProperty] public float OilTimeLeft { get; set; }

        [JsonProperty] public bool SolarExtended { get; set; }

        [JsonProperty] public ColorTemplateSave ColorTemplate { get; set; }

        [JsonProperty] public bool PullFromRelay { get; set; }
        [JsonProperty] internal IEnumerable<UpgradeSave> Upgrades { get; set; }
        [JsonProperty] public bool IsRangeVisible { get; set; }
        [JsonProperty] public bool AllowedToExport { get; set; }
        [JsonProperty] public bool IsBlackListMode { get; set; }
        [JsonProperty] public bool IsBrakeSet { get; set; }
        [JsonProperty] public string BeaconName { get; set; }
        [JsonProperty] public bool IsPingVisible { get; set; }
        public string BaseId { get; set; }
    }

    public class DeepDrillerLightDutySaveDataEntry : ISaveDataEntry
    {

        [JsonProperty] public float Health { get; set; }

        public string Id { get; set; }
        public string BaseId { get; set; }

        [JsonProperty] public FCSPowerStates PowerState { get; set; }

        [JsonProperty] public Dictionary<TechType, int> Items { get; set; }

        [JsonProperty] internal DeepDrillerPowerData PowerData { get; set; }

        [JsonProperty] public float OilTimeLeft { get; set; }

        [JsonProperty] public ColorTemplateSave ColorTemplate { get; set; }
        [JsonProperty] public string BeaconName { get; set; }
        [JsonProperty] public bool IsPingVisible { get; set; }
        [JsonProperty] public bool IsBrakeSet { get; set; }
        [JsonProperty] public HashSet<TechType> FocusOres { get; set; }
        [JsonProperty] public bool IsFocused { get; set; }
        [JsonProperty] public bool IsBlackListMode { get; set; }
    }

    public class HarvesterSaveDataEntry : ISaveDataEntry
    {
        public string Id { get; set; }
        public string BaseId { get; set; }
        [JsonProperty] public ColorTemplateSave ColorTemplate { get; set; }
        public SlotData Slot1Data { get;  set; }
        public SlotData Slot2Data { get;  set; }
        public SlotData Slot3Data { get;  set; }
        public SlotData Slot4Data { get;  set; }
        public bool IsLightOn { get; set; }
        [JsonProperty] internal HarvesterSpeedModes SpeedMode { get; set; }
    }
}