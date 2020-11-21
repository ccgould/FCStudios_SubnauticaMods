using System;
using System.Collections.Generic;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Objects;
using FCSCommon.Interfaces;
using Oculus.Newtonsoft.Json;

namespace FCS_EnergySolutions.Configuration
{
    [Serializable]
    internal class AlterraGenDataEntry : ISaveDataEntry
    {
        public string Id { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal ColorVec4 BodyColor { get; set; }
        [JsonProperty] internal Dictionary<TechType, int> Storage { get; set; }
        [JsonProperty] internal float ToConsume { get; set; }
        [JsonProperty] internal FCSPowerStates PowerState { get; set; }
        [JsonProperty] internal float Power { get; set; }
        [JsonProperty] internal float StoredPower { get; set; }
        public string BaseId { get; set; }
        [JsonProperty] internal bool IsVisible { get; set; }
    }

    internal class JetStreamT242DataEntry : ISaveDataEntry
    {
        public string Id { get; set; }
        public string BaseId { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal ColorVec4 BodyColor { get; set; }
        [JsonProperty] internal ColorVec4 SecondaryBodyColor { get; set; }
        [JsonProperty] internal FCSPowerStates PowerState { get; set; }
        [JsonProperty] internal float StoredPower { get; set; }
        [JsonProperty] internal string CurrentBiome { get; set; }
        [JsonProperty] internal bool IsIncreasing { get; set; }
        [JsonProperty] internal float CurrentSpeed { get; set; }
        [JsonProperty] internal float TargetRPM { get; set; }
        [JsonProperty] internal float TilterDeg { get; set; }
        [JsonProperty] internal bool TilterUseGlobal { get; set; }
        [JsonProperty] internal TargetAxis TilterAxis { get; set; }
        [JsonProperty] internal bool AxisTilterSet { get; set; }
        [JsonProperty] internal bool TilterMove { get; set; }
    }

    [Serializable]
    internal class SaveData
    {
        [JsonProperty] internal List<AlterraGenDataEntry> AlterraGenEntries = new List<AlterraGenDataEntry>();
        [JsonProperty] internal List<JetStreamT242DataEntry> MarineTurbineEntries = new List<JetStreamT242DataEntry>();
    }
}
