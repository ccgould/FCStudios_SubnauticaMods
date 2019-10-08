using AE.SeaCooker.Enumerators;
using FCSCommon.Objects;
using Oculus.Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AE.SeaCooker.Configuration
{
    [Serializable]
    internal class SaveDataEntry
    {
        [JsonProperty] internal string ID;
        [JsonProperty] internal float FuelLevel;
        [JsonProperty] internal FuelType TankType;
        [JsonProperty] internal ColorVec4 BodyColor;
        [JsonProperty] internal bool ExportToSeaBreeze;
        [JsonProperty] internal bool IsCooking;
        [JsonProperty] internal float PassedTime;
        [JsonProperty] internal float TargetTime;
        [JsonProperty] internal IEnumerable<EatableEntities> Input;
        [JsonProperty] internal IEnumerable<EatableEntities> Export;
    }

    [Serializable]
    internal class SaveData
    {
        [JsonProperty] internal List<SaveDataEntry> Entries = new List<SaveDataEntry>();
    }
}
