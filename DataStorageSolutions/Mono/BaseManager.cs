using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Debug;
using DataStorageSolutions.Interfaces;
using DataStorageSolutions.Model;
using DataStorageSolutions.Patches;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using FCSTechFabricator.Components;
using FCSTechFabricator.Enums;
using UnityEngine;

namespace DataStorageSolutions.Mono
{
    internal class BaseManager
    {
        private string _baseName;
        private BaseSaveData _savedData;
        private bool _hasBreakerTripped;
        private bool _isInitialized;
        private static bool _allowedToNotify;

        internal bool IsVisible
        {
            get
            {
                if (Habitat.isCyclops)
                {
                    return !_hasBreakerTripped;
                }

                var antenna = GetCurrentBaseAntenna();
                return antenna != null && antenna.IsVisible();
            }
        }
        internal SubRoot Habitat { get; }
        internal string InstanceID { get; }
        internal FCSPowerStates BasePrevPowerState { get; set; }
        internal NameController NameController { get; private set; }
        internal DSSVehicleDockingManager DockingManager { get; set; }
        internal bool IsOperational => !_hasBreakerTripped && BasePowerManager.HasPower();
        internal Dictionary<string, FCSConnectableDevice> FCSConnectables { get; set; } = new Dictionary<string, FCSConnectableDevice>();
        internal static Action<Player> OnPlayerTick { get; set; }
        internal static List<BaseManager> Managers { get; } = new List<BaseManager>();
        internal static readonly List<IBaseAntenna> BaseAntennas = new List<IBaseAntenna>();
        internal static Dictionary<string,FCSOperation> Operations { get; set; } = new Dictionary<string,FCSOperation>();
        internal BasePowerManager BasePowerManager { get; set; }
        internal BaseStorageManager StorageManager { get; set; }
        internal readonly HashSet<DSSOperatorController> BaseOperators = new HashSet<DSSOperatorController>();
        internal readonly HashSet<DSSTerminalController> BaseTerminals = new HashSet<DSSTerminalController>();
        internal readonly HashSet<DSSAutoCrafterController> BaseAutoCrafters = new HashSet<DSSAutoCrafterController>();

        #region Default Constructor

        public BaseManager(SubRoot habitat)
        {
            if (habitat == null) return;
            Habitat = habitat;
            InstanceID = habitat.gameObject.gameObject?.GetComponentInChildren<PrefabIdentifier>()?.Id;
            Initialize(habitat);
        }

        #endregion

        #region Initialization

        private void Initialize(SubRoot habitat)
        {
            if (_isInitialized) return;
            ReadySaveData();
            FCSConnectableAwake_Patcher.AddEventHandlerIfMissing(AlertedNewFCSConnectablePlaced);
            FCSConnectableDestroy_Patcher.AddEventHandlerIfMissing(AlertedFCSConnectableDestroyed);

            if (StorageManager == null)
            {
                StorageManager = new BaseStorageManager();
                StorageManager.Initialize(this);
            }

            if (NameController == null)
            {
                NameController = new NameController();
                NameController.Initialize(AuxPatchers.Submit(), Mod.AntennaFriendlyName);
                NameController.OnLabelChanged += OnLabelChangedMethod;
                NameController.SetCurrentName(string.IsNullOrEmpty(_savedData?.InstanceID) ? GetDefaultName() : _savedData?.BaseName);
            }

            if (BasePowerManager == null)
            {
                BasePowerManager = habitat.gameObject.EnsureComponent<BasePowerManager>();
                BasePowerManager.Initialize(this);
                BasePowerManager.OnPowerUpdate += (state, manager) =>
                {
                    foreach (DSSRackController baseRack in StorageManager.BaseRacks)
                    {
                        baseRack.DisplayManager.ChangeScreenPowerState(state);
                    }

                    foreach (DSSTerminalController terminal in BaseTerminals)
                    {
                        terminal.DisplayManager.ChangeScreenPowerState(state);
                    }
                };
            }

            Mod.OnAntennaBuilt += isBuilt =>
            {
                SendNotification();
            };

            if (DockingManager == null)
            {
                DockingManager = habitat.gameObject.AddComponent<DSSVehicleDockingManager>();
                DockingManager.Initialize(this);
                DockingManager.ToggleIsEnabled(_savedData?.AllowDocking ?? false);
            }

            _hasBreakerTripped = _savedData?.HasBreakerTripped ?? false;
            
            if (Mod.GetSaveData().Operations != null)
            {
                Operations = Mod.GetSaveData().Operations;
            }

            Player.main.StartCoroutine(TransferItems());

            _isInitialized = true;
        }
        
        #endregion

        #region Save Handler

        private void ReadySaveData()
        {
            _savedData = Mod.GetBaseSaveData(InstanceID);
        }

        #endregion

        #region Base Manager Operations

        internal void ToggleBreaker()
        {
            if (HasAntenna(true))
            {
                SendBaseMessage(_hasBreakerTripped);
            }

            _hasBreakerTripped = !_hasBreakerTripped;
        }

        private static BaseManager CreateNewManager(SubRoot habitat)
        {
            QuickLogger.Debug($"Creating new manager", true);
            var manager = new BaseManager(habitat);
            var baseConnectable  = habitat.gameObject.EnsureComponent<BaseConnectable>();
            baseConnectable.BaseManager = manager;
            Managers.Add(manager);
            QuickLogger.Debug($"Manager Count = {Managers.Count}", true);
            return manager;
        }

        internal static BaseManager FindManager(SubRoot subRoot)
        {
#if DEBUG
            QuickLogger.Debug($"Processing SubRoot = {subRoot.GetInstanceID()} || Name {subRoot.GetSubName()}");
#endif
            var baseManager = FindManager(subRoot.gameObject.GetComponentInChildren<PrefabIdentifier>()?.Id);
            return baseManager ?? CreateNewManager(subRoot);
        }

        internal static BaseManager FindManager(string instanceID)
        {
            var manager = Managers.Find(x => x.InstanceID == instanceID);
            return manager;
        }

        /// <summary>
        /// Find the manager this gameObject is attached to.
        /// </summary>
        /// <param name="gameObject">The gameObject the we are trying to find the base for</param>
        /// <returns></returns>
        internal static BaseManager FindManager(GameObject gameObject)
        {
            var subRoot = gameObject.GetComponentInParent<SubRoot>() ?? gameObject.GetComponentInChildren<SubRoot>();

            return subRoot != null ? FindManager(subRoot) : null;
        }

        internal static void RemoveDestroyedBases()
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
        /// Opens the Name Controller Dialog
        /// </summary>
        internal void ChangeBaseName()
        {
            NameController.Show();
        }

        /// <summary>
        /// Gets the stored base Name from the
        /// </summary>
        /// <returns></returns>
        internal string GetBaseName()
        {
            return _baseName;
        }

        /// <summary>
        /// Sets the base name field
        /// </summary>
        /// <param name="baseName"></param>
        internal void SetBaseName(string baseName)
        {
            _baseName = baseName;
        }

        /// <summary>
        /// Creates a default base name and number index based on the count of items
        /// </summary>
        /// <returns></returns>
        internal string GetDefaultName()
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

        internal void SendBaseMessage(bool state)
        {
            QuickLogger.Message(string.Format(AuxPatchers.BaseOnOffMessage(), GetBaseName(), state ? AuxPatchers.Online() : AuxPatchers.Offline()), true);
        }

        private void OnPowerUpdate(FCSPowerStates state, BaseManager manager)
        {
            if (manager == null || manager.BasePrevPowerState == state) return;

            manager.BasePrevPowerState = state;

            Mod.OnBaseUpdate?.Invoke();
        }

        #endregion

        #region Base Manager Conditionals
        
        internal bool GetHasBreakerTripped()
        {
            return _hasBreakerTripped;
        }

        internal bool HasAntenna(bool ignoreVisibleCheck = false)
        {
            if (Habitat.isCyclops)
            {
                return true;
            }

            return GetCurrentBaseAntenna(ignoreVisibleCheck) != null;
        }

        private bool HasARack()
        {
            return StorageManager?.BaseRacks?.Count > 0;
        }

        #endregion

        #region FCSConnectable

        private FCSConnectableDevice FindDevice(string deviceID)
        {
            if (string.IsNullOrWhiteSpace(deviceID) || FCSConnectables.Count <= 0) return null;

            foreach (var connectable in FCSConnectables)
            {
                if (connectable.Value.UnitID.Equals(deviceID)) return connectable.Value;
                QuickLogger.Info($"Found Device : {connectable.Value.UnitID}");
            }

            return null;
        }

        private void TrackNewFCSConnectable(FCSConnectableDevice obj)
        {
            if (obj == null) return;
            BaseManager manager = FindManager(obj.gameObject);

            QuickLogger.Debug($"Object Base ID: {manager?.InstanceID} || Current Base ID: {InstanceID}", true);

            if (manager == null || Habitat == null) return;
            
            if (manager.InstanceID == InstanceID)
            {
#if DEBUG
                QuickLogger.Debug($"FCSConnectable Found: {obj.UnitID}", true);
                QuickLogger.Debug($"FCSConnectable found in base: {Habitat?.name}", true);
#endif

                obj.GetStorage().OnContainerAddItem += OnContainerAddItem;
                obj.GetStorage().OnContainerRemoveItem += OnContainerRemoveItem;
                QuickLogger.Debug("Adding FCSConnectable");
                FCSConnectables.Add(obj.GetPrefabIDString(), obj);
                if (obj.IsVisible)
                {
                    StorageManager.RefreshFCSConnectable(obj,true);
                }
                obj.OnIsVisibleToggled += (connectable, value) =>
                {
                    StorageManager.RefreshFCSConnectable(connectable,value);
                };
                QuickLogger.Debug("Added FCSConnectable");
                if (!Operations.ContainsKey(obj.UnitID))
                {
                    var operation = new FCSOperation {Manager = manager, ConnectableDevice = obj, ID = obj.UnitID};
                    operation.Initialize();
                    Operations.Add(obj.UnitID, operation);
                }
                else
                {
                    var operation = Operations[obj.UnitID];
                    operation.Manager = manager;
                    operation.ConnectableDevice = obj;
                }
                RefreshBaseOperators();
            }
        }

        private void OnContainerRemoveItem(FCSConnectableDevice connectable,TechType techType)
        {
            StorageManager.RemoveItemsFromTracker(connectable,techType);
        }

        private void OnContainerAddItem(FCSConnectableDevice connectable, TechType techType)
        {
            StorageManager.AddItemsToTracker(connectable, techType);
        }

        private void RefreshBaseOperators()
        {
            QuickLogger.Debug($"Refreshing Base Operators",true);
            foreach (DSSOperatorController baseOperator in BaseOperators)
            {
                baseOperator.DisplayManager.Refresh();
            }
        }

        private void AlertedNewFCSConnectablePlaced(FCSConnectableDevice obj)
        {
            if (obj != null)
            {
                QuickLogger.Debug("OBJ Not NULL", true);
                TrackNewFCSConnectable(obj);
            }
        }

        private void AlertedFCSConnectableDestroyed(FCSConnectableDevice obj)
        {
            if (obj == null || obj.GetPrefabIDString() == null) return;

            BaseManager manager = FindManager(obj.gameObject);

            if(manager == null) return;

            if (manager.InstanceID == InstanceID)
            {
                var operation = Operations.FirstOrDefault(x => x.Key == obj.UnitID);
                operation.Value.Destroy();

                obj.GetStorage().OnContainerAddItem -= OnContainerAddItem;
                obj.GetStorage().OnContainerRemoveItem -= OnContainerRemoveItem;
                QuickLogger.Debug("OBJ Not NULL", true);
                FCSConnectables.Remove(obj.GetPrefabIDString());

                QuickLogger.Debug("Removed FCSConnectable");
                RefreshBaseOperators();
                Operations.Remove(obj.UnitID);
            }
        }



        #endregion

        #region Name Controller

        private void OnLabelChangedMethod(string newName, NameController controller)
        {
            SetBaseName(newName);
            Mod.OnBaseUpdate?.Invoke();
        }

        #endregion

        #region Terminal Registration

        internal void RegisterTerminal(DSSTerminalController unit)
        {
            if (!BaseTerminals.Contains(unit) && unit.IsConstructed)
            {
                BaseTerminals.Add(unit);
                QuickLogger.Debug($"Add Unit : {unit.GetPrefabIDString()}", true);
            }
        }

        internal void RemoveTerminal(DSSTerminalController unit)
        {
            if (!BaseTerminals.Contains(unit)) return;
            BaseTerminals.Remove(unit);
            QuickLogger.Debug($"Removed Unit : {unit.GetPrefabIDString()}", true);
        }

        #endregion

        #region Rack Registration

        internal void RegisterRack(DSSRackController unit)
        {
            if (!StorageManager.BaseRacks.Contains(unit) && unit.IsConstructed)
            {
                StorageManager.BaseRacks.Add(unit);

                if (BasePowerManager.HasPower())
                {
                    unit.DisplayManager.PowerOnDisplay();
                }
                else
                {
                    unit.DisplayManager.PowerOffDisplay();
                }
#if DEBUG
                QuickLogger.Debug($"Add Unit : {unit.GetPrefabIDString()} || Base Has Power: {BasePowerManager.HasPower()}", true);
#endif
            }
        }

        internal static void RemoveRack(DSSRackController unit)
        {
            foreach (BaseManager manager in Managers)
            {
                if (!manager.StorageManager.BaseRacks.Contains(unit)) continue;
                manager.StorageManager.BaseRacks.Remove(unit);
#if DEBUG
                QuickLogger.Debug($"Removed Unit : {unit.GetPrefabIDString()}", true);
#endif
            }
        }

        #endregion

        #region Antenna Registration

        internal static void RegisterAntenna(DSSAntennaController unit)
        {
            if (!BaseAntennas.Contains(unit) && unit.IsConstructed)
            {
                if (!unit.Manager.HasAntenna())
                {
                    unit.Manager.SendBaseMessage(true);
                }

                BaseAntennas.Add(unit);
                QuickLogger.Debug($"Add Unit : {unit.GetPrefabID()}", true);
            }

            SendNotification();
        }

        internal static void RemoveAntenna(DSSAntennaController unit)
        {
            if (BaseAntennas.Contains(unit))
            {
                BaseAntennas.Remove(unit);

                if (!unit.Manager.HasAntenna())
                {
                    unit.Manager.SendBaseMessage(false);
                }
            }

            SendNotification();
        }

        #endregion

        #region Antenna Information

        internal IBaseAntenna GetCurrentBaseAntenna(bool ignoreVisible = false)
        {
            return ignoreVisible ? BaseAntennas.FirstOrDefault(antenna => antenna != null && antenna.Manager.InstanceID == InstanceID) :
                BaseAntennas.FirstOrDefault(antenna => antenna != null && antenna.Manager.InstanceID == InstanceID && antenna.IsVisible());
        }

        internal IEnumerable<IBaseAntenna> GetBaseAntennas()
        {
            foreach (IBaseAntenna antenna in BaseAntennas)
            {
                if (antenna.Manager.InstanceID == InstanceID)
                {
                    yield return antenna;
                }
            }
        }

        #endregion

        internal static void RegisterServer(DSSServerController controller)
        {
            if (!BaseStorageManager.GlobalServers.Contains(controller))
            {
                var data = Mod.GetServerSaveData(controller.GetPrefabID());
                if (!string.IsNullOrWhiteSpace(data.PrefabID))
                {
                    controller.SetFilters(data.ServerFilters);
                    //controller.MoveItemsToStorageRoot(data.ServerItems);
                }
                BaseStorageManager.GlobalServers.Add(controller);
            }
        }

        internal void RegisterServerInBase(DSSServerController serverController)
        {
            serverController.Manager.StorageManager.RegisterServerInBase(serverController);
        }

        internal void UnRegisterServerFromBase(DSSServerController serverController)
        {
            serverController.Manager.StorageManager.RemoveServerFromBase(serverController);
        }

        internal static void UnRegisterServer(DSSServerController dssServerController)
        {
            BaseStorageManager.GlobalServers.Remove(dssServerController);
        }

        internal static void UpdateGlobalTerminals(bool isVehiclesUpdate = false)
        {
            foreach (BaseManager manager in Managers)
            {
                if (isVehiclesUpdate)
                {
                    manager.UpdateBaseTerminals(true);
                }
                else
                {
                    manager.UpdateBaseTerminals();
                }
            }
        }

        private void UpdateBaseTerminals(bool isVehicleUpdate = false)
        {
            foreach (DSSTerminalController terminal in BaseTerminals)
            {
                if (isVehicleUpdate)
                {
                    terminal.OnVehicleUpdate(this);
                }
                else
                {
                    terminal.UpdateScreen();
                }
            }
        }

        internal static IEnumerable<BaseSaveData> GetBaseSaveData()
        {
            foreach (var manager in Managers)
            {
                yield return new BaseSaveData{BaseName = manager.GetBaseName(),InstanceID = manager.InstanceID};
            }
        }

        internal static void SetAllowedToNotify(bool value)
        {
            if (DebugMenu.main.IsOpen)
            {
                _allowedToNotify = false;
                return;
            }
            _allowedToNotify = value;
            QuickLogger.Debug($"Allow Notification set to: {_allowedToNotify}",true);
            if (value)
            {
                SendNotification();
            }
        }

        internal static void SendNotification(bool isVehicleUpdate = false)
        {
            QuickLogger.Debug($"IsAllowed to Update: {_allowedToNotify}",true);
            if (_allowedToNotify)
            {
                BaseManager.UpdateGlobalTerminals();
                if (isVehicleUpdate)
                {
                    BaseManager.UpdateGlobalTerminals(true);
                }
            }
        }

        internal void UnRegisterOperator(DSSOperatorController operatorController)
        {
            BaseOperators.Remove(operatorController);
            if (BaseOperators.Count <= 0)
            {
                UpdateBaseTerminals();
            }
        }

        internal void RegisterOperator(DSSOperatorController operatorController)
        {
            if (BaseOperators.Contains(operatorController)) return;
            BaseOperators.Add(operatorController);
            if (BaseOperators.Count == 1)
            {
                UpdateBaseTerminals();
            }
        }

        internal void UnRegisterAutoCrafter(DSSAutoCrafterController crafterController)
        {
            BaseAutoCrafters.Remove(crafterController);
            if (BaseOperators.Count <= 0)
            {

            }
        }

        internal void RegisterAutoCrafter(DSSAutoCrafterController crafterController)
        {
            if (BaseAutoCrafters.Contains(crafterController)) return;
            BaseAutoCrafters.Add(crafterController);
            if (BaseOperators.Count == 1)
            {
                
            }
        }

        internal void UpdateBaseOperators()
        {
            foreach (DSSOperatorController baseOperator in BaseOperators)
            {
                baseOperator.UpdateScreen();
            }
        }

        internal void UpdateBaseCrafters()
        {
            foreach (DSSAutoCrafterController autoCrafterController in BaseAutoCrafters)
            {
                autoCrafterController.UpdateScreen();
            }
        }

        internal static IEnumerator TransferItems()
        {
            while (true)
            {

                yield return new WaitForSeconds(1f);
                foreach (KeyValuePair<string, FCSOperation> operation in Operations)
                {
                    if (!operation.Value.IsTransferAllowed) continue;
                    foreach (TransferOperationData data in operation.Value.TransferRequestOperations)
                    {
                        if (operation.Value.ConnectableDevice.GetItemCount(data.TransferRequestItem) < data.TransferRequestAmount &&
                            operation.Value.ConnectableDevice.CanBeStored(1, data.TransferRequestItem))
                        {
                            if (operation.Value.Manager.StorageManager.ServersContainsItem(data.TransferRequestItem))
                            {
                                QuickLogger.Debug("Container contains item trying to move");
                                var item = operation.Value.Manager.StorageManager.RemoveItemFromBaseServersOnly(data.TransferRequestItem, false);

                                if (!operation.Value.ConnectableDevice.AddItemToContainer(item.ToInventoryItem(), out string reason))
                                {
                                    //TODO add to logger;
                                }
                            }
                        }
                        else
                        {
                            yield return null;
                        }
                    }
                }
            }
        }

        internal static Dictionary<string, FCSOperation> GetOperationSaveData()
        {
            return Operations;
        }

        internal static FCSConnectableDevice FindConnectableDevice(string value)
        {
            foreach (BaseManager manager in Managers)
            {
                foreach (KeyValuePair<string, FCSConnectableDevice> connectable in manager.FCSConnectables)
                {
                    if (connectable.Value.UnitID.Equals(value))
                    {
                        return connectable.Value;
                    }
                }
            }

            return null;
        }
    }
}