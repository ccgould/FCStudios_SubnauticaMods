using System.Collections.Generic;

namespace FCS_AlterraHub.Mono
{
    public class TrackedResource
    {
        public TechType TechType { get; set; }
        public int Amount { get; set; }
        public HashSet<StorageContainer> StorageContainers { get; set; } = new HashSet<StorageContainer>();
        public HashSet<FcsDevice> Servers { get; set; } = new HashSet<FcsDevice>();
        public HashSet<FcsDevice> OtherStorage { get; set; } = new HashSet<FcsDevice>();
    }
}