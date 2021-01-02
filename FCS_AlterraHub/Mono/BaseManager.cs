using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono.Controllers;
using FCS_AlterraHub.Patches;
using FCS_AlterraHub.Registration;
using FCSCommon.Utilities;
using UnityEngine;

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
        public string BaseID { get; set; }
        public static Action<BaseManager> OnManagerCreated { get; set; }
        private readonly Dictionary<string, FcsDevice> _registeredDevices;
        private float _timeLeft = 1f;
        private PowerSystem.Status _prevPowerState;
        private Dictionary<string, TechLight> _baseTechLights;
        public readonly Dictionary<TechType, TrackedResource> TrackedResources = new Dictionary<TechType, TrackedResource>();
        public static List<string> DONT_TRACK_GAMEOBJECTS { get; private set; } = new List<string>
        {
            "planterpot",
            "planterbox",
            "plantershelf",
            "alongplanter"
        };
        public readonly HashSet<StorageContainer> BaseStorageLockers = new HashSet<StorageContainer>();
        public readonly HashSet<FcsDevice> BaseServers = new HashSet<FcsDevice>();
        public readonly HashSet<FcsDevice> BaseFcsStorage = new HashSet<FcsDevice>();
        public readonly HashSet<FcsDevice> BaseAntennas = new HashSet<FcsDevice>();
        public readonly HashSet<FcsDevice> BaseTerminals = new HashSet<FcsDevice>();
        private DumpContainerSimplified _dumpContainer;

        public SubRoot Habitat { get; set; }

        public bool IsVisible => GetIsVisible();

        private bool GetIsVisible()
        {
            return Habitat?.powerRelay != null && HasAntenna() && Habitat.powerRelay.GetPowerStatus() == PowerSystem.Status.Normal;
        }

        public static List<BaseManager> Managers { get; } = new List<BaseManager>();
        internal NameController NameController { get; private set; }
        public static Dictionary<string, TrackedLight> GlobalTrackedLights { get; } = new Dictionary<string, TrackedLight>();
        public Action<PowerSystem.Status> OnPowerStateChanged { get; set; }
        public bool IsBaseExternalLightsActivated { get; set; }
        public List<IDSSRack> BaseRacks { get; set; } = new List<IDSSRack>();
        public DSSVehicleDockingManager DockingManager { get; set; }
        public bool PullFromDockedVehicles { get; set; }
        public List<TechType> DockingBlackList { get; set; } = new List<TechType>();
        public Action<bool> OnBreakerStateChanged { get; set; }

        #region Default Constructor

        public BaseManager(SubRoot habitat)
        {
            if (habitat == null) return;
            Habitat = habitat;
            BaseID = habitat.gameObject.gameObject?.GetComponentInChildren<PrefabIdentifier>()?.Id;
            _registeredDevices = new Dictionary<string, FcsDevice>();
            _baseTechLights = new Dictionary<string, TechLight>();
            Initialize(habitat);
            Player_Patch.OnPlayerUpdate += PowerConsumption;
            Player_Patch.OnPlayerUpdate += PowerStateCheck;

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
                
                //TODO ReEnable
                //DockingManager.ToggleIsEnabled(_savedData?.AllowDocking ?? false);
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
            return Habitat.powerRelay.GetPowerStatus();
        }

        #endregion

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
            Managers.Add(manager);
            QuickLogger.Debug($"Manager Count = {Managers.Count}", true);
            OnManagerCreated?.Invoke(manager);
            return manager;
        }

        public static BaseManager FindManager(SubRoot subRoot)
        {
            var baseManager = FindManager(subRoot.gameObject.GetComponentInChildren<PrefabIdentifier>()?.Id);
            if (baseManager == null)
            {
                QuickLogger.Debug("[FindManager - SubRoot] returned null");
            }
            return baseManager ?? CreateNewManager(subRoot);
        }

        public static BaseManager FindManager(string instanceID)
        {
            var manager = Managers.Find(x => x.BaseID == instanceID);
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
            var subRoot = gameObject.GetComponentInParent<SubRoot>() ?? gameObject.GetComponentInChildren<SubRoot>();
            if (subRoot == null)
            {
                QuickLogger.Debug($"[BaseManager] SubRoot Returned null");
            }
            return subRoot != null ? FindManager(subRoot) : null;
        }

        public static void RemoveDestroyedBases()
        {
            for (int i = Managers.Count - 1; i > -1; i--)
            {
                if (Managers[i].Habitat == null)
                {
                    Managers.RemoveAt(i);
                }
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

            if (Habitat.isCyclops)
            {
                var count = Managers.Count(x => x.Habitat.isCyclops);
                return $"Cyclops {count}";
            }
            else
            {
                var count = Managers.Count(x => x.Habitat.isBase);
                return $"Base {count}";
            }
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
            }
        }

        private void PowerConsumption()
        {
            if (_registeredDevices == null) return;
            _timeLeft -= DayNightCycle.main.deltaTime;
            if (_timeLeft <= 0)
            {
                //Take power from the base
                for (int i = _registeredDevices.Count - 1; i >= 0; i--)
                {
                    var device = _registeredDevices.ElementAt(i);
                    if (device.Value.DoesTakePower && device.Value.IsOperational && Habitat.powerRelay != null)
                    {
                        var num = 1f * DayNightCycle.main.dayNightSpeed;
                        Habitat.powerRelay.ConsumeEnergy(device.Value.GetPowerUsage() * num, out float amountConsumed);
                    }
                }

                _timeLeft = 1f;
            }
        }

        public bool HasEnoughPower(float power)
        {
            if (Habitat.powerRelay == null)
            {
                QuickLogger.DebugError("Habitat is null");
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

        /// <summary>
        /// Checks to see if the device is registered at base doesnt take in count of being contructed
        /// </summary>
        /// <param name="tabID"></param>
        /// <returns></returns>
        public bool DeviceBuilt(string tabID)
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
            if (DeviceBuilt(tabID))
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


            var i = 0;
            var device = TrackedResources[techType];

            foreach (FcsDevice controller in device.Servers)
            {
                i += controller.GetItemCount(techType);
            }

            return i;
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

            return false;
        }

        public Pickupable TakeItem(TechType techType, StorageLocation storageFilter = StorageLocation.All)
        {


            if (storageFilter == StorageLocation.Servers || storageFilter == StorageLocation.All)
            {
                foreach (IDSSRack baseRack in BaseRacks)
                {
                    if (baseRack.HasItem(techType))
                    {
                        return baseRack.RemoveItemFromRack(techType);
                    }
                }
            }


            if (storageFilter == StorageLocation.StorageLockers || storageFilter == StorageLocation.All)
            {
                foreach (StorageContainer locker in BaseStorageLockers)
                {
                    if (locker.container.Contains(techType))
                    {
                        return locker.container.RemoveItem(techType);
                    }
                }
            }


            if (storageFilter == StorageLocation.AlterraStorage || storageFilter == StorageLocation.All)
            {
                foreach (FcsDevice device in BaseFcsStorage)
                {
                    if (device.GetStorage().ItemsContainer.Contains(techType))
                    {
                        return device.RemoveItemFromContainer(techType);
                    }
                }
            }
            
            return null;
        }

        private Dictionary<TechType, int> _item = new Dictionary<TechType, int>();
        private BaseSaveData _savedData;
        private bool _hasBreakerTripped;

        public Dictionary<TechType, int> GetItemsWithin(StorageLocation location = StorageLocation.All)
        {
            _item.Clear();

            switch (location)
            {
                case StorageLocation.All:
                    return TrackedResources.ToDictionary(x => x.Key, x => x.Value.Amount);
                case StorageLocation.Servers:
                    foreach (KeyValuePair<TechType, TrackedResource> resource in TrackedResources)
                    {
                        foreach (FcsDevice device in resource.Value.Servers)
                        {
                            CalculateItems(_item, resource, device);
                        }
                    }
                    return _item;
                case StorageLocation.StorageLockers:
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
                case StorageLocation.AlterraStorage:
                    foreach (KeyValuePair<TechType, TrackedResource> resource in TrackedResources)
                    {
                        foreach (FcsDevice device in resource.Value.AlterraStorage)
                        {
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
                item[resource.Key] += device.GetStorage().ItemsContainer.GetCount(resource.Key);
            }
            else
            {
                item.Add(resource.Key, device.GetStorage().ItemsContainer.GetCount(resource.Key));
            }
        }

        public bool ContainsItem(TechType techType)
        {
            return TrackedResources.ContainsKey(techType);
        }

        public void AddItemsToTracker(FcsDevice server,TechType item, int amountToAdd = 1)
        {
            QuickLogger.Debug($"AddItemsToTracker: DSSServerController || {item.AsString()} || {amountToAdd} ");

            if (server.TabID == "AS")
            {
                if (TrackedResources.ContainsKey(item))
                {
                    TrackedResources[item].Amount = TrackedResources[item].Amount + amountToAdd;
                    TrackedResources[item].AlterraStorage.Add(server);
                }
                else
                {
                    TrackedResources.Add(item, new TrackedResource()
                    {
                        TechType = item,
                        Amount = amountToAdd,
                        AlterraStorage = new HashSet<FcsDevice>() { server }
                    });
                }
            }
            else
            {
                if (TrackedResources.ContainsKey(item))
                {
                    TrackedResources[item].Amount = TrackedResources[item].Amount + amountToAdd;
                    TrackedResources[item].Servers.Add(server);
                }
                else
                {
                    TrackedResources.Add(item, new TrackedResource()
                    {
                        TechType = item,
                        Amount = amountToAdd,
                        Servers = new HashSet<FcsDevice>() { server }
                    });
                }
            }
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
            if (BaseStorageLockers.Contains(sc)) return;
            BaseStorageLockers.Add(sc);
            TrackStorageContainer(sc);
        }

        private void TrackExistingStorageContainers()
        {
            StorageContainer[] containers = Habitat.GetComponentsInChildren<StorageContainer>();
            foreach (StorageContainer sc in containers)
            {
                TrackStorageContainer(sc);
            }
        }

        private void TrackStorageContainer(StorageContainer sc)
        {
            if (sc == null || sc.container == null)
            {
                return;
            }

            foreach (string notTrackedObject in DONT_TRACK_GAMEOBJECTS)
            {
                if (sc.gameObject.name.ToLower().Contains(notTrackedObject))
                {
                    return;
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
        }

        public void AlertNewFcsStoragePlaced(FcsDevice alterraStorage)
        {
            if (BaseFcsStorage.Contains(alterraStorage)) return;
            BaseFcsStorage.Add(alterraStorage);
            TrackFcsStorage(alterraStorage);
        }

        private void TrackFcsStorage(FcsDevice sc)
        {
            if (sc == null || sc.GetStorage()?.ItemsContainer == null)
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

            foreach (var item in sc.GetStorage().ItemsContainer.GetItemTypes())
            {
                for (int i = 0; i < sc.GetStorage().ItemsContainer.GetCount(item); i++)
                {
                    AddItemsToTracker(sc, item);
                }
            }

            sc.GetStorage().ItemsContainer.onAddItem += (item) => AddItemsToTracker(sc, item.item.GetTechType());
            sc.GetStorage().ItemsContainer.onRemoveItem += (item) => RemoveItemsFromTracker(sc, item.item.GetTechType());
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
                //BaseManager.SendNotification();
            }
        }

        #endregion

        public int GetTotal(StorageLocation location)
        {
            return GetItemsWithin(location).Sum(x=>x.Value);
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

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {

            QuickLogger.Debug($"Checking if allowed {_dumpContainer.GetItemCount() + 1}", true);

            //TODO Check filter first


            int availableSpace = 0;
            foreach (IDSSRack baseRack in BaseRacks)
            {
                availableSpace += baseRack.GetFreeSpace();
            }

            var result = availableSpace >= _dumpContainer.GetItemCount() + 1;
            QuickLogger.Debug($"Allowed result: {result}", true);
            return result;
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
            }
            catch (Exception e)
            {
                QuickLogger.Debug(e.Message, true);
                QuickLogger.Debug(e.StackTrace);
                PlayerInteractionHelper.GivePlayerItem(item);
                return false;
            }
            return true;
        }
        public static bool AddItemToNetwork(InventoryItem item, BaseManager manager)
        {
            if (manager != null)
            {
                foreach (IDSSRack baseRack in manager.BaseRacks)
                {
                    if (baseRack.ItemAllowed(item, out var server))
                    {
                        server?.AddItemToMountedServer(item);
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsFilterAddedWithType(TechType techType)
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
                    BlackList = baseManager.DockingBlackList
                };
            }
        }

        public bool GetBreakerState()
        {
            return HasBreakerTripped;
        }
    }

    public class BaseSaveData
    {
        public string InstanceID { get; set; }
        public string BaseName { get; set; }
        public bool AllowDocking { get; set; }
        public bool HasBreakerTripped { get; set; }
        public List<TechType> BlackList { get; set; }
    }

    public enum StorageLocation
    {
        All,
        Servers,
        StorageLockers,
        AlterraStorage
    }

    public struct TrackedLight
    {
        public BaseManager Manager { get; set; }
        public TechLight TechLight { get; set; }
        public BaseSpotLight SpotLight { get; set; }
    }

    public class FCSTechLigtController : MonoBehaviour
    {
        private BaseManager _manager;

        public void Initialize(BaseManager manager)
        {
            _manager = manager;
        }


        private void Update()
        {

        }
    }

    public class TrackedResource
    {
        public TechType TechType { get; set; }
        public int Amount { get; set; }
        public HashSet<StorageContainer> StorageContainers { get; set; } = new HashSet<StorageContainer>();
        public HashSet<FcsDevice> Servers { get; set; } = new HashSet<FcsDevice>();
        public HashSet<FcsDevice> AlterraStorage { get; set; } = new HashSet<FcsDevice>();
    }
}
