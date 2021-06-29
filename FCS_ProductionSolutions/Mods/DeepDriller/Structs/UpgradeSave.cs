#if SUBNAUTICA_STABLE
using Oculus.Newtonsoft.Json;

#else
using Newtonsoft.Json;
#endif

namespace FCS_ProductionSolutions.Mods.DeepDriller.Structs
{
    internal struct UpgradeSave
    {
        [JsonProperty] internal string Function { get; set; }
        [JsonProperty] internal bool IsEnabled { get; set; }
    }
}
