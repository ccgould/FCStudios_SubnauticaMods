using FCS_AlterraHub.Enumerators;
using Oculus.Newtonsoft.Json;

namespace FCS_AlterraHub.Structs
{
    public struct CustomStoreItem
    {
        [JsonProperty] internal TechType TechType { get; set; }
        [JsonProperty] internal TechType ReturnItemTechType { get; set; }
        [JsonProperty] internal StoreCategory Category { get; set; }
        [JsonProperty] internal float Cost { get; set; }
    }
}
