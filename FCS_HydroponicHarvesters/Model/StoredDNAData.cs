using Oculus.Newtonsoft.Json;

namespace FCS_HydroponicHarvesters.Model
{
    internal class StoredDNAData
    {
        [JsonProperty] internal int Amount { get; set; }
        [JsonProperty] internal TechType TechType { get; set; }
    }
}