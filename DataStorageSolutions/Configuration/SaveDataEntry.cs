using System;
using System.Collections.Generic;
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
        [JsonProperty] internal List<List<ObjectData>> Servers { get; set; }
        [JsonProperty] internal List<ObjectData> ServerData { get; set; }
        [JsonProperty] internal string AntennaName { get; set; }
    }

    [Serializable]
    internal class SaveData
    {
        [JsonProperty] internal List<SaveDataEntry> Entries = new List<SaveDataEntry>();
        public Dictionary<string, List<ObjectData>> Servers { get; set; }
    }

    [Serializable]
    internal class ObjectData
    {
        public SaveDataObjectType DataObjectType { get; set; }
        public TechType TechType { get; set; }
        public int Slot { get; set; }
        public EatableEntities EatableEntity { get; set; }
        public PlayerToolData PlayToolData { get; set; }
        public List<ObjectData> ServerData { get; set; }
    }

    internal class PlayerToolData
    {
        public TechType TechType { get; set; }
        public BatteryInfo BatteryInfo { get; set; }
        public bool HasBattery => BatteryInfo != null;
    }
}
