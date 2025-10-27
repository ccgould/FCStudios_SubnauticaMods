using FCS_AlterraHub.Configuation;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Enumerators;
using FCS_AlterraHub.Models.Interfaces;
using FCS_ProductionSolutions.Configuration.Structs;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Models;
using FCS_ProductionSolutions.ModItems.Buildables.HydroponicHarvester.Enums;
using FCS_ProductionSolutions.ModItems.Buildables.HydroponicHarvester.Model;
using FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Enumerators;
using Newtonsoft.Json;
using SQLite;
using System.Collections.Generic;

namespace FCS_ProductionSolutions.Configuration;

public class SaveData
{
    [JsonProperty]
    public HashSet<object> Data { get; set; } = new ();

    [Table("CubeGenerator")]
    public class CubeGeneratorSaveData : BaseSaveData
    {
        public float Progress { get; set; }
        public int State { get; set; }
        public float GenerationProgress { get; set; }
        public float CoolDownProgress { get; set; }
        internal IonCubeGenSpeedModes CurrentSpeedMode { get; set; }
        public int NumberOfCubes { get; set; }
    }

    [Table("DeepDriller")]
    public class DeepDrillerSaveDataEntry : BaseSaveData
    {
        public float Health { get; set; }
        public FCSPowerStates PowerState { get; set; }

        public Dictionary<TechType, int> Items { get; set; }

        internal DeepDrillerPowerData PowerData { get; set; }

        public HashSet<TechType> FocusOres { get; set; }

        public bool IsFocused { get; set; }

        public string Biome { get; set; }

        public float OilTimeLeft { get; set; }

        public bool SolarExtended { get; set; }
        public bool PullFromRelay { get; set; }
        internal IEnumerable<UpgradeSave> Upgrades { get; set; }
        public bool IsRangeVisible { get; set; }
        public bool AllowedToExport { get; set; }
        public bool IsBlackListMode { get; set; }
        public bool IsBrakeSet { get; set; }
        public string BeaconName { get; set; }
        public bool IsPingVisible { get; set; }
    }

    [Table("DeepDrillerOperator")]
    public class DeepDrillerOperatorSaveDataEntry : BaseSaveData
    {
        public float Health { get; set; }

        public FCSPowerStates PowerState { get; set; }

        public Dictionary<TechType, int> Items { get; set; }

        internal DeepDrillerPowerData PowerData { get; set; }

        public HashSet<TechType> FocusOres { get; set; }

        public bool IsFocused { get; set; }

        public string Biome { get; set; }
        public bool SolarExtended { get; set; }
        public bool PullFromRelay { get; set; }
        internal IEnumerable<UpgradeSave> Upgrades { get; set; }
        public bool IsRangeVisible { get; set; }
        public bool AllowedToExport { get; set; }
        public bool IsBlackListMode { get; set; }
        public bool IsBrakeSet { get; set; }
        public string BeaconName { get; set; }
        public bool IsPingVisible { get; set; }
        public List<string> ConnectedDrills { get; set; }
    }

    [Table("DeepDrillerLightDuty")]
    public class DeepDrillerLightDutySaveDataEntry : BaseSaveData
    {

        public float Health { get; set; }

        public FCSPowerStates PowerState { get; set; }

        public Dictionary<TechType, int> Items { get; set; }

        internal DeepDrillerPowerData PowerData { get; set; }

        public float OilTimeLeft { get; set; }
        public string BeaconName { get; set; }
        public bool IsPingVisible { get; set; }
        public bool IsBrakeSet { get; set; }
        public HashSet<TechType> FocusOres { get; set; }
        public bool IsFocused { get; set; }
        public bool IsBlackListMode { get; set; }
    }

    [Table("Harvester")]
    public class HarvesterSaveDataEntry : BaseSaveData
    {
        // Slot 1
        public string Slot1DataJson { get; set; }

        [Ignore]
        public SlotData Slot1Data
        {
            get => string.IsNullOrEmpty(Slot1DataJson)
                ? new SlotData()
                : JsonConvert.DeserializeObject<SlotData>(Slot1DataJson);
            set => Slot1DataJson = JsonConvert.SerializeObject(value);
        }

        // Slot 2
        public string Slot2DataJson { get; set; }

        [Ignore]
        public SlotData Slot2Data
        {
            get => string.IsNullOrEmpty(Slot2DataJson)
                ? new SlotData()
                : JsonConvert.DeserializeObject<SlotData>(Slot2DataJson);
            set => Slot2DataJson = JsonConvert.SerializeObject(value);
        }

        // Slot 3
        public string Slot3DataJson { get; set; }

        [Ignore]
        public SlotData Slot3Data
        {
            get => string.IsNullOrEmpty(Slot3DataJson)
                ? new SlotData()
                : JsonConvert.DeserializeObject<SlotData>(Slot3DataJson);
            set => Slot3DataJson = JsonConvert.SerializeObject(value);
        }

        // Slot 4
        public string Slot4DataJson { get; set; }

        [Ignore]
        public SlotData Slot4Data
        {
            get => string.IsNullOrEmpty(Slot4DataJson)
                ? new SlotData()
                : JsonConvert.DeserializeObject<SlotData>(Slot4DataJson);
            set => Slot4DataJson = JsonConvert.SerializeObject(value);
        }

        public bool IsLightOn { get; set; }
        [JsonProperty] internal HarvesterSpeedModes SpeedMode { get; set; }
    }
}