using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using FCS_HomeSolutions.ModItems.Buildables.UniversalCharger.Enumerators;

namespace FCS_HomeSolutions.Configuration;

internal class SaveData
{
    [JsonProperty]
    public HashSet<object> Data { get; set; } = new();
}

internal class UniversalChargerDataEntry : ISaveDataEntry
{
    public string Id { get; set; }
    public string BaseId { get; set; }
    public ColorTemplateSave ColorTemplate { get; set; }
    [JsonProperty] internal Dictionary<string, string> BatteryData { get; set; }
    [JsonProperty] internal Dictionary<string, string> ChargerData { get; set; }
    [JsonProperty] internal PowerChargerMode Mode { get; set; }
}

