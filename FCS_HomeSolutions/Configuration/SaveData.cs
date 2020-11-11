using System;
using System.Collections.Generic;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Objects;
using FCS_HomeSolutions.HoverLiftPad.Mono;
using Oculus.Newtonsoft.Json;
using UnityEngine;

namespace FCS_HomeSolutions.Configuration
{
    [Serializable]
    internal class DecorationDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        [JsonProperty(PropertyName = "COL")] internal ColorVec4 Color { get; set; }
        [JsonProperty(PropertyName = "SCOL")] internal ColorVec4 SecondaryColor { get; set; }
    }

    [Serializable]
    internal class PaintToolDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        [JsonProperty(PropertyName = "COL")] internal ColorVec4 Color { get; set; }
        [JsonProperty(PropertyName = "A")] internal int Amount { get; set; }
        [JsonProperty(PropertyName = "PTM")] internal ColorTargetMode ColorTargetMode { get; set; }
    }

    [Serializable]
    internal class HoverLiftDataEntry
    {
        [JsonProperty] internal string DockedPrawnID;
        [JsonProperty] internal string Id { get; set; }
        [JsonProperty] internal List<LevelData> KnownLevels { get; set; }
        [JsonProperty] internal Vec3 PadCurrentPosition { get; set; }
        [JsonProperty] internal bool FrontGatesOpen { get; set; }
        [JsonProperty] internal bool BackGatesOpen { get; set; }
        [JsonProperty(PropertyName = "COL")] internal ColorVec4 Color { get; set; }
        [JsonProperty(PropertyName = "SCOL")] internal ColorVec4 SecondaryColor { get; set; }
        [JsonProperty] internal bool PlayerLocked { get; set; }
        [JsonProperty] internal bool ExosuitLocked { get; set; }
    }

    internal class PlanterDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        [JsonProperty(PropertyName = "COL")] internal ColorVec4 Color { get; set; }
        [JsonProperty(PropertyName = "SCOL")] internal ColorVec4 SecondaryColor { get; set; }
        [JsonProperty(PropertyName = "LUMCOL")] internal ColorVec4 LUMColor { get; set; }
        [JsonProperty]  internal byte[] Bytes { get; set; }
    }


    [Serializable]
    internal class SaveData
    {
        [JsonProperty] internal float SaveVersion { get; set; } = 1.0f;
        [JsonProperty] internal List<HoverLiftDataEntry> HoverLiftDataEntries = new List<HoverLiftDataEntry>();
        [JsonProperty] internal List<DecorationDataEntry> DecorationEntries = new List<DecorationDataEntry>();
        [JsonProperty] internal List<PaintToolDataEntry> PaintToolEntries = new List<PaintToolDataEntry>();
        [JsonProperty] internal List<PlanterDataEntry> PlanterEntries = new List<PlanterDataEntry>();
    }
}
