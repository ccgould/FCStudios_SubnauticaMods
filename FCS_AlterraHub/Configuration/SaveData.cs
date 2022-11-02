using System;
using System.Collections.Generic;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Objects;
using FCS_AlterraHub.Systems;
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
        public ColorTemplateSave ColorTemplate { get; set; }
        [JsonProperty(PropertyName = "TL")] internal float TimeLeft { get; set; }
        [JsonProperty(PropertyName = "OQ")] internal Queue<TechType> OreQueue { get; set; }
        [JsonProperty(PropertyName = "BT")] internal bool IsBreakerTripped { get; set; }
        public float SpeedMultiplyer { get; set; }
        public float PendingSpeedMultiplyer { get; set; }
        public float TargetTime { get; set; }
        public int PendingTargetTime { get; set; }
        public SpeedModes CurrentSpeedMode { get; set; } = SpeedModes.Min;
        public SpeedModes PendingSpeedMode { get; set; } = SpeedModes.Min;
    }

    internal class AlterraHubDepotEntry : ISaveDataEntry
    {
        public string Id { get; set; }
        public string BaseId { get; set; }
        public ColorTemplateSave ColorTemplate { get; set; }
        [JsonProperty(PropertyName = "Data")] internal Dictionary<TechType, int> Storage { get; set; }
    }    
    
    internal class AlterraDronePortEntry : ISaveDataEntry
    {
        public string Id { get; set; }
        public string BaseId { get; set; }
        public ColorTemplateSave ColorTemplate { get; set; }
    }

    internal class AlterraTransportDroneEntry
    {
        public string Id { get; set; }
        public int DestinationPortID { get; set; }
        public string DestinationBaseID { get; set; }
        public string DepartureBaseID { get; set; }
        public int DeparturePortID { get; set; }
        public Vec3 Position { get; set; }
        public Vec4 Rotation { get; set; }
        public string State { get; set; }
    }

    internal class AlterraHubConstructorEntry : ISaveDataEntry
    {
        public string Id { get; set; }
        public ColorTemplateSave ColorTemplate { get; set; }
        public string BaseId { get; set; }
        public IEnumerable<CartItemSaveData> PendingItems { get; set; }
        public float RequestedTime { get; set; }
        public float TotalTime { get; set; }
        public float CountDown { get; set; }
    }


    internal class PatreonStatueDataEntry : ISaveDataEntry
    {
        public string Id { get; set; }
        public ColorTemplateSave ColorTemplate { get; set; }
        public string BaseId { get; set; }

    }

    [Serializable]
    internal class FCSPDAEntry
    {
        [JsonProperty(PropertyName = "C")] internal IEnumerable<CartItemSaveData> CartItems { get; set; }
    }
    
    [Serializable]
    internal class SaveData
    {
        [JsonProperty] internal float SaveVersion { get; set; } = 1.0f;
        [JsonProperty] internal List<OreConsumerDataEntry> OreConsumerEntries = new();
        [JsonProperty] internal List<PatreonStatueDataEntry> PatreonStatueEntries = new();
        [JsonProperty] internal FCSPDAEntry FCSPDAEntry = new();
        [JsonProperty(PropertyName = "Acc")] internal AccountDetails AccountDetails { get; set; }
        [JsonProperty] internal List<AlterraHubDepotEntry> AlterraHubDepotEntries { get; set; } = new();
        [JsonProperty] internal List<AlterraDronePortEntry> AlterraDronePortEntries { get; set; } = new();
        [JsonProperty] internal List<AlterraTransportDroneEntry> AlterraTransportDroneEntries { get; set; } = new();
        [JsonProperty] internal List<AlterraHubConstructorEntry> AlterraHubConstructorEntries = new();
        [JsonProperty] internal List<BaseSaveData> BaseSaves = new();
    }
}
