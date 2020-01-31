using System;
using System.Collections.Generic;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;

namespace ExStorageDepot.Configuration
{
    public class Config
    {
        [JsonProperty] internal int MaxStorage { get; set; } = 200;
    }
}