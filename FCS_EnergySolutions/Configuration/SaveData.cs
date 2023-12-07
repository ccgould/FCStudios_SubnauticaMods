using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace FCS_EnergySolutions.Configuration;

internal class SaveData
{
    [JsonProperty]
    public HashSet<object> Data { get; set; } = new();

    public class SolarClusterSaveData : ISaveDataEntry
    {
        public string Id { get; set; }
        public string BaseId { get; set; }
        public ColorTemplateSave ColorTemplate { get; set; }
        [JsonProperty] internal float StoredPower { get; set; }
    }
}
