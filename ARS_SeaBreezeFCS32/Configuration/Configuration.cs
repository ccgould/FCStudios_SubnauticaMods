using System;
using FCSTechFabricator.Enums;
using Oculus.Newtonsoft.Json;

namespace ARS_SeaBreezeFCS32.Configuration
{
    public class ModConfiguration
    {
        private ModModes _modMode = ModModes.HardCore;
        [JsonProperty] internal string ConfigurationVersion { get; set; } = "2.0.0";
        [JsonProperty] internal int StorageLimit { get; set; } = 100;
        [JsonProperty] internal float PowerUsage { get; set; } = 0.5066666666666667f;
        [JsonProperty] internal bool UseBasePower { get; set; } = true;

        [JsonProperty]
        internal ModModes ModMode
        {
            get => _modMode;
            set
            {

                _modMode = value;
                OnModModeChanged?.Invoke(_modMode);
            }
        }

        [JsonIgnore] internal Action<ModModes> OnModModeChanged { get; set; }

    }
}