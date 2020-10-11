using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCSTechFabricator.Components;

namespace FCSTechFabricator.Interfaces
{
    public interface IFCSStorage
    {
       int GetContainerFreeSpace { get; }
       bool IsFull { get;}
       bool CanBeStored(int amount,TechType techType);
       bool AddItemToContainer(InventoryItem item);
       bool IsAllowedToAdd(Pickupable pickupable, bool verbose);
       bool IsAllowedToRemoveItems();
       Pickupable RemoveItemFromContainer(TechType techType, int amount);
       Dictionary<TechType, int> GetItemsWithin();
       Action<int, int> OnContainerUpdate { get; set; }
       Action<FCSConnectableDevice, TechType> OnContainerAddItem { get; set; }
       Action<FCSConnectableDevice, TechType> OnContainerRemoveItem { get; set; }
       bool ContainsItem(TechType techType);
    }
}
