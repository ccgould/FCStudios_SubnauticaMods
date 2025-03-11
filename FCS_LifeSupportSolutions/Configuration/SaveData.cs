using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace FCS_LifeSupportSolutions.Configuration;

internal class SaveData
{
    [JsonProperty]
    public HashSet<object> Data { get; set; } = new();
}

internal class MiniMedBayDataEntry : ISaveDataEntry
{
    public string Id { get; set; }
    [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
    public ColorTemplateSave ColorTemplate { get; set; }
    public string BaseId { get; set; }
    [JsonProperty] internal int FirstAidCount { get; set; }
    [JsonProperty] internal float TimeToSpawn { get; set; }
}

internal class BaseUtilityEntry : ISaveDataEntry
{
    public string Id { get; set; }
    public string BaseId { get; set; }
    [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
    public ColorTemplateSave ColorTemplate { get; set; }
    [JsonProperty] internal float O2Level { get; set; }
}

internal class BaseOxygenTankEntry : ISaveDataEntry
{
    public string Id { get; set; }
    public string BaseId { get; set; }
    [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
    public ColorTemplateSave ColorTemplate { get; set; }
    [JsonProperty] internal float O2Level { get; set; }
    [JsonProperty] internal string ParentID { get; set; }
}

