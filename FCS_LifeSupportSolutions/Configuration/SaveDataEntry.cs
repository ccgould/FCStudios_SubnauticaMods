using System;
using System.Collections.Generic;
using FCS_AlterraHub.Objects;
using FCSCommon.Interfaces;
#if SUBNAUTICA_STABLE
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif

namespace FCS_LifeSupportSolutions.Configuration
{
    [Serializable]
    internal class EnergyPillVendingMachineEntry : ISaveDataEntry
    {
        public string Id { get; set; }
        public string BaseId { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal Vec4 Body { get; set; }
        [JsonProperty] internal Vec4 SecondaryBody { get; set; }
    }      
    
    internal class BaseUtilityEntry : ISaveDataEntry
    {
        public string Id { get; set; }
        public string BaseId { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal Vec4 Body { get; set; }
        [JsonProperty] internal Vec4 SecondaryBody { get; set; }
        [JsonProperty] internal float O2Level { get; set; }
    }
    internal class BaseOxygenTankEntry : ISaveDataEntry
    {
        public string Id { get; set; }
        public string BaseId { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal Vec4 Body { get; set; }
        [JsonProperty] internal Vec4 SecondaryBody { get; set; }
        [JsonProperty] internal float O2Level { get; set; }
        [JsonProperty] internal string ParentID { get; set;}
    }    
    
    internal class MiniMedBayEntry : ISaveDataEntry
    {
        public string Id { get; set; }
        public string BaseId { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal Vec4 Body { get; set; }
        [JsonProperty] internal Vec4 SecondaryBody { get; set; }
        [JsonProperty] internal int FirstAidCount { get; set; }
        [JsonProperty] internal float TimeToSpawn { get; set; }
    }

    [Serializable]
    internal class SaveData
    {
        [JsonProperty] internal List<EnergyPillVendingMachineEntry> EnergyPillVendingMachineEntries = new List<EnergyPillVendingMachineEntry>();
        [JsonProperty] internal List<MiniMedBayEntry> MiniMedBayEntries = new List<MiniMedBayEntry>();
        [JsonProperty] internal List<BaseUtilityEntry> BaseUtilityUnitEntries = new List<BaseUtilityEntry>();
        [JsonProperty] internal List<BaseOxygenTankEntry> BaseOxygenTankEntries = new List<BaseOxygenTankEntry>();
    }
}
