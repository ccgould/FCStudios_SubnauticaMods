namespace FCS_AlterraHub.Interfaces
{
    public interface ISlotController
    {
        int GetFreeSpace();
        bool IsFull { get; }
        bool AddItemToMountedServer(InventoryItem item);
        bool ItemAllowed(TechType techType);
    }
}
