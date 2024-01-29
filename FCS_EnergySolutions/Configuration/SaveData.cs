using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.Core.Enumerators;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Enumerators;
using FCS_AlterraHub.Models.Interfaces;
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
}
