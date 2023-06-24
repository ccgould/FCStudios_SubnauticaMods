using Newtonsoft.Json;
using System.Collections.Generic;

namespace FCS_StorageSolutions.Configuation;

internal class SaveData
{
    [JsonProperty]
    public HashSet<object> Data { get; set; } = new();
}

//[FileName("StorageSolutions")]
//public class StorageSolutionsSaveData : SaveDataCache
//{

//}
