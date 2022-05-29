using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Model;
using DataStorageSolutions.Mono;
using FCSCommon.Enums;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using FCSTechFabricator.Enums;
using FCSTechFabricator.Managers;
using FCSTechFabricator.Objects;
using UnityEngine;

namespace DataStorageSolutions.Helpers
{
    internal class DSSHelpers
    {
        private static SaveDataObjectType FindSaveDataObjectType(GameObject go)
        {
            SaveDataObjectType objectType;

            if (go.GetComponent<Eatable>())
            {
                objectType = SaveDataObjectType.Eatable;
            }
            else if (go.GetComponent<DSSServerController>()) // Must check for Server first before playertool
            {
                objectType = SaveDataObjectType.Server;
            }
            else if (go.GetComponent<PlayerTool>())
            {
                objectType = SaveDataObjectType.PlayerTool;
            }
            else if (go.GetComponent<Battery>())
            {
                objectType = SaveDataObjectType.Battery;
            }
            else
            {
                objectType = SaveDataObjectType.Item;
            }

            return objectType;
        }

        private static HashSet<ObjectData> GetServerData(InventoryItem item)
        {
            var data = item.item.GetComponent<DSSServerController>().FCSFilteredStorage.Items;
            return new HashSet<ObjectData>(data);
        }

        private static EatableEntities GetEatableData(InventoryItem item)
        {
            var eatableEntity = new EatableEntities();
            eatableEntity.Initialize(item.item, false);
            return eatableEntity;
        }

        private static PlayerToolData GetPlayerToolData(InventoryItem item)
        {
            if (item == null)
            {
                QuickLogger.Error("Failed to get Inventory Item",true);
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

        private static PlayerToolData GetBatteryData(InventoryItem item)
        {
            var batteryGo = item.item.GetComponentInChildren<Battery>();

            var playerToolData = new PlayerToolData { TechType = item.item.GetTechType() };
            var techType = batteryGo.GetComponent<TechTag>().type;
            var iBattery = batteryGo.GetComponent<IBattery>();
            playerToolData.BatteryInfo = new BatteryInfo(techType, iBattery, String.Empty);

            return playerToolData;
        }

        internal static bool GivePlayerItem(TechType techType, ObjectDataTransferData itemData,
            Func<ObjectData, RackSlot> getServerWithObjectData)
        {
            QuickLogger.Debug($"Give Player Item: {techType}", true);

            bool isSuccessful = false;

#if SUBNAUTICA
            var itemSize = CraftData.GetItemSize(techType);
#elif BELOWZERO
            var itemSize = TechData.GetItemSize(techType);
#endif
            if (Inventory.main.HasRoomFor(itemSize.x, itemSize.y))
            {
                //TODO handle null playerToolData
                Pickupable pickup;
                if (itemData.Vehicle == null)
                {
                    if (EggHandler.GetDiscoveredEgg(techType, out TechType value))
                    {
                        pickup = CraftData.InstantiateFromPrefab(value).GetComponent<Pickupable>();
                    }
                    else
                    {
                        pickup = CraftData.InstantiateFromPrefab(techType).GetComponent<Pickupable>();
                    }
                    if (!itemData.IsServer)
                    {
                        var data = (ObjectData)itemData.data;

                        if (data != null)
                        {
                            switch (data.DataObjectType)
                            {
                                case SaveDataObjectType.PlayerTool:

                                    if (data.PlayToolData.HasBattery)
                                    {
                                        var batteryTechType = data.PlayToolData.BatteryInfo.TechType;
                                        var tempBattery = CraftData.GetPrefabForTechType(batteryTechType);
                                        var capacity = tempBattery?.gameObject.GetComponent<IBattery>()?.capacity;

                                        if (data.PlayToolData.HasBattery && capacity != null && capacity > 0)
                                        {
                                            var energyMixin = pickup.gameObject.GetComponent<EnergyMixin>();
                                            var normalizedCharge = data.PlayToolData.BatteryInfo.BatteryCharge / capacity;
                                            if (energyMixin.GetBattery() != null)
                                            {
                                                QuickLogger.Debug("Battery was already in device destroying");
                                            }

                                            if (!energyMixin.compatibleBatteries.Contains(batteryTechType))
                                            {
                                                energyMixin.compatibleBatteries.Add(batteryTechType);
                                            }

                                            energyMixin.SetBattery(data.PlayToolData.BatteryInfo.TechType,
                                                (float)normalizedCharge);
                                            QuickLogger.Info(
                                                $"Gave Player Player tool {data.PlayToolData.TechType} with battery {batteryTechType}");
                                        }
                                        else
                                        {
                                            QuickLogger.Error<DSSServerController>(
                                                "While trying to get the batter capacity of the battery it returned null or 0.");
                                        }
                                    }

                                    break;



                                case SaveDataObjectType.Eatable:
                                    //We are not handling decaying items so I dont need to set anything
                                    break;

                                case SaveDataObjectType.Server:
                                    var server = pickup.gameObject.GetComponent<DSSServerController>();
                                    server.FCSFilteredStorage.Items = new HashSet<ObjectData>(data.ServerData);
                                    server.Initialize();
                                    server.DisplayManager.UpdateDisplay();
                                    break;
                                case SaveDataObjectType.Battery:
                                    var battery = pickup.gameObject.GetComponent<Battery>();
                                    battery.charge = data.PlayToolData.BatteryInfo.BatteryCharge;
                                    break;
                            }

                        }

                        var result = getServerWithObjectData?.Invoke(data);
                        result?.Remove(data);
                        isSuccessful = true;
                    }
                    else
                    {
                        var data = (HashSet<ObjectData>)itemData.data;
                        var controller = pickup.gameObject.GetComponent<DSSServerController>();
                        controller.Initialize();
                        controller.FCSFilteredStorage.Items = new HashSet<ObjectData>(data);
                        controller.FCSFilteredStorage.Filters = new List<Filter>(itemData.Filters);
                        controller.DisplayManager.UpdateDisplay();
                        isSuccessful = true;
                    }

                    Inventory.main.Pickup(pickup);
                }
                else if (itemData.Vehicle != null)
                {
                    QuickLogger.Debug("Is Vehicle Item");

                    var vehicleContainers = itemData.Vehicle.gameObject.GetComponentsInChildren<StorageContainer>()
                        .Select((x) => x.container).ToList();
                    vehicleContainers.AddRange(GetSeamothStorage(itemData.Vehicle));

                    for (var index = 0; index < vehicleContainers.Count; index++)
                    {
                        for (var i = 0; i < vehicleContainers[index].ToList().Count; i++)
                        {
                            var item = vehicleContainers[index].ToList()[i];

                            if (item.item.GetTechType() == techType)
                            {
                                var passedItem = vehicleContainers[index].RemoveItem(item.item);
                                if (passedItem)
                                {
                                    if (Inventory.main.Pickup(item.item))
                                    {
                                        CrafterLogic.NotifyCraftEnd(Player.main.gameObject, item.item.GetTechType());

                                        goto _end;
                                    }
                                }
                            }
                        }
                    }

                _end:
                    isSuccessful = true;
                }
            }

            Mod.OnBaseUpdate?.Invoke();
            return isSuccessful;
        }
        internal static ObjectData MakeObjectData(InventoryItem item, int slot)
        {
            var go = item.item.gameObject;

            var objectType = FindSaveDataObjectType(go);

            ObjectData result;

            switch (objectType)
            {
                case SaveDataObjectType.Item:
                    result = new ObjectData { DataObjectType = objectType, TechType = item.item.GetTechType() };
                    break;
                case SaveDataObjectType.PlayerTool:
                    result = new ObjectData
                    {
                        DataObjectType = objectType,
                        TechType = item.item.GetTechType(),
                        PlayToolData = GetPlayerToolData(item)
                    };
                    break;
                case SaveDataObjectType.Eatable:
                    result = new ObjectData
                    {
                        DataObjectType = objectType,
                        TechType = item.item.GetTechType(),
                        EatableEntity = GetEatableData(item)
                    };
                    break;
                case SaveDataObjectType.Server:
                    result = new ObjectData
                    {
                        DataObjectType = objectType,
                        TechType = item.item.GetTechType(),
                        ServerData = GetServerData(item)
                    };
                    break;
                case SaveDataObjectType.Battery:
                    result = new ObjectData
                    {
                        DataObjectType = objectType,
                        TechType = item.item.GetTechType(),
                        PlayToolData = GetBatteryData(item)
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return result;
        }

        internal static List<ItemsContainer> GetSeamothStorage(Vehicle seamoth)
        {
            var results = new List<ItemsContainer>();
            if (seamoth is SeaMoth && seamoth.modules != null)
            {
                using (var e = seamoth.modules.GetEquipment())
                {
                    while (e.MoveNext())
                    {
                        var module = e.Current.Value;
                        if (module == null || module.item == null)
                        {
                            continue;
                        }

                        var container = module.item.GetComponent<SeamothStorageContainer>();
                        if (container != null && !container.gameObject.name.Contains("Torpedo"))
                        {
                            results.Add(container.container);
                        }
                    }
                }
            }

            return results;
        }

        internal static List<ItemsContainer> GetVehicleContainers(Vehicle vehicle)
        {
            var vehicleContainers = vehicle?.gameObject.GetComponentsInChildren<StorageContainer>().Select((x) => x?.container)
                .ToList();
            vehicleContainers?.AddRange(GetSeamothStorage(vehicle));
            return vehicleContainers;
        }

        public static void GivePlayerItem(Pickupable item)
        {
            if (Inventory.main.HasRoomFor(item))
            {
                if (Inventory.main.Pickup(item))
                {
                    CrafterLogic.NotifyCraftEnd(Player.main.gameObject, item.GetTechType());
                }
            }
        }
    }
}