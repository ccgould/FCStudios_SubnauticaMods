using ExStorageDepot.Enumerators;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using Oculus.Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using ExStorageDepot.Configuration;
using FCSTechFabricator.Enums;
using FCSTechFabricator.Objects;

namespace ExStorageDepot.Model
{
    internal class ItemData
    {
        [JsonProperty] internal TechType TechType { get; set; }
        [JsonProperty] internal String PrefabId { get; set; }
        [JsonProperty] internal BatteryData BatteryData { get; set; }
        [JsonProperty] internal FoodData FoodData { get; set; }
        [JsonProperty] internal HashSet<ObjectData> FcsFilteredStorage { get; set; }
        [JsonProperty] internal List<Filter> FcsFilters { get; set; }
        [JsonProperty] internal FuelData FuelData { get; set; }
        [JsonProperty] internal ItemType Type { get; set; }
        [JsonProperty] internal PlayerToolData PlayerToolData { get; private set; }
        internal InventoryItem InventoryItem { get; set; }

        internal void ExposeInventoryData()
        {
            if (InventoryItem != null)
            {
                TechType = InventoryItem.item.GetTechType();
                PrefabId = InventoryItem.item.gameObject.GetComponent<PrefabIdentifier>()?.Id;
                var energyMixin = InventoryItem.item.GetComponentInChildren<EnergyMixin>();
                var battery = InventoryItem.item.gameObject.GetComponent<Battery>();
                var food = InventoryItem.item.gameObject.GetComponent<Eatable>();
                var fuel = InventoryItem.item.gameObject.GetComponent<FireExtinguisher>();


                if (TechType == Mod.GetDSSServerTechType())
                {
                    QuickLogger.Debug("Is DDS Server",true);
                    Type = ItemType.FCSFilteredStorage;
                    var data = InventoryItem.item.gameObject.GetComponent<FCSFilteredStorage>();
                    FcsFilteredStorage = new HashSet<ObjectData>(data.Items.ToList().ConvertAll(obj => new ObjectData(obj)));
                    FcsFilters = data.Filters.ConvertAll(filter => new Filter(filter));
                }

                if(energyMixin != null)
                {
                    PlayerToolData = GetPlayerToolData(InventoryItem);
                    QuickLogger.Debug($"Saved {TechType} player tool");
                    Type = ItemType.PlayerTool;
                }
                
                if (fuel != null)
                {
                    FuelData = new FuelData { Fuel = fuel.fuel };
                    QuickLogger.Debug($"Saved {TechType} fire extinguisher");
                    Type = ItemType.Fuel;
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
                    FoodData = new FoodData { FoodValue = food.GetFoodValue(), WaterValue = food.GetWaterValue() };
                    QuickLogger.Debug($"Saved {TechType} Food");
                    Type = ItemType.Food;
                }
            }
            else
            {
                QuickLogger.Error($"InventoryItem was null when trying to set in the {nameof(ItemData)}");
            }
        }
        
        

        private PlayerToolData GetPlayerToolData(InventoryItem item)
        {
            if (item == null)
            {
                QuickLogger.Error("Failed to get Inventory Item", true);
                return null;
            }

            if (item.item == null)
            {
                QuickLogger.Error("Failed to get Pickupable Item", true);
                return null;
            }

            var energyMixin = item.item?.GetComponentInChildren<EnergyMixin>();

            var playerToolData = new PlayerToolData { TechType = item.item.GetTechType() };

            if (energyMixin == null) return playerToolData;

            var batteryGo = energyMixin.GetBattery().gameObject;
            var techType = batteryGo.GetComponentInChildren<TechTag>().type;
            var iBattery = batteryGo.GetComponentInChildren<Battery>();
            playerToolData.BatteryInfo = new BatteryInfo(techType, iBattery, String.Empty);

            return playerToolData;
        }
    }
}
