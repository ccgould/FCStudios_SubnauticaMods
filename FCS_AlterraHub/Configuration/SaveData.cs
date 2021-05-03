using System;
using System.Collections.Generic;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.AlterraHub;
using FCS_AlterraHub.Objects;
using FCS_AlterraHub.Systems;
using FCSCommon.Interfaces;
using UnityEngine;
#if SUBNAUTICA_STABLE
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif
namespace FCS_AlterraHub.Configuration
{
    [Serializable]
    internal class OreConsumerDataEntry : ISaveDataEntry
    {
        [JsonProperty] internal float RPM;
        public string Id { get; set; }
        public string BaseId { get; set; }
        [JsonProperty(PropertyName = "COL")] internal Vec4 Fcs { get; set; }
        [JsonProperty(PropertyName = "TL")] internal float TimeLeft { get; set; }
        [JsonProperty(PropertyName = "OQ")] internal Queue<TechType> OreQueue { get; set; }
        [JsonProperty(PropertyName = "BT")] internal bool IsBreakerTripped { get; set; }
    }

    [Serializable]
    internal class AlterraHubDataEntry: ISaveDataEntry
    {
        public string Id { get; set; }
        public string BaseId { get; set; }
        [JsonProperty(PropertyName = "C")] internal IEnumerable<CartItemSaveData> CartItems { get; set; }
        [JsonProperty(PropertyName = "COL")] internal Vec4 Fcs { get; set; }
    }
    
    [Serializable]
    internal class SaveData
    {
        [JsonProperty] internal float SaveVersion { get; set; } = 1.0f;
        [JsonProperty] internal List<OreConsumerDataEntry> OreConsumerEntries = new List<OreConsumerDataEntry>();
        [JsonProperty] internal List<AlterraHubDataEntry> AlterraHubEntries = new List<AlterraHubDataEntry>();
        [JsonProperty(PropertyName = "Acc")] internal AccountDetails AccountDetails { get; set; }
        [JsonProperty] internal List<BaseSaveData> BaseSaves = new List<BaseSaveData>();
    }
}
