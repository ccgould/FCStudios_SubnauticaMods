using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using FCS_AlterraHub.Configuation;
using SQLite;

namespace FCS_LifeSupportSolutions.Configuration;

internal class SaveData
{
    [JsonProperty]
    public HashSet<object> Data { get; set; } = new();
}

[Table("MiniMedBay")]
internal class MiniMedBayDataEntry : BaseSaveData
{
    [JsonProperty] internal int FirstAidCount { get; set; }
    [JsonProperty] internal float TimeToSpawn { get; set; }
}

[Table("BaseUtility")]
internal class BaseUtilityEntry : BaseSaveData
{
    [JsonProperty] internal float O2Level { get; set; }
}

[Table("BaseOxygen")]
internal class BaseOxygenTankEntry : BaseSaveData
{
    [JsonProperty] internal float O2Level { get; set; }
    [JsonProperty] internal string ParentID { get; set; }
}

