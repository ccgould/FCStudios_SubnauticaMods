using System;
using System.Collections.Generic;
using DataStorageSolutions.Model;
using DataStorageSolutions.Mono;
using FCSCommon.Enums;
using FCSCommon.Objects;
using FCSTechFabricator.Objects;
using Oculus.Newtonsoft.Json;

namespace DataStorageSolutions.Configuration
{
    [Serializable]
    internal class SaveDataEntry
    {
        [JsonProperty] internal string FileVersion { get; set; } = "1.0.0";
        [JsonProperty] internal ColorVec4 TerminalBodyColor { get; set; }
        [JsonProperty] internal string ID { get; set; }
        [JsonProperty] internal ColorVec4 BodyColor { get; set; }
        [JsonProperty] internal ColorVec4 AntennaBodyColor { get; set; }
        [JsonProperty] internal List<ServerData> Servers { get; set; }
        [JsonProperty] internal List<ObjectData> ServerData { get; set; }
        [JsonProperty] internal string AntennaName { get; set; }
        [JsonProperty] internal List<Filter> Filters { get; set; }
    }

    [Serializable]
    internal class SaveData
    {
        [JsonProperty] internal List<SaveDataEntry> Entries = new List<SaveDataEntry>();
        [JsonProperty] internal Dictionary<string, ServerData> Servers { get; set; }
    }

    [Serializable]
    internal class ObjectData
    {
        [JsonProperty] internal SaveDataObjectType DataObjectType { get; set; }
        [JsonProperty] internal TechType TechType { get; set; }
        [JsonProperty] internal int Slot { get; set; }
        [JsonProperty] internal EatableEntities EatableEntity { get; set; }
        [JsonProperty] internal PlayerToolData PlayToolData { get; set; }
        [JsonProperty] internal List<ObjectData> ServerData { get; set; }
        [JsonProperty] internal List<Filter> Filters { get; set; }
    }

    internal class PlayerToolData
    {
        [JsonProperty] internal TechType TechType { get; set; }
        [JsonProperty] internal BatteryInfo BatteryInfo { get; set; }
        [JsonProperty] internal bool HasBattery => BatteryInfo != null;
    }
}
