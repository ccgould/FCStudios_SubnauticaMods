using System;
using System.Collections.Generic;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem;
using FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems;
using FCS_AlterraHub.Mono;
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

    internal class AlterraHubDepotEntry : ISaveDataEntry
    {
        public string Id { get; set; }
        public string BaseId { get; set; }
        [JsonProperty(PropertyName = "COL")] internal Vec4 BodyColor { get; set; }
        [JsonProperty(PropertyName = "Data")] internal Dictionary<TechType, int> Storage { get; set; }
    }    
    
    internal class AlterraDronePortEntry : ISaveDataEntry
    {
        public string Id { get; set; }
        public string BaseId { get; set; }
        [JsonProperty(PropertyName = "COL")] internal Vec4 BodyColor { get; set; }
    }

    internal class AlterraTransportDroneEntry
    {
        public string HomePortID;
        public string Id { get; set; }
        public string DockedPortID { get; set; }
        public IEnumerable<CartItemSaveData> Cargo { get; set; }
        public string DestinationPortID { get; set; }
        public string DeparturePortID { get; set; }
        public DroneController.DroneStates DroneState { get; set; }
        public string ParentID { get; set; }
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
        [JsonProperty] internal List<OreConsumerDataEntry> OreConsumerEntries = new List<OreConsumerDataEntry>();
        [JsonProperty] internal FCSPDAEntry FCSPDAEntry = new FCSPDAEntry();
        [JsonProperty(PropertyName = "Acc")] internal AccountDetails AccountDetails { get; set; }
        [JsonProperty] internal List<AlterraHubDepotEntry> AlterraHubDepotEntries { get; set; } = new List<AlterraHubDepotEntry>();
        [JsonProperty] internal List<AlterraDronePortEntry> AlterraDronePortEntries { get; set; } = new List<AlterraDronePortEntry>();
        [JsonProperty] internal List<AlterraTransportDroneEntry> AlterraTransportDroneEntries { get; set; } = new List<AlterraTransportDroneEntry>();

        [JsonProperty] internal List<BaseSaveData> BaseSaves = new List<BaseSaveData>();
    }
}
