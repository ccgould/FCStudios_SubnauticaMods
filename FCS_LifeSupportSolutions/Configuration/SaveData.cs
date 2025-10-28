
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
    internal int FirstAidCount { get; set; }
    internal float TimeToSpawn { get; set; }
}

[Table("BaseUtility")]
internal class BaseUtilityEntry : BaseSaveData
{
    internal float O2Level { get; set; }
}

[Table("BaseOxygen")]
internal class BaseOxygenTankEntry : BaseSaveData
{
    internal float O2Level { get; set; }
    internal string ParentID { get; set; }
}

