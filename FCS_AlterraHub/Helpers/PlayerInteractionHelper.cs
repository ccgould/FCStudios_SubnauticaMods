using System.Collections.Generic;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Extensions;
using FCSCommon.Utilities;
using UnityEngine;
using UWE;

namespace FCS_AlterraHub.Helpers
{
    public static class PlayerInteractionHelper
    {
        public static bool GivePlayerItem(TechType techType)
        {
            QuickLogger.Debug($"Giving Player Item: {Language.main.Get(techType)}",true);
            var size = TechDataHelpers.GetItemSize(techType);
            if (Player.main.HasInventoryRoom(size.x,size.y))
            {
                CraftData.AddToInventory(techType, 1, false, false);
                return true;
            }
            return false;
        }

        public static bool GivePlayerItem(TechType techType,int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                QuickLogger.Debug($"Giving Player Item: {Language.main.Get(techType)}", true);
                var size = TechDataHelpers.GetItemSize(techType);
                if (Player.main.HasInventoryRoom(size.x, size.y))
                {
                    CraftData.AddToInventory(techType, 1, false, false);
                }
                else
                {
                    return false;
                }
            }

            return true;
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

        public static bool CanPlayerHold(TechType techType)
        {
            var size = TechDataHelpers.GetItemSize(techType);
            return Inventory.main.HasRoomFor(size.x, size.y);
        }

        public static bool HasItem(TechType techType)
        {
            return Inventory.main.container.Contains(techType);
        }
    }
}
