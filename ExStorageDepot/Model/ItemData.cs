using ExStorageDepot.Enumerators;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using Oculus.Newtonsoft.Json.Serialization;
using System;

namespace ExStorageDepot.Model
{
    internal class ItemData
    {
        [JsonProperty] internal TechType TechType { get; set; }
        [JsonProperty] internal String PrefabId { get; set; }
        [JsonProperty] internal BatteryData BatteryData { get; set; }
        [JsonProperty] internal FoodData FoodData { get; set; }
        [JsonProperty] internal ItemType Type { get; set; }
        internal InventoryItem InventoryItem { get; set; }

        internal void ExposeInventoryData()
        {
            if (InventoryItem != null)
            {
                TechType = InventoryItem.item.GetTechType();
                PrefabId = InventoryItem.item.gameObject.GetComponent<PrefabIdentifier>()?.Id;
                var battery = InventoryItem.item.gameObject.GetComponent<Battery>();
                var food = InventoryItem.item.gameObject.GetComponent<Eatable>();
                var playerTool = InventoryItem.item.gameObject.GetComponent<EnergyMixin>();

                if (playerTool != null)
                {
                    var techTag = playerTool.GetBattery().gameObject.GetComponent<TechTag>();
                    var toolBattery = playerTool.GetBattery().gameObject.GetComponent<Battery>();
                    BatteryData = new BatteryData { Capacity = toolBattery._capacity, Charge = toolBattery._charge, TechType = techTag.type };
                    QuickLogger.Debug($"Saved player tool {InventoryItem.item.GetTechType()} with battery {techTag.type}");
                    Type = ItemType.Playertool;
                    return;

                }

                if (battery != null)
                {
                    BatteryData = new BatteryData { Capacity = battery._capacity, Charge = battery._charge };
                    QuickLogger.Debug($"Saved {TechType} battery");
                    Type = ItemType.Battery;
                    return;
                }

                if (InventoryItem.item.gameObject.GetComponent<Eatable>() != null)
                {
                    FoodData = new FoodData { FoodValue = food.foodValue, WaterValue = food.waterValue };
                    QuickLogger.Debug($"Saved {TechType} Food");
                    Type = ItemType.Food;
                    return;
                }
            }
            else
            {
                QuickLogger.Error($"InventoryItem was null when trying to set in the {nameof(ItemData)}");
            }
        }
    }
}
