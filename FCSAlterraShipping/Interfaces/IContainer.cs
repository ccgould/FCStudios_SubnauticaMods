using System;

namespace FCSAlterraShipping.Interfaces
{
    interface IContainer
    {
        bool IsFull();
        int NumberOfItems { get; }
        bool HasItems();
        void OpenStorage();
        ItemsContainer GetContainer();
        void ClearContainer();
        void AddItem(InventoryItem item);
        void RemoveItem(Pickupable item);
        void RemoveItem(TechType item);
        Action OnPDAClose { get; set; }
        bool HasRoomFor(Pickupable pickupable);
    }
}
