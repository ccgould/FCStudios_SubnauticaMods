namespace FCSAlterraShipping.Interfaces
{
    interface IContainer
    {
        bool IsFull(TechType techType);
        int NumberOfItems { get; }
        void OpenStorage();
        ItemsContainer GetContainer();
        void ClearContainer();
        void AddItem(InventoryItem item);
        void RemoveItem(Pickupable item);
        void RemoveItem(TechType item);
        bool CanFit();

    }
}
