using AE.SeaCooker.Enumerators;
using FCSCommon.Objects;
using Oculus.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using FCSTechFabricator.Objects;

namespace AE.SeaCooker.Configuration
{
    [Serializable]
    internal class SaveDataEntry
    {
        [JsonProperty] internal string ID { get; set; }
        [JsonProperty] internal float FuelLevel { get; set; }
        [JsonProperty] internal FuelType TankType { get; set; }
        [JsonProperty] internal ColorVec4 BodyColor { get; set; }
        [JsonProperty] internal bool ExportToSeaBreeze { get; set; }
        [JsonProperty] internal bool IsCooking { get; set; }
        [JsonProperty] internal float PassedTime { get; set; }
        [JsonProperty] internal float TargetTime { get; set; }
        [JsonProperty] internal IEnumerable<EatableEntities> Input { get; set; }
        [JsonProperty] internal IEnumerable<EatableEntities> Export { get; set; }
        [JsonProperty] internal string CurrentSeaBreezeID { get; set; }
    [JsonProperty] internal bool AutoChooseSeabreeze { get; set; }
    }

    [Serializable]
    internal class SaveData
    {
        [JsonProperty] internal List<SaveDataEntry> Entries = new List<SaveDataEntry>();
    }
}
