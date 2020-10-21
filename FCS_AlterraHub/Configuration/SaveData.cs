using System;
using System.Collections.Generic;
using FCS_AlterraHub.Mono.AlterraHub;
using FCS_AlterraHub.Objects;
using FCS_AlterraHub.Systems;
using Oculus.Newtonsoft.Json;
using UnityEngine;

namespace FCS_AlterraHub.Configuration
{
    [Serializable]
    internal class OreConsumerDataEntry
    {
        [JsonProperty] internal float RPM;
        [JsonProperty] internal string Id { get; set; }
        [JsonProperty(PropertyName = "OC")] internal float OreConsumerCash { get; set; }
        [JsonProperty(PropertyName = "COL")] internal ColorVec4 Color { get; set; }
    }

    [Serializable]
    internal class AlterraHubDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        [JsonProperty(PropertyName = "C")] internal IEnumerable<CartItemSaveData> CartItems { get; set; }
        [JsonProperty(PropertyName = "COL")] internal ColorVec4 Color { get; set; }
    }
    
    [Serializable]
    public class SaveData
    {
        [JsonProperty] internal float SaveVersion { get; set; } = 1.0f;
        [JsonProperty] internal List<OreConsumerDataEntry> OreConsumerEntries = new List<OreConsumerDataEntry>();
        [JsonProperty] internal List<AlterraHubDataEntry> AlterraHubEntries = new List<AlterraHubDataEntry>();
        [JsonProperty(PropertyName = "Acc")] internal AccountDetails AccountDetails { get; set; }
    }
}
