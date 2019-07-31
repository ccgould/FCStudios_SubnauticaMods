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
        internal InventoryItem InventoryItem { get; set; }

        internal void ExposeInventoryData()
        {
            if (InventoryItem != null)
            {
                TechType = InventoryItem.item.GetTechType();
                PrefabId = InventoryItem.item.gameObject.GetComponent<PrefabIdentifier>()?.Id;
            }
            else
            {
                QuickLogger.Error($"InventoryItem was null when trying to set in the {nameof(ItemData)}");
            }
        }

        internal void SaveData()
        {
            TechType = InventoryItem.item.GetTechType();
            PrefabId = InventoryItem.item.gameObject.GetComponent<PrefabIdentifier>()?.Id;
            var battery = InventoryItem.item.gameObject.GetComponent<Battery>();
            var food = InventoryItem.item.gameObject.GetComponent<Eatable>();

            if (battery != null)
            {
                BatteryData = new BatteryData { Capacity = battery._capacity, Charge = battery._charge };
                return;
            }

            if (InventoryItem.item.gameObject.GetComponent<Eatable>() != null)
            {
                FoodData = new FoodData { FoodValue = food.foodValue, WaterValue = food.waterValue };
                return;
            }
        }
    }
}
