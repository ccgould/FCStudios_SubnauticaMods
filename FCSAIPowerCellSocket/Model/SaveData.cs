using System;
using System.Collections.Generic;
using FCSCommon.Objects;
using Oculus.Newtonsoft.Json;

namespace FCSAIPowerCellSocket.Model
{
    [Serializable]
    internal class SaveDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        [JsonProperty] internal IEnumerable<PowercellData> PowercellDatas { get; set; } = new List<PowercellData>();
    }

    [Serializable]
    internal class SaveData
    {
        [JsonProperty] internal List<SaveDataEntry> Entries = new List<SaveDataEntry>();
    }
}
