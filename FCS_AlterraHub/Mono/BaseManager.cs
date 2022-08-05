﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Managers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem;
using FCS_AlterraHub.Mono.Controllers;
using FCS_AlterraHub.Patches;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Structs;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using UnityEngine;
using UWE;
using Math = System.Math;

namespace FCS_AlterraHub.Mono
{
    /*
     * The Base manager class handles all connections to bases and power demands for the connected devices
     * Rules:
     * Should provide power usage information
     * Should Provide devices
     * Should allow communication between devices
     * 
     */
    public class BaseManager : IFCSDumpContainer
    {
        private bool HasBreakerTripped
        {
            get => _hasBreakerTripped;
            set
            {
                _hasBreakerTripped = value;
                OnBreakerStateChanged?.Invoke(value);
            }
        }
        private string _baseName;
        private readonly Dictionary<string, FcsDevice> _registeredDevices;
        private float _timeLeft = 1f;
        private PowerSystem.Status _prevPowerState;
        private Dictionary<string, TechLight> _baseTechLights;
        private DumpContainerSimplified _dumpContainer;
        private Dictionary<TechType, int> _item = new();
        private BaseSaveData _savedData;
        private bool _hasBreakerTripped;
        private Dictionary<string, BaseOperationObject> _baseOperationObjects = new();
        private List<BaseTransferOperation> _baseOperations  = new();
        private BaseHullStrength _baseHullStrength;
        public string BaseID { get; set; }
        public float ActiveBaseOxygenTankCount = 0;
        public readonly Dictionary<TechType, TrackedResource> TrackedResources = new();
        public static List<string> DONT_TRACK_GAMEOBJECTS { get; private set; } = new()
        {
            "planterpot",
            "planterbox",
            "plantershelf",
            "alongplanter",
            "basicexteriorplantpot",
            "chicexteriorplantpot",
            "compositeexteriorplantpot",
            "farmingtray",
            "longplanterb",
            "labtrashcan",
            "trashcan",
            "serverrack",
            "powerstorage",
            "recycler",
            "alterrahubdepot",
            "neonplanter",
            "autocrafter",
            "fcsstove",
            "fcsmicrowave",
            "fcsCuringCabinet",
            "maproomupgrades",
            "planter"
        };
        public readonly HashSet<StorageContainer> BaseStorageLockers = new();
        public readonly HashSet<FcsDevice> BaseServers = new();
        public readonly HashSet<FcsDevice> BaseFcsStorage = new();
        public readonly HashSet<FcsDevice> BaseAntennas = new();
        public readonly HashSet<FcsDevice> BaseTerminals = new();
        private PortManager _portManager;
        public SubRoot Habitat { get; set; }
        public static Action<BaseManager> OnManagerCreated { get; set; }
        public bool IsVisible => GetIsVisible();
        public static List<BaseManager> Managers { get; } = new();
        internal NameController NameController { get; private set; }
        public static Dictionary<string, TrackedLight> GlobalTrackedLights { get; } = new();
        public Action<PowerSystem.Status> OnPowerStateChanged { get; set; }
        public List<IDSSRack> BaseRacks { get; set; } = new();
        public DSSVehicleDockingManager DockingManager { get; set; }
        public bool PullFromDockedVehicles { get; set; }
        public List<TechType> DockingBlackList { get; set; } = new();
        public Action<bool> OnBreakerStateChanged { get; set; }
        public static TechType ActivateGoalTechType { get; set; }
        public Base BaseComponent { get; set; }
        public string BaseFriendlyID { get; set; }


        #region Default Constructor

        public BaseManager(SubRoot habitat)
        {
            if (habitat == null) return;
            Habitat = habitat;
            
            BaseComponent = Habitat.GetComponent<Base>();
            _baseHullStrength = Habitat.GetComponent<BaseHullStrength>();
            BaseID = habitat.gameObject.gameObject?.GetComponentInChildren<PrefabIdentifier>()?.Id;

            FCSAlterraHubService.PublicAPI.RegisterBase(this);
            
            _registeredDevices = new Dictionary<string, FcsDevice>();
            _baseTechLights = new Dictionary<string, TechLight>();
            Initialize(habitat);
            Player_Patches.OnPlayerUpdate += Update;

        }

        private void Update()
        {
            _timeLeft -= DayNightCycle.main.deltaTime;
            
            if (_timeLeft <= 0)
            {
                PowerConsumption();
                PerformOperations();
                _timeLeft = 1f;
            }
            
            PowerStateCheck();
        }

        private void PerformOperations()
        {
            if (_registeredDevices == null || _baseOperationObjects == null || !_baseOperationObjects.Any())
            {
                if (_baseOperationObjects == null)
                {
                    _baseOperationObjects = new Dictionary<string, BaseOperationObject>();
                }


                return;
            }

            foreach (BaseTransferOperation operation in _baseOperations)
            {
                if (operation.Device == null || !operation.Device.IsConstructed || !operation.Device.IsInitialized || !operation.IsEnabled /*|| !operation.Device.AllowsTransceiverPulling*/) continue;

                if (operation.IsPullOperation)
                {
                    PerformPullOperation(operation);
                }

                foreach (TechType item in operation.TransferItems)
                {
                    if (HasItem(item))
                    {
                        if (operation.PullWhenAmountIsAbove > 0)
                        {
                            if(GetItemCount(item) <= operation.PullWhenAmountIsAbove) continue;
                        }

                        QuickLogger.Debug($"Device {operation.DeviceId}: {operation.Device.GetItemsContainer()?.count}|{operation.MaxAmount}", true);
                        if (operation.Device.CanBeStored(1, item) && operation.Device.GetItemsContainer() != null && operation.Device.GetItemsContainer().count < operation.MaxAmount)
                        {
#if SUBNAUTICA_STABLE
                            var result = TakeItem(item).Pickup(false).GetTechType().ToInventoryItemLegacy();
                            operation.Device.AddItemToContainer(result);
                            #else
                            CoroutineHost.StartCoroutine(AddItemToDevice(operation.Device, item));
                            #endif
                        }
                    }
                }
            }
        }

#if  !SUBNAUTICA_STABLE
        private IEnumerator AddItemToDevice(FcsDevice Device, TechType item)
        {
            var result = new TaskResult<InventoryItem>();
            yield return TakeItem(item).GetTechType().ToInventoryItem(result);
            Device.AddItemToContainer(result.value);
        }
#endif

        private void PerformPullOperation(BaseTransferOperation operation)
        {
            var randomItem = operation.Device.GetRandomTechTypeFromDevice();

            if (randomItem != TechType.None)
            {
                if (!IsAllowedToAdd(randomItem, false)) return;
                var item = operation.Device.RemoveItemFromDevice(randomItem);
                if (item != null)
                {
                    AddItemToContainer(item.ToInventoryItem());
                }
            }
        }

        private void Initialize(SubRoot habitat)
        {
            _savedData = Mod.GetBaseSaveData(BaseID);

            if (NameController == null)
            {
                NameController = new NameController();
                NameController.Initialize(Buildables.AlterraHub.Submit(), Buildables.AlterraHub.ChangeBaseName());
                _baseName = string.IsNullOrEmpty(_savedData?.InstanceID) ? GetDefaultName() : _savedData?.BaseName;
                QuickLogger.Debug($"Setting Base Name: {_baseName}");
                NameController.SetCurrentName(_baseName);
                NameController.OnLabelChanged += OnLabelChangedMethod;
            }

            if (_savedData != null)
            {
                DockingBlackList = _savedData.BlackList;
                PullFromDockedVehicles = _savedData.AllowDocking;
                HasBreakerTripped = _savedData.HasBreakerTripped;

                QuickLogger.Debug($"Base Operator: Operations is null: {_savedData.BaseOperations == null} | Is Version Empty: {string.IsNullOrWhiteSpace(_savedData.Version)} | Version is 1.0: {_savedData.Version == "1.0"}");

                if (_savedData.BaseOperations != null && !string.IsNullOrWhiteSpace(_savedData.Version) &&
                    _savedData.Version == "1.0")
                {
                    _baseOperations = _savedData.BaseOperations;
                }
            }

            if (_dumpContainer == null)
            {
                _dumpContainer = habitat.gameObject.EnsureComponent<DumpContainerSimplified>();
                _dumpContainer.Initialize(habitat.gameObject.transform, $"Add item to base: {GetBaseName()}", this, 6, 8, habitat.gameObject.name);
            }

            if (DockingManager == null)
            {
                DockingManager = habitat.gameObject.AddComponent<DSSVehicleDockingManager>();
                DockingManager.Initialize(this);
            }
        }

        private void OnLabelChangedMethod(string newName, NameController controller)
        {
            SetBaseName(newName);
        }

        private void PowerStateCheck()
        {
            if (Habitat?.powerRelay == null) return;
            if (_prevPowerState != Habitat.powerRelay.GetPowerStatus())
            {
                _prevPowerState = Habitat.powerRelay.GetPowerStatus();
                OnPowerStateChanged?.Invoke(Habitat.powerRelay.GetPowerStatus());
            }
        }

        public PowerSystem.Status GetPowerState()
        {
            if (Habitat?.powerRelay?.GetPowerStatus() == null)
            {
                QuickLogger.DebugError("GetPowerState returned null");
            }
            return Habitat?.powerRelay?.GetPowerStatus() ?? PowerSystem.Status.Offline;
        }

        #endregion

        private bool GetIsVisible()
        {
            return Habitat?.powerRelay != null && HasAntenna() && Habitat.powerRelay.GetPowerStatus() != PowerSystem.Status.Offline && !_hasBreakerTripped;
        }

        public void ToggleBreaker()
        {
            if (HasAntenna())
            {
                SendBaseMessage(HasBreakerTripped);
            }

            HasBreakerTripped = !HasBreakerTripped;
        }

        private static BaseManager CreateNewManager(SubRoot habitat)
        {
            QuickLogger.Debug($"Creating new manager", true);
            var manager = new BaseManager(habitat);
            QuickLogger.Debug($"Created new base manager with ID {manager.BaseID}", true);
            manager._portManager = habitat.gameObject.EnsureComponent<PortManager>();
            Managers.Add(manager);
            QuickLogger.Debug($"Manager Count = {Managers.Count}", true);
            OnManagerCreated?.Invoke(manager);
            return manager;
        }

        public static BaseManager FindManager(SubRoot subRoot)
        {
            if (subRoot == null) return null;
            var baseManager = FindManager(subRoot.gameObject.GetComponentInChildren<PrefabIdentifier>()?.Id);
            if (baseManager == null)
            {
                QuickLogger.Debug("[FindManager - SubRoot] returned null");
            }
            return baseManager ?? CreateNewManager(subRoot);
        }

        public static BaseManager FindManager(string instanceID)
        {
            if (string.IsNullOrWhiteSpace(instanceID)) return null;
            var manager = Managers.Find(x => x.BaseID.Equals(instanceID,StringComparison.OrdinalIgnoreCase));
            return manager;
        }

        public static BaseManager FindManager(TechLight techLight)
        {
            return GlobalTrackedLights.FirstOrDefault(x => x.Value.TechLight == techLight).Value.Manager;
        }

        /// <summary>
        /// Find the manager this gameObject is attached to.
        /// </summary>
        /// <param name="gameObject">The gameObject the we are trying to find the base for</param>
        /// <returns></returns>
        public static BaseManager FindManager(GameObject gameObject)
        {
            var subRoot = gameObject?.GetComponentInParent<SubRoot>() ?? gameObject?.GetComponentInChildren<SubRoot>();
            
            if (subRoot == null)
            {
                QuickLogger.Debug($"[BaseManager] SubRoot Returned null");
                return null;
            }
            return FindManager(subRoot);
        }

        public static BaseManager FindManagerByFriendlyID(string friendlyID)
        {
            foreach (BaseManager baseManager in Managers)
            {
                if (baseManager.BaseFriendlyID.Equals(friendlyID))
                {
                    QuickLogger.Debug($"Found Base: {baseManager.GetBaseName()} with id: {friendlyID}",true);
                    return baseManager;
                }
            }

            return null;
        }

        public static void RemoveDestroyedBases()
        {
            try
            {
                for (int i = Managers.Count - 1; i > -1; i--)
                {
                    if (Managers[i].Habitat == null)
                    {
                        FCSAlterraHubService.PublicAPI.UnRegisterBase(Managers[i]);
                        Managers.RemoveAt(i);
                    }
                }
            }
            catch (Exception e)
            {
                QuickLogger.DebugError(e.Message);
                QuickLogger.DebugError(e.StackTrace);
            }
        }

        /// <summary>
        /// Gets the stored base Name from the
        /// </summary>
        /// <returns></returns>
        public string GetBaseName()
        {
            return _baseName;
        }

        /// <summary>
        /// Gets the stored base Friendly ID
        /// </summary>
        /// <returns></returns>
        public string GetBaseFriendlyId()
        {
            return BaseFriendlyID;
        }

        /// <summary>
        /// Sets the base name field
        /// </summary>
        /// <param name="baseName"></param>
        public void SetBaseName(string baseName)
        {
            _baseName = baseName;
            GlobalNotifyByID(String.Empty, "BaseUpdate");
        }

        /// <summary>
        /// Creates a default base name and number index based on the count of items
        /// </summary>
        /// <returns></returns>
        public string GetDefaultName()
        {
            if (Habitat == null || Managers == null) return "Unknown";

            var id = GetBaseFriendlyId().Remove(0, 2);

            return Habitat.isCyclops ? $"Cyclops {id}" : $"Base {id}";
        }

        public void SendBaseMessage(string baseMessage)
        {
            QuickLogger.Message(baseMessage, true);
        }

        public void RegisterDevice(FcsDevice device)
        {
            if (!_registeredDevices.ContainsKey(device.UnitID))
            {
                if (device.IsRack)
                {
                    BaseRacks.Add((IDSSRack)device);
                }
                _registeredDevices.Add(device.UnitID, device);
                GlobalNotifyByID(String.Empty, "DeviceBuiltUpdate");
            }
        }

        private void PowerConsumption()
        {
            if (_registeredDevices == null) return;
                //Take power from the base
                for (int i = _registeredDevices.Count - 1; i >= 0; i--)
                {
                    var device = _registeredDevices.ElementAt(i);
                    if (device.Value.DoesTakePower && device.Value.IsOperational && Habitat.powerRelay != null)
                    {
                        Habitat.powerRelay.ConsumeEnergy(device.Value.GetPowerUsage(), out float amountConsumed);
                    }
                }
        }

        public bool HasEnoughPower(float power)
        {
            if (Habitat.powerRelay == null)
            {
                //QuickLogger.DebugError("Habitat is null");
            }

            if (!GameModeUtils.RequiresPower()) return true;

            if (Habitat.powerRelay == null || Habitat.powerRelay.GetPower() < power) return false;
            return true;
        }

        public void UnRegisterDevice(FcsDevice device)
        {
            if (device != null && !string.IsNullOrWhiteSpace(device.UnitID) && _registeredDevices.ContainsKey(device.UnitID))
            {
                if (device.IsRack)
                {
                    BaseRacks.Remove((IDSSRack)device);
                }

                _registeredDevices?.Remove(device.UnitID);
                GlobalNotifyByID(string.Empty, "DeviceBuiltUpdate");
            }
        }

        public void UnRegisterDevice(TechLight device)
        {
            var prefabId = device.gameObject.GetComponent<PrefabIdentifier>()?.Id;
            if (!string.IsNullOrEmpty(prefabId) && _baseTechLights.ContainsKey(prefabId))
            {
                _baseTechLights.Remove(prefabId);
                RemoveGlobalTrackedLight(prefabId);
            }
        }

        public IEnumerable<FcsDevice> GetDevices(string tabID)
        {
            foreach (KeyValuePair<string, FcsDevice> device in _registeredDevices)
            {
                if (device.Key.StartsWith(tabID, StringComparison.OrdinalIgnoreCase))
                {
                    yield return device.Value;
                }
            }
        }

        public Dictionary<string, FcsDevice> GetRegisteredDevices()
        {
            return _registeredDevices;
        }

        /// <summary>
        /// Checks to see if the device is registered at base doesnt take in count of being contructed
        /// </summary>
        /// <param name="tabID"></param>
        /// <returns></returns>
        public bool IsDeviceBuilt(string tabID)
        {
            return _registeredDevices.Any(x => x.Key.StartsWith(tabID, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Checks to see if the device is registered at base and take in count of being constructed
        /// </summary>
        /// <param name="tabID"></param>
        /// <param name="device">Found device</param>
        /// <returns></returns>
        public bool DeviceBuilt(string tabID, out IEnumerable<KeyValuePair<string, FcsDevice>> device)
        {
            if (IsDeviceBuilt(tabID))
            {
                device = _registeredDevices.Where(x => x.Key.StartsWith(tabID, StringComparison.OrdinalIgnoreCase) && x.Value.IsConstructed);
                return device.Any();
            }
            
            device = null;
            return false;
        }

        public void NotifyByID(string modID, string commandMessage)
        {
            if (!string.IsNullOrEmpty(modID))
            {
                foreach (KeyValuePair<string, FcsDevice> device in FCSAlterraHubService.PublicAPI
                    .GetRegisteredDevicesOfId(modID))
                {
                    device.Value.IPCMessage?.Invoke(commandMessage);
                }
            }
            else
            {
                foreach (KeyValuePair<string, FcsDevice> device in FCSAlterraHubService.PublicAPI.GetRegisteredDevices()
                )
                {
                    device.Value.IPCMessage?.Invoke(commandMessage);
                }
            }
        }

        public static void GlobalNotifyByID(string modID, string commandMessage)
        {
            if (!string.IsNullOrEmpty(modID))
            {
                foreach (KeyValuePair<string, FcsDevice> device in FCSAlterraHubService.PublicAPI.GetRegisteredDevicesOfId(modID))
                {
                    device.Value.IPCMessage?.Invoke(commandMessage);
                }
            }
            else
            {
                foreach (KeyValuePair<string, FcsDevice> device in FCSAlterraHubService.PublicAPI.GetRegisteredDevices()
                )
                {
                    device.Value.IPCMessage?.Invoke(commandMessage);
                }
            }
        }

        public float GetPower()
        {
            if (Habitat.powerRelay != null)
            {
                return Habitat.powerRelay.GetPower();
            }

            return 0;
        }

        public void RegisterDevice(TechLight device)
        {
            var prefabId = device.gameObject.GetComponent<PrefabIdentifier>()?.Id;
            if (!string.IsNullOrEmpty(prefabId) && !_baseTechLights.ContainsKey(prefabId))
            {
                _baseTechLights.Add(prefabId, device);
                var controller = device.gameObject.AddComponent<FCSTechLigtController>();
                controller.Initialize(this);
                AddGlobalTrackedLight(prefabId, device, null, this);
            }
        }

        private static void AddGlobalTrackedLight(string prefabId, TechLight techLight, BaseSpotLight spotLight, BaseManager baseManager)
        {
            if (!GlobalTrackedLights.ContainsKey(prefabId))
            {
                GlobalTrackedLights.Add(prefabId, new TrackedLight { Manager = baseManager, TechLight = techLight, SpotLight = spotLight });
            }
        }

        private static void RemoveGlobalTrackedLight(string prefabId)
        {
            if (GlobalTrackedLights.ContainsKey(prefabId))
            {
                GlobalTrackedLights.Remove(prefabId);
            }
        }

        public int GetDevicesCount(string unitTabId)
        {
            var i = 0;
            foreach (var device in _registeredDevices)
            {
                if (device.Value.UnitID.StartsWith(unitTabId))
                {
                    i++;
                }
            }

            return i;
        }

        public int GetItemCount(TechType techType)
        {
            if (!TrackedResources.ContainsKey(techType)) return 0;

            return TrackedResources.ContainsKey(techType) ? TrackedResources[techType].Amount : 0;
        }

        public bool HasItem(TechType techType)
        {
            foreach (IDSSRack baseRack in BaseRacks)
            {
                if (baseRack.HasItem(techType))
                {
                    return true;
                }
            }

            foreach (StorageContainer locker in BaseStorageLockers)
            {
                if (locker?.container == null)
                {
                    continue;
                }

                if (locker.container.Contains(techType))
                {
                    return true;
                }
            }

            foreach (FcsDevice device in BaseFcsStorage)
            {
                if(device?.GetItemsContainer() == null)continue;
                
                if (device.GetItemsContainer().Contains(techType))
                {
                    return true;
                }
            }

            return false;
        }

        public Pickupable TakeItem(TechType techType, StorageType storageFilter = StorageType.All)
        {
            if (storageFilter == StorageType.Servers || storageFilter == StorageType.All)
            {
                foreach (IDSSRack baseRack in BaseRacks)
                {
                    if (baseRack != null && baseRack.HasItem(techType))
                    {
                        QuickLogger.Debug("Took Item from Sever",true);
                        return baseRack.RemoveItemFromRack(techType);
                    }
                }
            }

            if (storageFilter == StorageType.SeaBreeze || storageFilter == StorageType.All)
            {
                foreach (FcsDevice device in BaseFcsStorage)
                {
                    if(!device.TabID.Equals("SB")) continue;

                    var storage = device?.GetItemsContainer();

                    if (storage == null || !storage.Contains(techType)) continue;

                    QuickLogger.Debug("Took Item from Seabreeze", true);
                    return device.GetItemsContainer().RemoveItem(techType);
                }
            }

            if (storageFilter == StorageType.Replicator || storageFilter == StorageType.All)
            {
                foreach (FcsDevice device in BaseFcsStorage)
                {
                    if (!device.TabID.Equals("RM")) continue;

                    var storage = device?.GetItemsContainer();

                    if (storage == null || !storage.Contains(techType)) continue;

                    QuickLogger.Debug("Took Item from Replicator", true);
                    return device.GetItemsContainer().RemoveItem(techType);
                }
            }

            if (storageFilter == StorageType.Harvester || storageFilter == StorageType.All)
            {
                foreach (FcsDevice device in BaseFcsStorage)
                {
                    if (!device.TabID.Equals("HH")) continue;

                    var storage = device?.GetItemsContainer();

                    if (storage == null || !storage.Contains(techType)) continue;

                    QuickLogger.Debug("Took Item from Harvester", true);
                    return device.GetItemsContainer().RemoveItem(techType);
                }
            }


            if (storageFilter == StorageType.RemoteStorage || storageFilter == StorageType.All)
            {
                foreach (FcsDevice device in BaseFcsStorage)
                {
                    var storage = device?.GetItemsContainer();

                    if (storage == null || !storage.Contains(techType)) continue;

                    QuickLogger.Debug("Took Item from FCSDevice", true);
                    return device.GetItemsContainer().RemoveItem(techType);
                }
            }

            if (storageFilter == StorageType.StorageLockers || storageFilter == StorageType.All)
            {
                foreach (StorageContainer locker in BaseStorageLockers)
                {
                    if (locker?.container == null)
                    {
                        continue;
                    }

                    if (locker.container.Contains(techType))
                    {
                        QuickLogger.Debug("Took Item from Storage Container", true);
                        return locker.container.RemoveItem(techType);
                    }
                }
            }

            GlobalNotifyByID("DTC", "ItemUpdateDisplay");

            return null;
        }

        public Dictionary<TechType, int> GetItemsWithin(StorageType type = StorageType.All)
        {
            _item.Clear();

            switch (type)
            {
                case StorageType.All:
                    return TrackedResources.ToDictionary(x => x.Key, x => x.Value.Amount);
                case StorageType.Servers:
                    foreach (KeyValuePair<TechType, TrackedResource> resource in TrackedResources)
                    {
                        foreach (FcsDevice device in resource.Value.Servers)
                        {
                            CalculateItems(_item, resource, device);
                        }
                    }
                    return _item;
                case StorageType.SeaBreeze:
                    foreach (KeyValuePair<TechType, TrackedResource> resource in TrackedResources)
                    {
                        foreach (FcsDevice device in resource.Value.OtherStorage)
                        {
                            if(!device.TabID.Equals("SB")) continue;
                            CalculateItems(_item, resource, device);
                        }
                    }
                    return _item;
                case StorageType.StorageLockers:
                    foreach (KeyValuePair<TechType, TrackedResource> resource in TrackedResources)
                    {
                        foreach (StorageContainer device in resource.Value.StorageContainers)
                        {
                            if (_item.ContainsKey(resource.Key))
                            {
                                _item[resource.Key] += device.container.GetCount(resource.Key);
                            }
                            else
                            {
                                _item.Add(resource.Key, device.container.GetCount(resource.Key));
                            }
                        }
                    }
                    return _item;
                case StorageType.RemoteStorage:
                    foreach (KeyValuePair<TechType, TrackedResource> resource in TrackedResources)
                    {
                        foreach (FcsDevice device in resource.Value.OtherStorage)
                        {
                            if (!device.TabID.Equals("AS")) continue;
                            CalculateItems(_item, resource, device);
                        }
                    }
                    return _item;
                case StorageType.Harvester:
                    foreach (KeyValuePair<TechType, TrackedResource> resource in TrackedResources)
                    {
                        foreach (FcsDevice device in resource.Value.OtherStorage)
                        {
                            if (!device.TabID.Equals("HH")) continue;
                            CalculateItems(_item, resource, device);
                        }
                    }
                    return _item;
                case StorageType.Replicator:
                    foreach (KeyValuePair<TechType, TrackedResource> resource in TrackedResources)
                    {
                        foreach (FcsDevice device in resource.Value.OtherStorage)
                        {
                            if (!device.TabID.Equals("RM")) continue;
                            CalculateItems(_item, resource, device);
                        }
                    }
                    return _item;
            }

            return null;
        }

        private static void CalculateItems(Dictionary<TechType, int> item, KeyValuePair<TechType, TrackedResource> resource, FcsDevice device)
        {
            if (item.ContainsKey(resource.Key))
            {
                item[resource.Key] += device.GetItemsContainer().GetCount(resource.Key);
            }
            else
            {
                item.Add(resource.Key, device.GetItemsContainer().GetCount(resource.Key));
            }
        }

        public bool ContainsItem(TechType techType)
        {
            return TrackedResources.ContainsKey(techType);
        }

        public void AddItemsToTracker(FcsDevice device,TechType item, int amountToAdd = 1)
        {
            if (!string.IsNullOrWhiteSpace(device?.TabID) && !device.TabID.Equals("DSV"))
            {
                if (TrackedResources.ContainsKey(item))
                {
                    TrackedResources[item].Amount += amountToAdd;
                    TrackedResources[item].OtherStorage.Add(device);
                }
                else
                {
                    TrackedResources.Add(item, new TrackedResource()
                    {
                        TechType = item,
                        Amount = amountToAdd,
                        OtherStorage = new HashSet<FcsDevice>() { device }
                    });
                }
            }
            else
            {
                if (TrackedResources.ContainsKey(item))
                {
                    TrackedResources[item].Amount += amountToAdd;
                    TrackedResources[item].Servers.Add(device);
                }
                else
                {
                    TrackedResources.Add(item, new TrackedResource()
                    {
                        TechType = item,
                        Amount = amountToAdd,
                        Servers = new HashSet<FcsDevice>() { device }
                    });
                }
            }

            GlobalNotifyByID("DTC", "ItemUpdateDisplay");
        }

        public void RemoveItemsFromTracker(FcsDevice server, TechType item, int amountToRemove = 1)
        {
            QuickLogger.Debug($"RemoveItemsFromTracker: DSSServerController || {item.AsString()} || {amountToRemove} ");


            if (TrackedResources.ContainsKey(item))
            {
                TrackedResource trackedResource = TrackedResources[item];
                int newAmount = trackedResource.Amount - amountToRemove;
                trackedResource.Amount = newAmount;

                if (newAmount <= 0)
                {
                    TrackedResources.Remove(item);
                    //BaseManager.SendNotification();
                }
                else
                {
                    int amountLeftInContainer = server.GetItemCount(item);
                    if (amountLeftInContainer <= 0)
                    {
                        trackedResource.Servers.Remove(server);
                    }
                }

                GlobalNotifyByID("DTC", "ItemUpdateDisplay");
            }
        }

        public void RemoveServerFromBase(FcsDevice server)
        {

            //BaseManager.SetAllowedToNotify(false);
            foreach (KeyValuePair<TechType, int> item in server.GetItemsWithin())
            {
                RemoveItemsFromTracker(server, item.Key, item.Value);
            }

            server.OnAddItem -= OnServerAddItem;
            server.OnRemoveItem -= OnServerRemoveItem;
            BaseServers.Remove(server);
            //BaseManager.SetAllowedToNotify(true);
            GlobalNotifyByID("DTC", "ItemUpdateDisplay");
        }

        public void RegisterServerInBase(FcsDevice server)
        {
            if (!BaseServers.Contains(server))
            {
                BaseServers.Add(server);
                //BaseManager.SetAllowedToNotify(false);
                foreach (KeyValuePair<TechType, int> item in server.GetItemsWithin())
                {
                    AddItemsToTracker(server, item.Key, item.Value);
                }
                server.OnAddItem += OnServerAddItem;
                server.OnRemoveItem += OnServerRemoveItem;
                //BaseManager.SetAllowedToNotify(true);
                GlobalNotifyByID("DTC", "ItemUpdateDisplay");
            }
        }

        private void OnServerRemoveItem(FcsDevice server, InventoryItem item)
        {
            RemoveItemsFromTracker(server, item.item.GetTechType());
        }

        private void OnServerAddItem(FcsDevice server, InventoryItem item)
        {
            AddItemsToTracker(server, item.item.GetTechType());
        }
        
        #region Storage Containers

        public void AlertNewStorageContainerPlaced(StorageContainer sc)
        {
            if (BaseStorageLockers.Contains(sc) || !TrackStorageContainer(sc)) return;

            var stracker = sc.gameObject.EnsureComponent<FCSStorageTracker>();
            stracker.Set(sc);
            stracker.OnDestroyAction += OnDestroyAction;
            BaseStorageLockers.Add(sc);
        }

        private void OnDestroyAction(StorageContainer obj,FCSStorageTracker tracker)
        {
            tracker.OnDestroyAction -= OnDestroyAction;
            BaseStorageLockers.Remove(obj);
        }

        private void TrackExistingStorageContainers()
        {
            StorageContainer[] containers = Habitat.GetComponentsInChildren<StorageContainer>();
            foreach (StorageContainer sc in containers)
            {
                TrackStorageContainer(sc);
            }
        }

        private bool TrackStorageContainer(StorageContainer sc)
        {
            if (sc == null || sc.container == null)
            {
                return false;
            }

            foreach (string notTrackedObject in DONT_TRACK_GAMEOBJECTS)
            {
                if (sc.gameObject.name.ToLower().Contains(notTrackedObject))
                {
                    return false;
                }
            }

            foreach (var item in sc.container.GetItemTypes())
            {
                for (int i = 0; i < sc.container.GetCount(item); i++)
                {
                    AddItemsToTracker(sc, item);
                }
            }

            sc.container.onAddItem += (item) => AddItemsToTracker(sc, item.item.GetTechType());
            sc.container.onRemoveItem += (item) => RemoveItemsFromTracker(sc, item.item.GetTechType());
            return true;
        }

        public void AlertNewFcsStoragePlaced(FcsDevice device)
        {
            if (BaseFcsStorage.Contains(device)) return;
            BaseFcsStorage.Add(device);
            TrackFcsStorage(device);
        }

        private void TrackFcsStorage(FcsDevice sc)
        {
            if(sc == null) return;

            if (sc.GetItemsContainer() == null)
            {
                QuickLogger.Debug($"Failed to add {sc.UnitID} at {sc.BaseId} because ItemsContainer returned null",true);
                return;
            }

            foreach (string notTrackedObject in DONT_TRACK_GAMEOBJECTS)
            {
                if (sc.gameObject.name.ToLower().Contains(notTrackedObject))
                {
                    return;
                }
            }

            foreach (var item in sc.GetItemsContainer().GetItemTypes())
            {
                for (int i = 0; i < sc.GetItemsContainer().GetCount(item); i++)
                {
                    AddItemsToTracker(sc, item);
                }
            }

            sc.GetItemsContainer().onAddItem += (item) => AddItemsToTracker(sc, item.item.GetTechType());
            sc.GetItemsContainer().onRemoveItem += (item) => RemoveItemsFromTracker(sc, item.item.GetTechType());
        }

        public void RemoveItemsFromTracker(StorageContainer sc, TechType item, int amountToRemove = 1)
        {
            QuickLogger.Debug($"RemoveItemsFromTracker: StorageContainer || {item.AsString()} || {amountToRemove} ");

            if (TrackedResources.ContainsKey(item))
            {
                TrackedResource trackedResource = TrackedResources[item];
                int newAmount = trackedResource.Amount - amountToRemove;
                trackedResource.Amount = newAmount;

                if (newAmount <= 0)
                {
                    TrackedResources.Remove(item);
                }
                else
                {
                    int amountLeftInContainer = sc.container.GetCount(item);
                    if (amountLeftInContainer <= 0)
                    {
                        trackedResource.StorageContainers.Remove(sc);
                    }
                }
                GlobalNotifyByID("DTC", "ItemUpdateDisplay");
            }
        }

        public void AddItemsToTracker(StorageContainer sc, TechType item, int amountToAdd = 1)
        {
            QuickLogger.Debug($"AddItemsToTracker: StorageContainer || {item.AsString()} || {amountToAdd} ");


            if (DONT_TRACK_GAMEOBJECTS.Contains(item.AsString().ToLower()))
            {
                return;
            }

            if (TrackedResources.ContainsKey(item))
            {
                TrackedResources[item].Amount = TrackedResources[item].Amount + amountToAdd;
                TrackedResources[item].StorageContainers.Add(sc);
            }
            else
            {
                TrackedResources.Add(item, new TrackedResource()
                {
                    TechType = item,
                    Amount = amountToAdd,
                    StorageContainers = new HashSet<StorageContainer>()
                    {
                        sc
                    }
                });
            }
            GlobalNotifyByID("DTC", "ItemUpdateDisplay");
        }

        #endregion

        public int GetTotal(StorageType type)
        {
            return GetItemsWithin(type).Sum(x=>x.Value);
        }
        
        internal void RegisterAntenna(FcsDevice unit)
        {
            if (!BaseAntennas.Contains(unit) && unit.IsConstructed)
            {
                if (!unit.Manager.HasAntenna())
                {
                    unit.Manager.SendBaseMessage(true);
                }

                BaseAntennas.Add(unit);
            }
        }

        internal void RemoveAntenna(FcsDevice unit)
        {
            if (BaseAntennas.Contains(unit))
            {
                BaseAntennas.Remove(unit);

                if (!unit.Manager.HasAntenna())
                {
                    unit.Manager.SendBaseMessage(false);
                }
            }
        }

        private bool HasAntenna()
        {
            if (Habitat.isCyclops) return true;
            return BaseAntennas.Count > 0;
        }

        internal void SendBaseMessage(bool state)
        {
            QuickLogger.Message(Buildables.AlterraHub.BaseOnOffMessage(GetBaseName(), state ? Buildables.AlterraHub.Online() : Buildables.AlterraHub.Offline()), true);
        }

        public void ChangeBaseName()
        {
            NameController.Show();
        }

        public void AlertNewAntennaPlaced(FcsDevice fcsDevice)
        {
            if (!BaseAntennas.Contains(fcsDevice))
            {
                BaseAntennas.Add(fcsDevice);
                GlobalNotifyByID(String.Empty, "BaseUpdate");
            }
        }

        public void AlertNewAntennaDestroyed(FcsDevice fcsDevice)
        {
            BaseAntennas.Remove(fcsDevice);
            GlobalNotifyByID(String.Empty, "BaseUpdate");
        }

        public void OpenBaseStorage()
        {
            _dumpContainer.OpenStorage();
        }

        public bool IsAllowedToAdd(TechType techType, bool verbose)
        {
            int availableSpace = 0;

            var validServers = new List<ISlotController>();

#if SUBNAUTICA
            var techDataResult = CraftData.cookedCreatureList.ContainsValue(techType) || WorldHelpers.Eatables.Contains(techType);
#else
            var techDataResult = WorldHelpers.Eatables.Contains(techType);
#endif

            if (techDataResult)
            {
                QuickLogger.ModMessage(Buildables.AlterraHub.FoodItemsNotAllowed());
                return false;
            }

            QuickLogger.Debug($"Checking if allowed {_dumpContainer.GetItemCount() + 1}", true);

            foreach (IDSSRack baseRack in BaseRacks)
            {
                foreach (var server in baseRack.GetServers())
                {
                    if (server.ItemAllowed((techType)))
                    {
                        validServers.Add(server);
                    }
                }

            }

            if (validServers.Count > 0)
            {
                availableSpace = validServers.Sum(x => x.GetFreeSpace());
            }

            var result = availableSpace >= _dumpContainer.GetItemCount() + 1;

            QuickLogger.Debug($"Allowed result: {result}", true);
            return result;
        }

        public bool IsAllowedToAdd(TechType techType,int amount, bool checkForOtherStorages, bool verbose = false)
        {
            int availableSpace = 0;
            int target = amount;
            var validServers = new List<ISlotController>();

            //QuickLogger.Debug($"Checking if allowed {_dumpContainer.GetItemCount() + 1}", true);

            foreach (IDSSRack baseRack in BaseRacks)
            {
                if (baseRack.ItemAllowed(techType, out var server))
                {
                    if (!server.IsFull)
                        validServers.Add(server);
                }
            }

            if (validServers.Count > 0)
            {
                availableSpace = validServers.Sum(x => x.GetFreeSpace());
            }

            target -= availableSpace;
            
            if (target < 0)
            {
                target = 0;
            }


            //TODO Fix currently when checking other storage for lockers its only checking the container once for one item if multiple items are needed
            //this wont be checked. ex amount is 2

            if (checkForOtherStorages && target > 0)
            {
                foreach (var locker in BaseStorageLockers)
                {

                    var sizes = new List<Vector2int>();
                    var size = TechDataHelpers.GetItemSize(techType);

                    for (int i = 0; i < amount; i++)
                    {
                        sizes.Add(size);
                    }

                    if (locker.container.HasRoomFor(sizes))
                    {
                            return  true;
                    }
                }

                foreach (FcsDevice fcsDevice in GetDevices("AS"))
                {
                    if (fcsDevice.CanBeStored(target, techType))
                    {
                        return true;
                    }
                }

            }

            //QuickLogger.Debug($"Allowed result: {result}", true);
            return target == 0;
        }

        public bool IsAllowedToAdd(Pickupable inventoryItem, bool verbose)
        {
            return IsAllowedToAdd(inventoryItem.GetTechType(),verbose);
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            try
            {
                var result = AddItemToNetwork(item, this);

                if (!result)
                {
                    PlayerInteractionHelper.GivePlayerItem(item);
                }
                else
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message, true);
                QuickLogger.Error(e.StackTrace);
                PlayerInteractionHelper.GivePlayerItem(item);
            }
            return false;
        }

        public static bool AddItemToNetwork(InventoryItem item, BaseManager manager)
        {
            QuickLogger.Debug($"Trying to add item to network: {manager?.GetBaseName()} | {Language.main.Get(item.item.GetTechType())}",true);

            if (manager == null) return false;

            foreach (IDSSRack baseRack in manager.BaseRacks)
            {
                if (baseRack.ItemAllowed(item.item.GetTechType(), out var server))
                {
                    server?.AddItemToMountedServer(item);
                    return true;
                }
            }
            return false;
        }

        public static bool AddItemToNetwork(BaseManager manager, InventoryItem item, bool includeOtherStorages = false)
        {
            QuickLogger.Debug($"Trying to add item to network: {manager?.GetBaseName()} | {Language.main.Get(item.item.GetTechType())}", true);

            if (manager == null) return false;

            foreach (IDSSRack baseRack in manager.BaseRacks)
            {
                if (baseRack.ItemAllowed(item.item.GetTechType(), out var server))
                {
                    server?.AddItemToMountedServer(item);
                    QuickLogger.Debug($"Added item {Language.main.Get(item.item.GetTechType())} to base rack {baseRack.UnitID}",true);
                    return true;
                }
            }

            foreach (StorageContainer locker in manager.BaseStorageLockers)
            {
                if(locker.container.HasRoomFor(item.item))
                {
                    locker.container.UnsafeAdd(item);
                    QuickLogger.Debug($"Added item {Language.main.Get(item.item.GetTechType())} to locker {locker.storageRoot.Id}", true);
                    return true;
                }
            }

            foreach (FcsDevice fcsDevice in manager.GetDevices("AS"))
            {
                if (fcsDevice.CanBeStored(1, item.item.GetTechType()))
                {
                    fcsDevice.AddItemToContainer(item);
                    QuickLogger.Debug($"Added item {Language.main.Get(item.item.GetTechType())} to Alterra Storage {fcsDevice.UnitID}", true);

                    return true;
                }
            }

            return false;
        }

        public bool IsDockingFilterAddedWithType(TechType techType)
        {
            return DockingBlackList.Contains(techType);
        }

        public static IEnumerable<BaseSaveData> Save()
        {
            foreach (BaseManager baseManager in Managers)
            {
                yield return new BaseSaveData
                {
                    InstanceID = baseManager.BaseID,
                    BaseName = baseManager.GetBaseName(),
                    AllowDocking = baseManager.PullFromDockedVehicles,
                    HasBreakerTripped = baseManager.HasBreakerTripped,
                    BlackList = baseManager.DockingBlackList,
                    BaseOperations = baseManager.GetBaseOperations(),
                };
            }
        }

        public bool GetBreakerState()
        {
            return HasBreakerTripped;
        }

        public bool HasIngredientsFor(TechType techType)
        {
            var techData = CraftDataHandler.GetTechData(techType);
            //QuickLogger.Debug($"TechData: {techData?.ingredientCount}",true);


            if (techData != null)
            {
                foreach (Ingredient ingredient in techData.Ingredients)
                {
                    var hasItem = GetItemCount(ingredient.techType) >= ingredient.amount;
                    //QuickLogger.Debug($"Has Item: {hasItem} || Item: {ingredient.techType}",true);
                    
                    if(!hasItem)
                        return false;
                }
            }

            return true;
        }

        public void ConsumeIngredientsFor(TechType techType)
        {
            var techData = CraftDataHandler.GetTechData(techType);
            QuickLogger.Debug($"TechData: {techData?.ingredientCount} | {Language.main.Get(techType)}", true);


            if (techData != null)
            {
                foreach (Ingredient ingredient in techData.Ingredients)
                {
                    for (int i = 0; i < ingredient.amount; i++)
                    {
                        var item = TakeItem(ingredient.techType);
                        if (item != null)
                        {
                            GameObject.Destroy(item.gameObject);
                        }
                        else
                        {
                            QuickLogger.Error($"Failed to pull item {Language.main.Get(techType)} from TakeItem()");
                        }
                    }
                }
            }
        }
        
        public float GetRequiredTankCount(bool hardcore)
        {
            float bigRooms = 0;
            float smallRooms = 0;

            foreach (Int3 cell in BaseComponent.AllCells)
            {
                Base.CellType cellType = BaseComponent.GetCell(cell);

                switch (cellType)
                {
                    case Base.CellType.Corridor:
                        smallRooms += 1;
                        break;
                    case Base.CellType.Observatory:
                        smallRooms += 1;
                        break;
                    case Base.CellType.MapRoom:
                        if (hardcore)
                            bigRooms += 1f / 9f;
                        else
                            smallRooms += 1f / 9f;
                        break;
                    case Base.CellType.MapRoomRotated:
                        if (hardcore)
                            bigRooms += 1f / 9f;
                        else
                            smallRooms += 1f / 9f;
                        break;
                    case Base.CellType.Moonpool:
                        bigRooms += 1f / 12f;
                        break;
                    case Base.CellType.Room:
                        bigRooms += 1f / 9f;
                        break;
                }
            }

            return (float)Math.Round((bigRooms / (hardcore ? 1 : 2)) + (smallRooms / (hardcore ? 4 : 10)),2);
        }

        public float GetBasePowerCapacity()
        {
            return Habitat?.powerRelay.GetMaxPower() ?? 0;
        }

        public Dictionary<string, BaseOperationObject> GetBaseOperators()
        {
            return _baseOperationObjects;
        }

        public List<BaseTransferOperation> GetBaseOperations()
        {
            return _baseOperations;
        }

        public void AddTransceiver(BaseOperationObject operationObject)
        {
            if (string.IsNullOrWhiteSpace(operationObject?.GetPrefabId()))
            {
                QuickLogger.DebugError("BaseOperation Object PrefabId is null",true);
                return;
            }

            if (!_baseOperationObjects.ContainsKey(operationObject.GetPrefabId()))
            { 
                _baseOperationObjects.Add(operationObject.GetPrefabId(), operationObject);
                GlobalNotifyByID("DTC", "RefreshTransceiverData");
                QuickLogger.Debug("Added Transmitter to base",true);
            }
        }

        public void RemoveTransceiver(BaseOperationObject operationObject)
        {
            if (string.IsNullOrWhiteSpace(operationObject?.GetPrefabId()))
            {
                QuickLogger.DebugError("BaseOperation Object PrefabId is null", true);
                return;
            }

            if (!_baseOperationObjects.ContainsKey(operationObject.GetPrefabId())) return;
            
            _baseOperationObjects.Remove(operationObject.GetPrefabId());
            
            GlobalNotifyByID("DTC", "RefreshTransceiverData");
            QuickLogger.Debug("Removed Transmitter to from base", true);
        }

        public bool HasTransceiverConnected()
        {
            return _baseOperationObjects.Count > 0;
        }

        public void AddOperationForDevice(BaseTransferOperation operation )
        {
            var result = _baseOperations.Any(x => x.DeviceId == operation.DeviceId);
            if (result)
            {
                _baseOperations[_baseOperations.FindIndex(x => x.DeviceId.Equals(operation.DeviceId))] = operation;
                return;
            }

            _baseOperations.Add(operation);
        }

        public BaseTransferOperation GetDeviceOperation(FcsDevice device)
        {
            foreach (BaseTransferOperation operation in _baseOperations)
            {
                if (operation.Device == device)
                {
                    return operation;
                }
            }

            return null;
        }

        public FcsDevice FindDeviceById(string deviceID)
        {
            if (_registeredDevices.ContainsKey(deviceID))
            {
                return _registeredDevices[deviceID];
            }

            return null;
        }

        internal PortManager GetPortManager()
        {
            return _portManager;
        }

        public static void FindManager(string instanceId, Action<BaseManager> callBack)
        {
            if (string.IsNullOrWhiteSpace(instanceId)) return;
            CoroutineHost.StartCoroutine(LocateBaseManager(instanceId,callBack));
        }

        private static IEnumerator LocateBaseManager(string instanceId,Action<BaseManager> callBack)
        {
            while (FindManager(instanceId) == null)
            {
                QuickLogger.Debug($"Trying to find base with ID ({instanceId})");
                yield return null;
            }

            callBack?.Invoke(FindManager(instanceId));
            yield break;
        }

        public PowerRelay GetPowerRelay()
        {
            return Habitat.powerRelay;
        }

        public void DestroyItemsFromBase(TechType techType, int value)
        {
            for (int i = 0; i < value; i++)
            {
                var item = TakeItem(techType);
                if(item == null) return;
                GameObject.Destroy(item.gameObject);
            }
        }

        /// <summary>
        /// Checks if the current base is the same as the passed in base
        /// </summary>
        /// <param name="sub"></param>
        /// <returns></returns>
        public bool IsSame(SubRoot sub)
        {
            return sub == Habitat;
        }

        public string GetBaseHullStrength()
        {
            return _baseHullStrength?.GetTotalStrength().ToString("F2") ?? "0";
        }

        public static BaseManager GetPlayersCurrentBase()
        {
            if (Player.main == null || Player.main.currentSub == null) return null;

            return Player.main.currentSub.isBase || Player.main.currentSub.isCyclops ? FindManager(Player.main.currentSub) : null;
        }

        public bool IsValidForTeleport()
        {
            if (Player.main == null) return false;

            if (Player.main.IsPiloting())
            {
                foreach (KeyValuePair<string, FcsDevice> device in _registeredDevices)
                {
                    if (device.Key.ToLower().StartsWith("qvp") && device.Value.IsOperational) return true;
                }
            }
            else
            {
                foreach (KeyValuePair<string, FcsDevice> device in _registeredDevices)
                {
                    if (device.Key.ToLower().StartsWith("qt") && device.Value.IsOperational && device.Value.IsVisible) return true;
                }
            }

            return false;
        }

        public List<string> GetConnectedDevices(string telepowerPylonTabId)
        {
            return (from device in _registeredDevices where device.Key == telepowerPylonTabId select device.Value.UnitID).ToList();
        }

        internal static AlterraDronePortController FindPort(string portPrefabId)
        {
            foreach (BaseManager baseManager in Managers)
            {
                var port = baseManager.GetPortManager().FindPort(portPrefabId);
                if (port != null)
                {
                    return port;
                }
            }

            return null;
        }
    }

    public class FCSStorageTracker : MonoBehaviour
    {
        public Action<StorageContainer,FCSStorageTracker> OnDestroyAction;
        private StorageContainer _sc;

        internal void Set(StorageContainer sc)
        {
            _sc = sc;
        }

        private void OnDestroy()
        {
            OnDestroyAction?.Invoke(_sc,this);
        }
    }
}