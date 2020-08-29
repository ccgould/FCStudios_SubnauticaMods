using FCSCommon.Enums;
using Oculus.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using FCS_DeepDriller.Managers;
using FCS_DeepDriller.Structs;
using FCSCommon.Objects;
using FCSTechFabricator.Enums;
using FCSTechFabricator.Objects;

namespace FCS_DeepDriller.Configuration
{
    [Serializable]
    internal class DeepDrillerSaveDataEntry
    {
        [JsonProperty] internal float Health { get; set; }

        [JsonProperty] internal string Id { get; set; }

        [JsonProperty] internal FCSPowerStates PowerState { get; set; }

        [JsonProperty] internal Dictionary<TechType, int> Items { get; set; }

        [JsonProperty] internal DeepDrillerPowerData PowerData { get; set; }
        
        [JsonProperty] internal HashSet<TechType> FocusOres { get; set; }

        [JsonProperty] internal bool IsFocused { get; set; }
        
        [JsonProperty] internal string Biome { get; set; }
        
        [JsonProperty] internal float OilTimeLeft { get; set; }
        
        [JsonProperty] internal bool SolarExtended { get; set; }
        
        [JsonProperty] internal ColorVec4 BodyColor { get; set; }
        
        [JsonProperty] internal bool PullFromRelay { get; set; }
        [JsonProperty] internal IEnumerable<UpgradeSave> Upgrades { get; set; }
        [JsonProperty] internal bool IsRangeVisible { get; set; }
        [JsonProperty] internal bool AllowedToExport { get; set; }
    }

    [Serializable]
    internal class DeepDrillerSaveData
    {
        [JsonProperty] internal float SaveVersion { get; set; } = 1.0f;
        [JsonProperty] internal List<DeepDrillerSaveDataEntry> Entries = new List<DeepDrillerSaveDataEntry>();
    }
}
