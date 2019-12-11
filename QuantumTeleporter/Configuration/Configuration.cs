using Oculus.Newtonsoft.Json;

namespace QuantumTeleporter.Configuration
{
    public class ModConfiguration
    {
        [JsonProperty] internal float GlobalTeleportPowerUsage { get; set; }
        [JsonProperty] internal float InternalTeleportPowerUsage { get; set; }
    }
}
