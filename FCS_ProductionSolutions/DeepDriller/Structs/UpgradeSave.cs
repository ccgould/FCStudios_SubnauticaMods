using Oculus.Newtonsoft.Json;

namespace FCS_ProductionSolutions.DeepDriller.Structs
{
    internal struct UpgradeSave
    {
        [JsonProperty] internal string Function { get; set; }
        [JsonProperty] internal bool IsEnabled { get; set; }
    }
}
