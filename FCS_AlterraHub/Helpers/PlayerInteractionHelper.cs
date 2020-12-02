using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Systems;
using FCSCommon.Extensions;

namespace FCS_AlterraHub.Helpers
{
    public static class PlayerInteractionHelper
    {
        public static void GivePlayerItem(TechType techType)
        {
            CraftData.AddToInventory(techType,1,false,false);
        }

        public static void GivePlayerItem(InventoryItem inventoryItem)
        {
            if (inventoryItem == null) return;
            
            if (Player.main.HasInventoryRoom(inventoryItem.item))
            {
                Inventory.main.Pickup(inventoryItem.item);
            }
        }

        public static void GivePlayerItem(Pickupable pickupable)
        {
            if (pickupable == null) return;

            if (Player.main.HasInventoryRoom(pickupable))
            {
                Inventory.main.Pickup(pickupable);
            }
        }

        public static bool HasCard()
        {
            return Inventory.main.container.Contains(Mod.DebitCardTechType);
        }
    }
}
