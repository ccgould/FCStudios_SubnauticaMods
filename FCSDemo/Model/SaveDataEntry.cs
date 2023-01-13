using System;
using System.Collections.Generic;
using FCS_AlterraHub.Model;
using Newtonsoft.Json;

namespace FCSDemo.Model
{
    [Serializable]
    internal class SaveDataEntry
    {
        [JsonProperty] internal string ID { get; set; }
        [JsonProperty] internal ColorTemplateSave ColorTemplate { get; set; }
    }

    [Serializable]
    internal class SaveData
    {
        [JsonProperty] internal List<SaveDataEntry> Entries = new List<SaveDataEntry>();
    }
}
