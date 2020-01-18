using System;
using System.Collections.Generic;
using FCSCommon.Objects;
using FCSTechFabricator.Objects;
using Oculus.Newtonsoft.Json;

namespace AMMiniMedBay.Configuration
{
    [Serializable]
    internal class SaveDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        [JsonProperty] internal int SCA { get; set; }
        [JsonProperty] internal float TTS { get; set; }
        [JsonProperty] internal ColorVec4 BodyColor { get; set; }

    }

    [Serializable]
    internal class SaveData
    {
        [JsonProperty] internal List<SaveDataEntry> Entries = new List<SaveDataEntry>();
    }
}
