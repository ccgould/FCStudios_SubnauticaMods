using ExStorageDepot.Enumerators;
using ExStorageDepot.Model;
using FCSCommon.Utilities;
using FCSTechFabricator.Managers;
using FCSTechFabricator.Objects;
using UnityEngine;
using UWE;

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
                return null;
            }

            QuickLogger.Debug("1");
            if (EggHandler.GetDiscoveredEgg(itemData.TechType, out TechType value))
            {
                return CraftData.InstantiateFromPrefab(value).GetComponent<Pickupable>();
            }

            QuickLogger.Debug("2");

            var gameObject = CraftData.GetPrefabForTechType(itemData.TechType);

            QuickLogger.Debug("3");
             

            if (gameObject == null)
            {
                QuickLogger.Error($"Couldn't get the prefab for the tech-type {itemData.TechType}");
                return null;
            }

            QuickLogger.Debug("4");

            var go = GameObject.Instantiate(gameObject);

            QuickLogger.Debug("5");

            if (go == null)
            {
                QuickLogger.Debug("GameObject is null in convert to pickupable", true);
                QuickLogger.Debug($"TechType = {itemData.Type}", true);
                return null;
            }

            switch (itemData.Type)
            {
                case ItemType.Battery:
                    go.GetComponent<Battery>()._charge = itemData.BatteryData.Charge;
                    break;

                case ItemType.Food:
                    var eatable = go.GetComponent<Eatable>();
                    eatable.foodValue = itemData.FoodData.FoodValue;
                    eatable.waterValue = itemData.FoodData.WaterValue;
                    break;

                case ItemType.Fuel:
                    go.GetComponent<FireExtinguisher>().fuel = itemData.FuelData.Fuel;
                    break;

                case ItemType.PlayerTool:
                    if (itemData.PlayerToolData.HasBattery)
                    {
                        var batteryTechType = itemData.PlayerToolData.BatteryInfo.TechType;
                        var tempBattery = CraftData.GetPrefabForTechType(batteryTechType);
                        var capacity = tempBattery?.gameObject.GetComponent<IBattery>()?.capacity;

                        if (itemData.PlayerToolData.HasBattery && capacity != null && capacity > 0)
                        {
                            //var pickup = CraftData.InstantiateFromPrefab(itemData.TechType).GetComponent<Pickupable>();
                            var energyMixin = go.gameObject.GetComponent<EnergyMixin>();
                            var normalizedCharge = itemData.PlayerToolData.BatteryInfo.BatteryCharge / capacity;
                            if (energyMixin.GetBattery() != null)
                            {
                                QuickLogger.Debug("Battery was already in device destroying");
                            }

                            if (!energyMixin.compatibleBatteries.Contains(batteryTechType))
                            {
                                energyMixin.compatibleBatteries.Add(batteryTechType);
                            }

                            energyMixin.SetBattery(itemData.PlayerToolData.BatteryInfo.TechType,
                                (float)normalizedCharge);
                            QuickLogger.Info($"Gave Player Player tool {itemData.PlayerToolData.TechType} with battery {batteryTechType}");
                        }
                        else
                        {
                            QuickLogger.Error("While trying to get the batter capacity of the battery it returned null or 0.");
                        }
                    }
                    break;

                case ItemType.FCSFilteredStorage:
                    //TODO FIX
                    var storage = go.GetComponent<FCSFilteredStorage>();
                    //storage.Items = itemData.FcsFilteredStorage;
                    //storage.Filters = itemData.FcsFilters;
                    storage.ForceUpdateDisplay();
                    break;
            }

            QuickLogger.Debug("6");

            return go.GetComponent<Pickupable>();
        }
    }
}
