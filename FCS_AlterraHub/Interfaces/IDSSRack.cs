using System.Collections.Generic;
using FCS_AlterraHub.Mono;

namespace FCS_AlterraHub.Interfaces
{
    public interface IDSSRack
    {
        bool IsOpen { get; }
        string UnitID { get; set; }
        BaseManager Manager { get; set; }
        FCSStorage GetStorage();
        void UpdateStorageCount();
        bool HasSpace(int amount);
        bool AddItemToRack(InventoryItem item);
        int GetFreeSpace();
        bool ItemAllowed(InventoryItem item, out ISlotController unknown);
        int GetItemCount(TechType techType);
        bool HasItem(TechType techType);
        Pickupable RemoveItemFromRack(TechType techType);
    }
}
