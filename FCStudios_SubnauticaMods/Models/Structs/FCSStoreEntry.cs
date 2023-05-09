using FCS_AlterraHub.Models.Enumerators;

namespace FCS_AlterraHub.Models.Structs;

public struct FCSStoreEntry
{
    public TechType TechType { get; set; }
    public decimal Cost { get; set; }
    public StoreCategory StoreCategory { get; set; }
    public TechType ReceiveTechType { get; set; }
    public int ReturnAmount { get; set; }
    public bool ForcedUnlock { get; set; }
}
