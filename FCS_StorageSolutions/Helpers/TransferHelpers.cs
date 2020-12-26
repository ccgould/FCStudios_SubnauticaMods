using System.Collections.Generic;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono;
using FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Rack;
using FCSCommon.Utilities;


namespace FCS_StorageSolutions.Helpers
{
    internal static class TransferHelpers
    {
        internal static bool AddItemToRack(DSSFloorServerRackController rack, InventoryItem item, int amount)
        {
            foreach (KeyValuePair<string, DSSSlotController> slotController in rack.GetSlots())
            {
                //TODO Check filter
                if (slotController.Value.IsOccupied && slotController.Value.HasSpace(amount))
                {
                    var result = slotController.Value.AddItemMountedItem(item);

                    if (!result)
                    {
                        QuickLogger.Debug($"Failed to add item to server: {slotController.Value.GetSlotName()} in rack {rack.GetPrefabID()}", true);
                        return false;
                    }

                    QuickLogger.Debug($"Added item to server: {slotController.Value.GetSlotName()} in rack {rack.GetPrefabID()}", true);
                    return true;
                }
            }

            return false;
        }

        public static bool AddItemToRack(DSSWallServerRackController rack, InventoryItem item, int amount)
        {
            foreach (KeyValuePair<string, DSSSlotController> slotController in rack.GetSlots())
            {
                //TODO Check filter
                if (slotController.Value.IsOccupied && slotController.Value.HasSpace(amount))
                {
                    var result = slotController.Value.AddItemMountedItem(item);

                    if (!result)
                    {
                        QuickLogger.Debug($"Failed to add item to server: {slotController.Value.GetSlotName()} in rack {rack.GetPrefabID()}", true);
                        return false;
                    }

                    QuickLogger.Debug($"Added item to server: {slotController.Value.GetSlotName()} in rack {rack.GetPrefabID()}", true);
                    return true;
                }
            }

            return false;
        }

        internal static bool AddItemToNetwork(InventoryItem item, BaseManager manager)
        {
            if (manager != null)
            {
                foreach (IDSSRack baseRack in manager.BaseRacks)
                {
                    if (baseRack.ItemAllowed(item, out var server))
                    {
                        server?.AddItemMountedItem(item);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
