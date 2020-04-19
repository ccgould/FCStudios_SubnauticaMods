using System;
using System.Collections.Generic;
using FCS_HydroponicHarvesters.Enumerators;
using FCS_HydroponicHarvesters.Model;
using FCSTechFabricator.Enums;
using FCSTechFabricator.Objects;
using Oculus.Newtonsoft.Json;

namespace Model
{
    [Serializable]
    internal class SaveDataEntry
    {
        [JsonProperty] internal Dictionary<TechType, StoredDNAData> DnaSamples { get; set; }
        [JsonProperty] internal string ID { get; set; }
        [JsonProperty] internal ColorVec4 BodyColor { get; set; }
        [JsonProperty] internal Dictionary<TechType, int> Container { get; set; }
        [JsonProperty] internal float StartUpProgress { get; set; }
        [JsonProperty] internal float GenerationProgress { get; set; }
        [JsonProperty] internal float CoolDownProgress { get; set; }
        [JsonProperty] internal SpeedModes CurrentSpeedMode { get; set; }
        [JsonProperty] internal bool LightState { get; set; }
        [JsonProperty] internal FCSEnvironment BedType { get; set; }
        [JsonProperty] internal float UnitSanitation { get; set; }
    }

    [Serializable]
    internal class SaveData
    {
        [JsonProperty] internal List<SaveDataEntry> Entries = new List<SaveDataEntry>();
    }
}
