using ExStorageDepot.Enumerators;
using ExStorageDepot.Model;
using FCSCommon.Utilities;
using UnityEngine;
namespace ExStorageDepot.Helpers
{
    internal static class InventoryHelpers
    {
        internal static ItemData CovertToItemData(InventoryItem item, bool destroy = false)
        {
            var data = new ItemData();
            data.InventoryItem = item;
            data.ExposeInventoryData();

            if (destroy)
            {
                GameObject.Destroy(item.item.gameObject);
            }

            return data;
        }

        internal static Pickupable ConvertToPickupable(ItemData itemData)
        {


            QuickLogger.Debug("ConvertToPickupable", true);


            if (itemData == null)
            {
                QuickLogger.Debug("Item Data is null", true);
            }

            QuickLogger.Debug("1", true);


            var go = GameObject.Instantiate(CraftData.GetPrefabForTechType(itemData.TechType));
            QuickLogger.Debug("2", true);

            if (go == null)
            {
                QuickLogger.Debug("GameObject is null in ConverToPickupable", true);
                QuickLogger.Debug($"TechType = {itemData.Type}", true);

            }
            QuickLogger.Debug("3", true);

            switch (itemData.Type)
            {
                case ItemType.Battery:
                    QuickLogger.Debug("4", true);
                    go.GetComponent<Battery>()._charge = itemData.BatteryData.Charge;
                    QuickLogger.Debug("5", true);

                    break;

                case ItemType.Food:
                    QuickLogger.Debug("5", true);
                    var eatable = go.GetComponent<Eatable>();
                    QuickLogger.Debug("5.1", true);
                    eatable.foodValue = itemData.FoodData.FoodValue;
                    QuickLogger.Debug("5.2", true);
                    eatable.waterValue = itemData.FoodData.WaterValue;
                    QuickLogger.Debug("6", true);

                    break;

                case ItemType.Fuel:
                    QuickLogger.Debug("7", true);
                    go.GetComponent<FireExtinguisher>().fuel = itemData.FuelData.Fuel;
                    QuickLogger.Debug("8", true);

                    break;
            }
            return go.GetComponent<Pickupable>();
        }
    }
}
