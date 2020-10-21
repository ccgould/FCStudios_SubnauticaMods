using FCS_AlterraHub.Enumerators;
using Oculus.Newtonsoft.Json;

namespace FCS_AlterraHub.Structs
{
    public struct CustomStoreItem
    {
        public TechType TechType { get; set; }
        public TechType ReturnItemTechType { get; set; }
        public StoreCategory Category { get; set; }
        public float Cost { get; set; }
    }
}
