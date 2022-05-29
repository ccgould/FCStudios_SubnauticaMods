using System;
using System.Collections.Generic;
using FCS_AlterraHub.Mono;

namespace FCS_AlterraHub.Interfaces
{
    public interface IFCSStorage
    {
        int GetContainerFreeSpace { get; }
        bool IsFull { get; }
        bool CanBeStored(int amount, TechType techType);
        bool AddItemToContainer(InventoryItem item);
        bool IsAllowedToAdd(Pickupable pickupable, bool verbose);
        bool IsAllowedToRemoveItems();
        Pickupable RemoveItemFromContainer(TechType techType);

        //IEnumerator RemoveItemFromContainer(TechType techType, IOut<Pickupable> pickupable);

        Dictionary<TechType, int> GetItemsWithin();
        Action<int, int> OnContainerUpdate { get; set; }
        Action<FcsDevice, TechType> OnContainerAddItem { get; set; }
        Action<FcsDevice, TechType> OnContainerRemoveItem { get; set; }
        bool ContainsItem(TechType techType);
        ItemsContainer ItemsContainer { get; }
        int StorageCount();
    }

    public interface IFCSDumpContainer
    {
        bool AddItemToContainer(InventoryItem item);
        bool IsAllowedToAdd(TechType techType, bool verbose);
        bool IsAllowedToAdd(Pickupable pickupable, bool verbose);
    }
}
