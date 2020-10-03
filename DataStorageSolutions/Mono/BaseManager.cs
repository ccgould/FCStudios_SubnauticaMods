using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Debug;
using DataStorageSolutions.Helpers;
using DataStorageSolutions.Interfaces;
using DataStorageSolutions.Model;
using DataStorageSolutions.Patches;
using FCSCommon.Utilities;
using FCSTechFabricator.Abstract;
using FCSTechFabricator.Components;
using FCSTechFabricator.Enums;
using FCSTechFabricator.Objects;
using UnityEngine;

namespace DataStorageSolutions.Mono
{
    internal class BaseManager
    {
        private string _baseName;
        private BaseSaveData _savedData;
        private bool _hasBreakerTripped;

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

        internal static Action OnPlayerTick { get; set; }
        internal static List<BaseManager> Managers { get; } = new List<BaseManager>();
        internal static readonly List<IBaseAntenna> BaseAntennas = new List<IBaseAntenna>();
        internal static List<FCSOperation> Crafts { get; set; } = new List<FCSOperation>();
        internal static List<FCSOperation> Operations { get; set; } = new List<FCSOperation>();
        public BasePowerManager BasePowerManager { get; set; }
        public BaseStorageManager StorageManager { get; set; }
        internal readonly HashSet<DSSOperatorController> BaseOperators = new HashSet<DSSOperatorController>();
        internal readonly HashSet<DSSTerminalController> BaseTerminals = new HashSet<DSSTerminalController>();
        private bool _isInitialized;
        private static bool _allowedToNotify;

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
            habitat.gameObject.EnsureComponent<BaseConnectable>();
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
            SubRoot newSeaBase = obj.GetComponentInParent<SubRoot>(); //obj?.gameObject?.transform?.parent?.gameObject;
            var fcsConnectableBase = FindManager(newSeaBase?.GetComponentInChildren<PrefabIdentifier>().Id);

#if DEBUG
            QuickLogger.Debug($"FCSConnectable Base Found: {newSeaBase?.name}", true);
            QuickLogger.Debug($"FCSConnectable found in base: {Habitat?.name}", true);
#endif

            if (fcsConnectableBase == null || Habitat) return;

            if (fcsConnectableBase.Habitat == Habitat)
            {
                obj.GetStorage().OnContainerUpdate += OnFCSConnectableContainerUpdate;
                QuickLogger.Debug("Adding FCSConnectable");
                FCSConnectables.Add(obj.GetPrefabIDString(), obj);
                QuickLogger.Debug("Added FCSConnectable");
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

            QuickLogger.Debug("OBJ Not NULL", true);
            FCSConnectables.Remove(obj.GetPrefabIDString());
            QuickLogger.Debug("Removed FCSConnectable");
        }

        private void OnFCSConnectableContainerUpdate(int arg1, int arg2)
        {
            Mod.OnBaseUpdate?.Invoke();
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

        public void UnRegisterServerFromBase(DSSServerController serverController)
        {
            serverController.Manager.StorageManager.RemoveServerFromBase(serverController);
        }

        public static void UnRegisterServer(DSSServerController dssServerController)
        {
            BaseStorageManager.GlobalServers.Remove(dssServerController);
        }

        public static void UpdateGlobalTerminals(bool isVehiclesUpdate = false)
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

        public static IEnumerable<BaseSaveData> GetBaseSaveData()
        {
            foreach (var manager in Managers)
            {
                yield return new BaseSaveData{BaseName = manager.GetBaseName(),InstanceID = manager.InstanceID};
            }
        }

        public static void SetAllowedToNotify(bool value)
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

        public void UnRegisterOperator(DSSOperatorController operatorController)
        {
            BaseOperators.Remove(operatorController);
        }

        public void RegisterOperator(DSSOperatorController operatorController)
        {
            if (BaseOperators.Contains(operatorController)) return;
            BaseOperators.Add(operatorController);
        }
    }
}
