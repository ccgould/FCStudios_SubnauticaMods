using FCS_AlterraHub.Configuration;
using UWE;

namespace FCS_AlterraHub.Helpers
{
    public static class PlayerInteractionHelper
    {
        public static bool GivePlayerItem(TechType techType)
        {
            var size = CraftData.GetItemSize(techType);
            if (Player.main.HasInventoryRoom(size.x,size.y))
            {
                CraftData.AddToInventory(techType, 1, false, false);
                return true;
            }
            return false;
        }
        
        public static void GivePlayerItem(InventoryItem inventoryItem)
        {
            if (inventoryItem == null) return;
            
            if (Player.main.HasInventoryRoom(inventoryItem.item))
            {
                TaskResult<bool> pickupResult = new TaskResult<bool>();
                CoroutineHost.StartCoroutine(Inventory.main.PickupAsync(inventoryItem.item, pickupResult));
            }
        }

        public static void GivePlayerItem(Pickupable pickupable)
        {
            if (pickupable == null) return;

            if (Player.main.HasInventoryRoom(pickupable))
            {
                TaskResult<bool> pickupResult = new TaskResult<bool>();
                CoroutineHost.StartCoroutine(Inventory.main.PickupAsync(pickupable, pickupResult));
            }
        }

        public static bool HasCard()
        {
            return Inventory.main.container.Contains(Mod.DebitCardTechType);
        }

        public static bool CanPlayerHold(TechType techType)
        {
            var size = CraftData.GetItemSize(techType);
            return Inventory.main.HasRoomFor(size.x, size.y);
        }
    }
}
