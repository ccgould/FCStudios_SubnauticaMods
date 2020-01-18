using Oculus.Newtonsoft.Json;

namespace QuantumTeleporter.Configuration
{
    public class ModConfiguration
    {
        [JsonProperty] internal float GlobalTeleportPowerUsage { get; set; } = 1500f;
        [JsonProperty] internal float InternalTeleportPowerUsage { get; set; } = 150f;
    }
}
