using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FCSTechFabricator.Interfaces
{
    public interface IFCSStorage
    {
       int GetContainerFreeSpace { get; }
       bool IsFull { get;}
       bool CanBeStored(int amount,TechType techType);
       bool AddItemToContainer(InventoryItem item);
       bool IsAllowedToAdd(Pickupable pickupable, bool verbose);
       Pickupable RemoveItemFromContainer(TechType techType, int amount);
       Dictionary<TechType, int> GetItemsWithin();
       Action<int, int> OnContainerUpdate { get; set; }
       bool ContainsItem(TechType techType);
    }
}
