﻿using System;
using System.Collections.Generic;
using FCSTechFabricator.Objects;
using GasPodCollector.Models;
using Oculus.Newtonsoft.Json;

namespace GasPodCollector.Configuration
{
    [Serializable]
    internal class SaveDataEntry
    {
        [JsonProperty] internal string ID { get; set; }
        [JsonProperty] internal int GaspodAmount { get; set; }
        [JsonProperty] internal ColorVec4 BodyColor { get; set; }
        [JsonProperty] internal Dictionary<int, BatteryInfo> Batteries { get; set; }
    }

    [Serializable]
    internal class SaveData
    {
        [JsonProperty] internal List<SaveDataEntry> Entries = new List<SaveDataEntry>();
    }
}
