using FCS_AlterraHub.Configuation;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace FCS_StorageSolutions.Configuation;

internal class SaveData
{
    [JsonProperty]
    public HashSet<object> Data { get; set; } = new();
}

public class DSSItemDisplaySaveData: BaseSaveData
{
    public TechType CurrentTechType { get; set; }
}

//[FileName("StorageSolutions")]
//public class StorageSolutionsSaveData : SaveDataCache
//{

//}
