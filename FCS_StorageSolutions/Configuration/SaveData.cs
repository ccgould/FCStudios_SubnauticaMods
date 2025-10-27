using FCS_AlterraHub.Configuation;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;

namespace FCS_StorageSolutions.Configuration;

internal class SaveData
{
    [JsonProperty]
    public HashSet<object> Data { get; set; } = new();
}

[Table("DSSItemDisplay")]
public class DSSItemDisplaySaveData : BaseSaveData
{
    public TechType CurrentTechType { get; set; }
}

[Table("DSSRack")]
public class DSSRackSaveData : BaseSaveData
{
    public TechType CurrentTechType { get; set; }
    public string RackDataJson { get; set; }

    [Ignore]
    public Dictionary<string, int> RackData
    {
        get => string.IsNullOrEmpty(RackDataJson)
            ? new Dictionary<string, int>()
            : JsonConvert.DeserializeObject<Dictionary<string, int>>(RackDataJson);

        set => RackDataJson = JsonConvert.SerializeObject(value);
    }

}

//[FileName("StorageSolutions")]
//public class StorageSolutionsSaveData : SaveDataCache
//{

//}
