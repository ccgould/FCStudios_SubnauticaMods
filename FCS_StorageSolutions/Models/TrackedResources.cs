using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Mono;
using System.Collections.Generic;

namespace FCS_StorageSolutions.Models;
public class TrackedResource
{
    public TechType TechType { get; set; }
    public int Amount { get; set; }
    public HashSet<StorageContainer> StorageContainers { get; set; } = new HashSet<StorageContainer>();
    public HashSet<FCSStorage> Servers { get; set; } = new HashSet<FCSStorage>();
    public HashSet<FCSDevice> OtherStorage { get; set; } = new HashSet<FCSDevice>();
}
