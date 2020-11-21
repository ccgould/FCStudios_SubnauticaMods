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
            if (techType == TechType.None) return;

#if SUBNAUTICA
            var itemSize = CraftData.GetItemSize(techType);
#elif BELOWZERO
            var itemSize = TechData.GetItemSize(techType);
#endif
           
            if (Player.main.HasInventoryRoom(itemSize.x, itemSize.y))
            {
                var item = techType.ToPickupable();
                if (item != null)
                {
                    Inventory.main.Pickup(item);
                }
                else
                {
                    MessageBoxHandler.main.Show($"[GivePlayerItem]:{AlterraHub.ErrorHasOccured()}");
                }

            }
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
