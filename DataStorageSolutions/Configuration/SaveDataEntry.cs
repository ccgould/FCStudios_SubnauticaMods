using System;
using System.Collections.Generic;
using DataStorageSolutions.Model;
using FCSCommon.Objects;
using FCSTechFabricator.Objects;
using Oculus.Newtonsoft.Json;

namespace DataStorageSolutions.Configuration
{
    [Serializable]
    internal class SaveDataEntry
    {
        [JsonProperty] internal string FileVersion { get; set; } = "2.0";
        [JsonProperty] internal string BaseID { get; set; }
        [JsonProperty] internal ColorVec4 TerminalBodyColor { get; set; }
        [JsonProperty] internal string ID { get; set; }
        [JsonProperty] internal ColorVec4 BodyColor { get; set; }
        [JsonProperty] internal ColorVec4 AntennaBodyColor { get; set; }
        [JsonProperty] internal List<ServerData> Servers { get; set; }
        [JsonProperty] internal HashSet<ObjectData> ServerData { get; set; }
        [JsonProperty] internal string AntennaName { get; set; }
        [JsonProperty] internal List<Filter> Filters { get; set; }
        [JsonProperty] internal TechType ItemDisplayItem { get; set; }
    }

    [Serializable]
    internal class SaveData
    {
        [JsonProperty] internal HashSet<ServerData> GlobalServers { get; set; }
        [JsonProperty] internal List<BaseSaveData> Bases { get; set; }

        [JsonProperty] internal List<SaveDataEntry> Entries = new List<SaveDataEntry>();
    }

    internal class OperationSaveData
    {
        public string FromDeviceID { get; set; }
        public string ToDeviceID { get; set; }
        public string ManagerID { get; set; }
        public TechType TechType { get; set; }
        public bool IsCraftable { get; set; }
    }
}
