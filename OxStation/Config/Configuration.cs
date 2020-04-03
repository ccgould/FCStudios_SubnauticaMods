using Oculus.Newtonsoft.Json;

namespace MAC.OxStation.Config
{
    internal class Configuration
    {
        [JsonProperty] internal bool PlaySFX { get; set; } = true;
        [JsonProperty] internal float TankCapacity { get; set; } = 75f;
        [JsonProperty] internal float OxygenPerSecond { get; set; } = 4;
        [JsonProperty] internal float EnergyPerSec { get; set; } = 0.5066666666666667f; //0.102f was the orignal;
        [JsonProperty] internal bool DamageOverTime { get; set; } = true;
    }
}