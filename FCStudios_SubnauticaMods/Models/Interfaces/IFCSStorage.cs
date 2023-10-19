using System;
using System.Collections.Generic;

namespace FCS_AlterraHub.Models.Interfaces;

public interface IFCSStorage
{
    int GetContainerFreeSpace { get; }
    bool IsFull { get; }
    bool CanBeStored(int amount, TechType techType);
    bool AddItemToContainer(InventoryItem item);
    bool IsAllowedToAdd(Pickupable pickupable, bool verbose);
    bool IsAllowedToRemoveItems();
    Pickupable RemoveItemFromContainer(TechType techType);
    Dictionary<TechType, int> GetItemsWithin();
    Action<int, int> OnContainerUpdate { get; set; }
    //Action<FCSDevice, TechType> OnContainerAddItem { get; set; }
    //Action<FCSDevice, TechType> OnContainerRemoveItem { get; set; }
    bool ContainsItem(TechType techType);
    ItemsContainer ItemsContainer { get; }
    int GetCount();
}

public interface IFCSDumpContainer
{
    bool AddItemToContainer(InventoryItem item);
    bool IsAllowedToAdd(TechType techType, bool verbose);
    bool IsAllowedToAdd(Pickupable pickupable, bool verbose);
    bool IsAllowedToAdd(TechType techType, int containerTotal);
}
