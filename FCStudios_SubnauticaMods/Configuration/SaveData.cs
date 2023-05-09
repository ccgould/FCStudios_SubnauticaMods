using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Structs;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace FCS_AlterraHub.Configuation
{
    internal class SaveData
    {
        [JsonProperty]
        public HashSet<object> Data { get; set; } = new();
        public GSPSaveData GamePlayService { get; set; } = new();
        public AccountDetails AccountDetails { get;  set; }
        public StoreManagerSaveData StoreManagerSaveData { get; set; }

        public class PDASaveData : ISaveDataEntry
        {
            public string Id { get; set; }
            public string BaseId { get; set; }
            public ColorTemplateSave ColorTemplate { get; set; }

        }
    }
}
