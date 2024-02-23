using FCS_AlterraHub.Configuation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace FCS_StorageSolutions.Configuration;

internal class SaveData
{
    [JsonProperty]
    public HashSet<object> Data { get; set; } = new();
}

public class DSSItemDisplaySaveData : BaseSaveData
{
    public TechType CurrentTechType { get; set; }
}

public class DSSRackSaveData : BaseSaveData
{
    public TechType CurrentTechType { get; set; }
    public Dictionary<string, int> RackData { get; set; }
}

//[FileName("StorageSolutions")]
//public class StorageSolutionsSaveData : SaveDataCache
//{

//}
