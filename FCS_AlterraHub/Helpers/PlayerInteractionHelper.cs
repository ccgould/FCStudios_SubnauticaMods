using System;
using System.Collections.Generic;
using FCS_AlterraHub.Configuration;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Helpers
{
    public static class PlayerInteractionHelper
    {
        public static bool GivePlayerItem(TechType techType)
        {
            QuickLogger.Debug($"Giving Player Item: {Language.main.Get(techType)}", true);
            var size = TechDataHelpers.GetItemSize(techType);
            if (Player.main.HasInventoryRoom(size.x, size.y))
            {
                CraftData.AddToInventory(techType, 1, false, false);
                return true;
            }

            return false;
        }

        public static bool GivePlayerItem(TechType techType, int amount)
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
            GivePlayerItem(inventoryItem.item);
        }

        public static void GivePlayerItem(Pickupable pickupable)
        {
            if (pickupable == null) return;

            if (Player.main.HasInventoryRoom(pickupable))
            {
#if SUBNAUTICA_EXP
                Inventory.main.StartCoroutine(Inventory.main.PickupAsync(pickupable, new TaskResult<bool>(), false));
#else
                Inventory.main.Pickup(pickupable);
#endif
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

        public static bool TakeItemFromInventory(TechType techType, Action<Pickupable> itemCallBack = null,
            bool destroyItem = true, int amountToTake = 1)
        {
            try
            {
                if (Inventory.main.container.GetCount(techType) < amountToTake)
                {
                    return false;
                }

                for (int i = 0; i < amountToTake; i++)
                {
                    var item = Inventory.main.container.RemoveItem(techType);

                    if (destroyItem)
                    {
                        GameObject.Destroy(item.gameObject);
                    }
                    else
                    {
                        itemCallBack?.Invoke(item);
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                QuickLogger.Error("Failed to take item from player inventory");
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
                return false;
            }
        }

        public static IList<InventoryItem> GetItemsOnPlayer(TechType techType)
        {
            return Inventory.main.container.GetItems(techType);
        }
    }
}