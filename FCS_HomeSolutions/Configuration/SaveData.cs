using System;
using System.Collections.Generic;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Objects;
using Oculus.Newtonsoft.Json;

namespace FCS_HomeSolutions.Configuration
{
    [Serializable]
    internal class DecorationDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        [JsonProperty(PropertyName = "COL")] internal ColorVec4 Color { get; set; }
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
    internal class SaveData
    {
        [JsonProperty] internal float SaveVersion { get; set; } = 1.0f;
        [JsonProperty] internal List<DecorationDataEntry> DecorationEntries = new List<DecorationDataEntry>();
        [JsonProperty] internal List<PaintToolDataEntry> PaintToolEntries = new List<PaintToolDataEntry>();
    }
}
