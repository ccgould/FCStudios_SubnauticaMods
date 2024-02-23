using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Enumerators;
using FCS_AlterraHub.Models.Interfaces;
using FCS_EnergySolutions.ModItems.Buildables.TelepowerPylon.Enumerators;
using FCS_EnergySolutions.ModItems.Buildables.UniversalCharger.Enumerators;
using Nautilus.Json;
using Nautilus.Json.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace FCS_EnergySolutions.Configuration;

internal class SaveData
{
    [JsonProperty]
    public HashSet<object> Data { get; set; } = new();

    public class SolarClusterSaveData : ISaveDataEntry
    {
        public string Id { get; set; }
        public string BaseId { get; set; }
        public ColorTemplateSave ColorTemplate { get; set; }
        [JsonProperty] internal float StoredPower { get; set; }
    }

    internal class PowerStorageDataEntry : ISaveDataEntry
    {
        public string Id { get; set; }
        public string BaseId { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        public ColorTemplateSave ColorTemplate { get; set; }
        [JsonProperty] internal FCSPowerStates PowerState { get; set; }
        [JsonProperty] internal byte[] Data { get; set; }
    }

    internal class JetStreamT242SaveData : ISaveDataEntry
    {
        public string Id { get; set; }
        public string BaseId { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        public ColorTemplateSave ColorTemplate { get; set; }
        [JsonProperty] internal FCSPowerStates PowerState { get; set; }
        [JsonProperty] internal float StoredPower { get; set; }
        [JsonProperty] internal string CurrentBiome { get; set; }
        [JsonProperty] internal RotorSaveData RotorSaveData { get; set; }
        [JsonProperty] internal MotorSaveData MotorSaveData { get; set; }
    }

    internal class AlterraGenDataEntry : ISaveDataEntry
    {
        public string Id { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        public ColorTemplateSave ColorTemplate { get; set; }
        [JsonProperty] internal Dictionary<TechType, int> Storage { get; set; }
        [JsonProperty] internal float ToConsume { get; set; }
        [JsonProperty] internal FCSPowerStates PowerState { get; set; }
        [JsonProperty] internal float Power { get; set; }
        [JsonProperty] internal float StoredPower { get; set; }
        public string BaseId { get; set; }
        [JsonProperty] internal bool IsVisible { get; set; }
    }

    internal class TelepowerPylonDataEntry : ISaveDataEntry
    {
        public string Id { get; set; }
        public string BaseId { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        public ColorTemplateSave ColorTemplate { get; set; }
        [JsonProperty] internal FCSPowerStates PowerState { get; set; }
        [JsonProperty] internal TelepowerPylonMode PylonMode { get; set; }
        [JsonProperty] internal List<string> CurrentConnections { get; set; }
    }

    internal class TelepowerPylonBaseDataEntry
    {
        [JsonProperty] internal TelepowerPylonUpgrade CurrentUpgrade { get;  set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal FCSPowerStates PowerState { get; set; }
        [JsonProperty] internal TelepowerPylonMode PylonMode { get; set; }
        [JsonProperty] internal List<string> CurrentConnections { get; set; }
        [JsonProperty] internal int GlobalBaseCount { get; set; }
        [JsonProperty] internal int BasePylonCount { get; set; }
    }

    internal class UniversalChargerDataEntry : ISaveDataEntry
    {
        public string Id { get; set; }
        public string BaseId { get; set; }
        public ColorTemplateSave ColorTemplate { get; set; }
        [JsonProperty] internal Dictionary<string, string> BatteryData { get; set; }
        [JsonProperty] internal Dictionary<string, string> ChargerData { get; set; }
        [JsonProperty] internal PowerChargerMode Mode { get; set; }
    }

    

    [FileName("EnergySolutions")]
    internal class EnergrySolutionsSaveData : SaveDataCache
    {
        public Dictionary<string, TelepowerPylonBaseDataEntry> TelepowerPylonBaseData = new();

    }
}