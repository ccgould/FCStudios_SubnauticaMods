using FCS_AlterraHub.Mono;

namespace FCS_AlterraHub.Structs
{
    public struct TrackedLight
    {
        public BaseManager Manager { get; set; }
        public TechLight TechLight { get; set; }
        public BaseSpotLight SpotLight { get; set; }
    }
}