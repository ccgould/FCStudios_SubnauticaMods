using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace FCS_ProductionSolutions.Configuration
{
    internal class SaveData
    {
        [JsonProperty]
        public HashSet<object> Data { get; set; } = new ();

        [JsonObject]
        internal class CubeGeneratorSaveData : ISaveDataEntry
        {
            [JsonProperty]
            public string Id { get; set; }
            [JsonProperty]
            public string BaseId { get; set; }
            [JsonProperty]
            public ColorTemplateSave ColorTemplate { get; set; }
        }
    }
}
