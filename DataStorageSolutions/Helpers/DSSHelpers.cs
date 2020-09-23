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
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using UnityEngine;

namespace DataStorageSolutions.Helpers
{
    //TODO REDO Class
    internal static class DSSHelpers
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
            //var data = item.item.GetComponent<DSSServerController>().FCSFilteredStorage.Items;
            //TODO FIX
            //return new HashSet<ObjectData>(data);
            return new HashSet<ObjectData>();
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
                QuickLogger.Error("Failed to get Inventory Item", true);
                return null;
            }

            if (item.item == null)
            {
                QuickLogger.Error("Failed to get Pickupable Item", true);
                return null;
            }

            var energyMixin = item.item?.GetComponentInChildren<EnergyMixin>();
            
            QuickLogger.Debug("GetPlayerToolData : 1");

            var playerToolData = new PlayerToolData {TechType = item.item.GetTechType()};

            QuickLogger.Debug("GetPlayerToolData : 2");


            if (energyMixin == null) return playerToolData;

            QuickLogger.Debug("GetPlayerToolData : 3");


            var batteryGo = energyMixin.GetBattery()?.gameObject;
            
            QuickLogger.Debug("GetPlayerToolData : 4");


            var techType = batteryGo?.GetComponentInChildren<TechTag>().type ?? TechType.None;

            QuickLogger.Debug("GetPlayerToolData : 5");


            var iBattery = batteryGo?.GetComponentInChildren<Battery>();
            
            QuickLogger.Debug("GetPlayerToolData : 6");


            if (techType != TechType.None)
            {
                QuickLogger.Debug("GetPlayerToolData : 7");

                playerToolData.BatteryInfo = new BatteryInfo(techType, iBattery, String.Empty);

                QuickLogger.Debug("GetPlayerToolData : 8");

            }

            return playerToolData;
        }

        private static PlayerToolData GetBatteryData(InventoryItem item)
        {
            var batteryGo = item.item.GetComponentInChildren<Battery>();

            var playerToolData = new PlayerToolData {TechType = item.item.GetTechType()};
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
                if (itemData.Vehicle == null)
                {
                    var pickup = CheckIfEggAnExtractPickable(techType);

                    if (!itemData.IsServer)
                    {
                        var data = (ObjectData) itemData.data;

                        if (data != null)
                        {
                            DetectDataObjectTypeAndPerformConversion(data, pickup);

                            var result = getServerWithObjectData?.Invoke(data);
                            result?.Remove(data);
                            isSuccessful = true;
                        }
                    }
                    else
                    {
                        var data = (HashSet<ObjectData>) itemData.data;
                        var controller = pickup.gameObject.GetComponent<DSSServerController>();
                        controller.Initialize();
                        
                            //TODO FIX
                        //controller.FCSFilteredStorage.Items = new HashSet<ObjectData>(data);
                        //controller.FCSFilteredStorage.Filters = new List<Filter>(itemData.Filters);
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

        private static void DetectDataObjectTypeAndPerformConversion(ObjectData data, Pickupable pickup)
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
                                (float) normalizedCharge);
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

                    //TODO FIX
                    //server.FCSFilteredStorage.Items = new HashSet<ObjectData>(data.ServerData);
                    server.Initialize();
                    server.DisplayManager.UpdateDisplay();
                    break;
                case SaveDataObjectType.Battery:
                    var battery = pickup.gameObject.GetComponent<Battery>();
                    battery.charge = data.PlayToolData.BatteryInfo.BatteryCharge;
                    break;
            }
        }

        private static Pickupable CheckIfEggAnExtractPickable(TechType techType)
        {
            Pickupable pickup;
            if (EggHandler.GetDiscoveredEgg(techType, out TechType value))
            {
                pickup = CraftData.InstantiateFromPrefab(value).EnsureComponent<Pickupable>();
            }
            else
            {
                pickup = CraftData.InstantiateFromPrefab(techType).EnsureComponent<Pickupable>();
            }

            return pickup;
        }

        internal static ObjectData MakeObjectData(InventoryItem item, int slot)
        {
            QuickLogger.Debug($"In Make Object");
            var go = item?.item?.gameObject;
            if (go == null) return new ObjectData{DataObjectType = SaveDataObjectType.Item, TechType = TechType.None};

            QuickLogger.Debug($"GameObject {go}");

            var objectType = FindSaveDataObjectType(go);
            QuickLogger.Debug($"Object Type {objectType}");

            var techType = item.item.GetTechType();
            QuickLogger.Debug($"Object Tech Type {techType}");


            ObjectData result;

            switch (objectType)
            {
                case SaveDataObjectType.Item:
                    result = new ObjectData {DataObjectType = objectType, TechType = item.item.GetTechType()};
                    break;
                case SaveDataObjectType.PlayerTool:
                    result = new ObjectData
                    {
                        DataObjectType = objectType,
                        TechType = techType,
                        PlayToolData = GetPlayerToolData(item)
                    };
                    break;
                case SaveDataObjectType.Eatable:
                    result = new ObjectData
                    {
                        DataObjectType = objectType,
                        TechType = techType,
                        EatableEntity = GetEatableData(item)
                    };
                    break;
                case SaveDataObjectType.Server:
                    result = new ObjectData
                    {
                        DataObjectType = objectType,
                        TechType = techType,
                        ServerData = GetServerData(item)
                    };
                    break;
                case SaveDataObjectType.Battery:
                    result = new ObjectData
                    {
                        DataObjectType = objectType,
                        TechType = techType,
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
            var vehicleContainers = vehicle?.gameObject.GetComponentsInChildren<StorageContainer>()
                .Select((x) => x?.container)
                .ToList();
            vehicleContainers?.AddRange(GetSeamothStorage(vehicle));
            return vehicleContainers;
        }

        public static bool GivePlayerItem(Pickupable pickupable)
        {
            if (pickupable == null)
            {
                //TODO put message on screen
                return false;
            }
            if (!Inventory.main.HasRoomFor(pickupable)) return false;

            if (!Inventory.main.Pickup(pickupable)) return false;

            CrafterLogic.NotifyCraftEnd(Player.main.gameObject, pickupable.GetTechType());
            return true;
        }

        public static Pickupable ToPickable(this ObjectData data)
        {
            var pickup = CheckIfEggAnExtractPickable(data.TechType);

            if (data.DataObjectType != SaveDataObjectType.Server)
            {
                DetectDataObjectTypeAndPerformConversion(data, pickup);

            }
            else
            {
                var controller = pickup.gameObject.GetComponent<DSSServerController>();
                controller.Initialize();

                //TODO FIX
                //controller.FCSFilteredStorage.Items = new HashSet<ObjectData>(data.ServerData);
                //controller.FCSFilteredStorage.Filters = new List<Filter>(data.Filters);
            }

            return pickup;
        }

        internal static Pickupable ToPickable(this ObjectDataTransferData itemData, TechType techType)
        {
            var pickup = CheckIfEggAnExtractPickable(techType);
            if (itemData.Vehicle == null)
            {
                if (!itemData.IsServer)
                {
                    var data = (ObjectData) itemData.data;

                    if (data != null)
                    {
                        DetectDataObjectTypeAndPerformConversion(data, pickup);
                    }
                }
                else
                {
                    var data = (HashSet<ObjectData>) itemData.data;
                    var controller = pickup.gameObject.GetComponent<DSSServerController>();
                    controller.Initialize();
                    //TODO FIX
                    // controller.FCSFilteredStorage.Items = new HashSet<ObjectData>(data);
                    //controller.FCSFilteredStorage.Filters = new List<Filter>(itemData.Filters);
                }
            }

            return pickup;
        }

        internal static string GetTechDataString(TechType techType)
        {
            var techData = CraftDataHandler.GetTechData(techType);

            var sb = new StringBuilder();
            sb.Append($"Craft {Language.main.Get(techType)} Ingredients:");
            sb.Append(Environment.NewLine);

            foreach (Ingredient ingredient in techData.Ingredients)
            {
                sb.Append(Language.main.Get(ingredient.techType));
                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        internal static TechData CheckIfTechDataAvailable(FCSOperation craft, out bool pass)
        {
            var techData = CraftDataHandler.GetTechData(craft.TechType);

            pass = true;

            if (techData != null)
            {
                foreach (Ingredient ingredient in techData.Ingredients)
                {
                    //if (craft.Manager.GetItemCount(ingredient.techType) < ingredient.amount)
                    //{
                    //    pass = false;
                    //    break;
                    //}
                }
            }

            return techData;
        }

        internal static bool CheckIfTechDataAvailable(TechType craft)
        {
            return CraftDataHandler.GetTechData(craft) != null;
        }
    }
}