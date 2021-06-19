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
            var size = CraftData.GetItemSize(techType);
            if (Player.main.HasInventoryRoom(size.x,size.y))
            {
                CraftData.AddToInventory(techType, 1, false, false);
                return true;
            }
            return false;
        }

        public static bool GivePlayerItemV2(TechType techType,int amount)
        {
            if (amount > 0)
            {
                var sizes = new List<Vector2int>();
                for (int i = 0; i < amount; i++)
                {
                    sizes.Add(CraftData.GetItemSize(techType));
                }
                
                if (Inventory.main.container.HasRoomFor(sizes))
                {
                    if (CraftData.IsAllowed(techType))
                    {
                        for (int i = 0; i < amount; i++)
                        {
                            GameObject gameObject = CraftData.InstantiateFromPrefab(techType, false);
                            if (gameObject != null)
                            {
                                gameObject.transform.position = MainCamera.camera.transform.position + MainCamera.camera.transform.forward * 3f;
                                CrafterLogic.NotifyCraftEnd(gameObject, techType);
                                Pickupable component = gameObject.GetComponent<Pickupable>();
                                if (component != null && !Inventory.main.Pickup(component, false))
                                {
                                    ErrorMessage.AddError(Language.main.Get("InventoryFull"));
                                }
                            }
                        }
                        return true;
                    }
                }
            }

            return false;
        }

#if SUBNAUTICA_STABLE
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
#else
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
#endif

        public static bool HasCard()
        {
            return Inventory.main.container.Contains(Mod.DebitCardTechType);
        }

        public static bool CanPlayerHold(TechType techType)
        {
            var size = CraftData.GetItemSize(techType);
            return Inventory.main.HasRoomFor(size.x, size.y);
        }

        public static bool HasItem(TechType techType)
        {
            return Inventory.main.container.Contains(techType);
        }
    }
}
