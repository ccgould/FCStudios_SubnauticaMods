using System.Collections.Generic;
using FCS_AlterraHub.Mono;

namespace FCS_AlterraHub.Model
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