using System;
using System.Collections.Generic;
using System.Linq;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Helpers;
using DataStorageSolutions.Interfaces;
using DataStorageSolutions.Mono;
using DataStorageSolutions.Patches;
using DataStorageSolutions.Structs;
using FCSCommon.Enums;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using FCSTechFabricator.Components;
using FCSTechFabricator.Enums;
using FCSTechFabricator.Interfaces;
using UnityEngine;

namespace DataStorageSolutions.Model
{
    internal class BaseManager : IFCSStorage
    {
        private string _baseName;
        private BaseSaveData _savedData;
        private bool _hasBreakerTripped;


        internal static List<BaseManager> Managers { get; } = new List<BaseManager>();
        internal static readonly List<IBaseAntenna> BaseAntennas = new List<IBaseAntenna>();
        internal string InstanceID { get; }
        internal readonly HashSet<DSSRackController> BaseRacks = new HashSet<DSSRackController>();
        internal readonly HashSet<DSSOperatorController> BaseOperators  = new HashSet<DSSOperatorController>();

        internal readonly HashSet<DSSTerminalController> BaseTerminals = new HashSet<DSSTerminalController>();
        internal readonly Dictionary<TechType,int> TrackedItems = new Dictionary<TechType, int>();
        internal bool GivePlayerItem { get; set; } = true;

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
        internal FCSPowerStates PrevPowerState { get; set; }
        internal SubRoot Habitat { get; }
        internal DumpContainer DumpContainer { get; private set; }
        public int GetContainerFreeSpace { get; }
        public bool IsFull { get; }
        internal NameController NameController { get; private set; }
        internal DSSVehicleDockingManager DockingManager { get; set; }
        internal bool IsOperational => !_hasBreakerTripped || BaseHasPower();
        internal static Action OnPlayerTick { get; set; }
        internal Action<BaseManager> OnVehicleStorageUpdate { get; set; }
        internal Action<List<Vehicle>,BaseManager> OnVehicleUpdate { get; set; }
        internal Action<BaseManager> OnContainerUpdate { get; set; }
        public bool ContainsItem(TechType techType)
        {
             return TrackedItems.ContainsKey(techType);
        }

        internal Dictionary<string, FCSConnectableDevice> FCSConnectables { get; set; } = new Dictionary<string, FCSConnectableDevice>();

        Action<int, int> IFCSStorage.OnContainerUpdate { get; set; }

        private void Initialize(SubRoot habitat)
        {
            ReadySaveData();
            FCSConnectableAwake_Patcher.AddEventHandlerIfMissing(AlertedNewFCSConnectablePlaced);
            FCSConnectableDestroy_Patcher.AddEventHandlerIfMissing(AlertedFCSConnectableDestroyed);
            GetFCSConnectables();


            if (NameController == null)
            {
                NameController = new NameController();
                NameController.Initialize(AuxPatchers.Submit(), Mod.AntennaFriendlyName);
                NameController.OnLabelChanged += OnLabelChangedMethod;

                if (string.IsNullOrEmpty(_savedData?.InstanceID))
                {
                    NameController.SetCurrentName(GetDefaultName());
                }
                else
                {
                    NameController.SetCurrentName(_savedData?.BaseName);
                }
            }

            if (DumpContainer == null)
            {
                DumpContainer = habitat.gameObject.AddComponent<DumpContainer>();
                DumpContainer.Initialize(habitat.transform, AuxPatchers.BaseDumpReceptacle(), AuxPatchers.NotAllowed(),
                    AuxPatchers.CannotBeStored(), this);
            }
            
            if (DockingManager == null)
            {
                DockingManager = habitat.gameObject.AddComponent<DSSVehicleDockingManager>();
                DockingManager.Initialize(habitat,this);
                DockingManager.ToggleIsEnabled(_savedData?.AllowDocking ?? false);
            }

            _hasBreakerTripped = _savedData?.HasBreakerTripped ?? false;
        }
        
        private void ReadySaveData()
        {
            _savedData = Mod.GetBaseSaveData(InstanceID);
        }
        
        private void OnLabelChangedMethod(string newName, NameController controller)
        {
            SetBaseName(newName);
            Mod.OnBaseUpdate?.Invoke();
        }

        private static BaseManager CreateNewManager(SubRoot habitat)
        {
            QuickLogger.Debug($"Creating new manager", true);
            var manager = new BaseManager(habitat);
            Managers.Add(manager);
            QuickLogger.Debug($"Manager Count = {Managers.Count}", true);
            return manager;
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

        private static void GetStoredData(DSSRackController rackController, Dictionary<TechType, int> data)
        {
            if (rackController == null)
            {
                return;
            }

            foreach (KeyValuePair<TechType, int> storedItems in rackController.GetItemsWithin())
            {
                CollectServerItems(storedItems, data);
            }
        }

        private static void CollectServerItems(KeyValuePair<TechType, int> storedItem, Dictionary<TechType, int> data)
        {
            if (data.ContainsKey(storedItem.Key))
            {
                data[storedItem.Key] += storedItem.Value;
            }
            else
            {
                data.Add(storedItem.Key, storedItem.Value);
            }
        }

        internal void ToggleBreaker()
        {
            if (HasAntenna(true))
            {
                SendBaseMessage(_hasBreakerTripped);
            }

            _hasBreakerTripped = !_hasBreakerTripped;
        }
        
        private bool BaseHasPower()
        {
            if(Habitat != null)
            {
                if (Habitat.powerRelay.GetPowerStatus() == PowerSystem.Status.Offline)
                {
                    return false;
                }

                if (Habitat.powerRelay.GetPowerStatus() == PowerSystem.Status.Normal || Habitat.powerRelay.GetPowerStatus() == PowerSystem.Status.Emergency)
                {
                    return true;
                }
            }

            return false;
        }
        
        public BaseManager(SubRoot habitat)
        {
            if (habitat == null) return;
            Habitat = habitat;
            InstanceID = habitat.gameObject.gameObject?.GetComponentInChildren<PrefabIdentifier>()?.Id;
            Initialize(habitat);
        }

        internal void AddToTrackedItems(TechType techType)
        {
            if (TrackedItems.ContainsKey(techType))
            {
                TrackedItems[techType] += 1;
#if DEBUG
                QuickLogger.Debug($"Added another {techType}", true);
#endif
            }
            else
            {
                TrackedItems.Add(techType, 1);
#if DEBUG
                QuickLogger.Debug($"Item {techType} was added to the tracking list.", true);
#endif
            }
        }

        internal void RemoveFromTrackedItems(TechType techType)
        {
            if (!TrackedItems.ContainsKey(techType)) return;

            if (TrackedItems[techType] == 1)
            {
                TrackedItems.Remove(techType);
#if DEBUG
                QuickLogger.Debug($"Removed another {techType}", true);
#endif
            }
            else
            {
                TrackedItems[techType] -= 1;
#if DEBUG
                QuickLogger.Debug($"Item {techType} is now {TrackedItems[techType]}", true);
#endif
            }
        }

        internal bool GetHasBreakerTripped()
        {
            return _hasBreakerTripped;
        }

        internal static BaseManager FindManager(SubRoot subRoot)
        {
            QuickLogger.Debug($"Processing SubRoot = {subRoot.GetInstanceID()} || Name {subRoot.GetSubName()}");
            var g = FindManager(subRoot.gameObject.GetComponentInChildren<PrefabIdentifier>()?.Id);
            var manager = Managers.Find(x => x.InstanceID == g?.InstanceID && x.Habitat == subRoot);
            return manager ?? CreateNewManager(subRoot);
        }

        internal static BaseManager FindManager(string instanceID)
        {
            var manager = Managers.Find(x => x.InstanceID == instanceID);
            return manager;
        }
        
        internal bool HasAntenna(bool ignoreVisibleCheck = false)
        {
            if (Habitat.isCyclops)
            {
                return true;
            }
            
            return GetCurrentBaseAntenna(ignoreVisibleCheck) != null;
        }
        
        internal void AddTerminal(DSSTerminalController unit)
        {
            if (!BaseTerminals.Contains(unit) && unit.IsConstructed)
            {
                BaseTerminals.Add(unit);
                unit.PowerManager.OnPowerUpdate += OnPowerUpdate;
                QuickLogger.Debug($"Add Unit : {unit.GetPrefabIDString()}", true);
            }
        }

        internal void RemoveTerminal(DSSTerminalController unit)
        {
            if (!BaseTerminals.Contains(unit)) return;
            BaseTerminals.Remove(unit);
            unit.PowerManager.OnPowerUpdate -= OnPowerUpdate;
            QuickLogger.Debug($"Removed Unit : {unit.GetPrefabIDString()}", true);
        }

        internal void AddRack(DSSRackController unit)
        {
            if (!BaseRacks.Contains(unit) && unit.IsConstructed)
            {
                BaseRacks.Add(unit);
                unit.PowerManager.OnPowerUpdate += OnPowerUpdate;
                QuickLogger.Debug($"Add Unit : {unit.GetPrefabIDString()}", true);
            }
        }
        
        internal static void RemoveRack(DSSRackController unit)
        {
            foreach (BaseManager manager in Managers)
            {
                if (!manager.BaseRacks.Contains(unit)) continue;
                manager.BaseRacks.Remove(unit); 
                unit.PowerManager.OnPowerUpdate -= OnPowerUpdate;
                QuickLogger.Debug($"Removed Unit : {unit.GetPrefabIDString()}", true);
            }
        }

        internal static void AddAntenna(DSSAntennaController unit)
        {
            if (!BaseAntennas.Contains(unit) && unit.IsConstructed)
            {
                unit.PowerManager.OnPowerUpdate += OnPowerUpdate;

                if (!unit.Manager.HasAntenna())
                {
                    unit.Manager.SendBaseMessage(true);
                }
                
                BaseAntennas.Add(unit);
                QuickLogger.Debug($"Add Unit : {unit.GetPrefabIDString()}", true);
            }
        }

        internal static void OnPowerUpdate(FCSPowerStates state, BaseManager manager)
        {
            if (manager == null ||  manager.PrevPowerState == state) return;

            manager.PrevPowerState = state;
            
            Mod.OnBaseUpdate?.Invoke();
        }

        internal void SendBaseMessage(bool state)
        {
            QuickLogger.Message(string.Format(AuxPatchers.BaseOnOffMessage(), GetBaseName(), state ? AuxPatchers.Online() : AuxPatchers.Offline()), true);
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

                unit.PowerManager.OnPowerUpdate -= OnPowerUpdate;
            }
        }
        
        public Dictionary<TechType,int> GetItemsWithin()
        {
            try
            {
                Dictionary<TechType, int> data = new Dictionary<TechType, int>();

                foreach (DSSRackController rackController in BaseRacks)
                {
                    if(rackController == null) continue;
                    GetStoredData(rackController, data);
                }

                foreach (var fcsConnectable in FCSConnectables)
                {
                    if(!fcsConnectable.Value.GetStorage().IsAllowedToRemoveItems() || !fcsConnectable.Value.IsVisible) continue;
                    var items = fcsConnectable.Value.GetItemsWithin();
                    foreach (KeyValuePair<TechType, int> item in items)
                    {
                        if (data.ContainsKey(item.Key))
                        {
                            data[item.Key] += item.Value;
                        }
                        else
                        {
                            data.Add(item.Key,item.Value);
                        }
                    }
                }

                return data;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.StackTrace);
                return null;
            }
        }

        private Pickupable PerformTakeOperation(TechType techType)
        {
            QuickLogger.Debug("Perform Take Operation",true);
            foreach (DSSRackController baseUnit in BaseRacks)
            {
                if (baseUnit.HasItem(techType))
                {
                    var data = baseUnit.GetItemDataFromServer(techType);
                    QuickLogger.Debug("Calling Take",true);

                    if (GivePlayerItem)
                    {
                        var result = baseUnit.GivePlayerItem(techType, data);
                        if (!result)
                        {
                            return null;
                            //TODO Add Message
                        }
                    }

                    GivePlayerItem = true;
                    return data.ToPickable(techType);
                }
            }

            //Check connectables
            foreach (KeyValuePair<string, FCSConnectableDevice> fcsConnectable in FCSConnectables)
            {
                Vector2int itemSize = CraftData.GetItemSize(techType);
                if (fcsConnectable.Value.ContainsItem(techType) && Inventory.main.HasRoomFor(itemSize.x,itemSize.y))
                {
                    var item = fcsConnectable.Value.RemoveItemFromContainer(techType, 1);
                    if(item == null) continue;
                    if (GivePlayerItem)
                    {
                        DSSHelpers.GivePlayerItem(item);
                    }

                    GivePlayerItem = true;
                    return item;
                }
            }

            return null;
        }

        internal void OpenDump(TransferData data)
        {
            DumpContainer.OpenStorage();
        }

        internal IBaseAntenna GetCurrentBaseAntenna(bool ignoreVisible = false)
        {
            return ignoreVisible ? BaseAntennas.FirstOrDefault(antenna => antenna != null && antenna.Manager.InstanceID == InstanceID) : 
                BaseAntennas.FirstOrDefault(antenna => antenna != null && antenna.Manager.InstanceID == InstanceID && antenna.IsVisible());
        }
        
        /// <summary>
        /// Checks to see if this Item can be stored in this base
        /// </summary>
        /// <param name="amount">amount to store</param>
        /// <param name="techType">The techtype to store</param>
        /// <returns></returns>
        public bool CanBeStored(int amount, TechType techType)
        {

            QuickLogger.Debug($"In Can Be Stored",true);
            return FindValidRack(techType,amount,out var slotId) != null; 

        }

        private DSSRackController FindValidRack(TechType itemTechType,int amount, out int slot)
        {
            slot = -1;
            QuickLogger.Debug($"In FindValidRack",true);
            //Check the filtered racks first
            foreach (DSSRackController baseUnit in BaseRacks)
            {
                var d = DumpContainer.GetTechTypeCount(itemTechType);
                QuickLogger.Debug(d.ToString(),true);
                if (baseUnit.CanHoldItem(amount,itemTechType,out var slotId,d,true))
                {
                    QuickLogger.Debug($"Item: {itemTechType} is allowed in server rack {baseUnit.GetPrefabIDString()} in slot {slotId} is Filtered: {baseUnit.HasFilters()}", true);
                    slot = slotId;
                    return baseUnit;
                }
            }

            //Check the filtered racks first then the unfiltered
            foreach (DSSRackController baseUnit in BaseRacks)
            {
                if (baseUnit.CanHoldItem(amount,itemTechType,out var slotId))
                {
                    QuickLogger.Debug($"Item: {itemTechType} is allowed in server rack {baseUnit.GetPrefabIDString()} in slot {slotId} is Filtered: {baseUnit.HasFilters()}", true);
                    slot = slotId;
                    return baseUnit;
                }
            }

            QuickLogger.Debug($"No qualified server rack is found to hold techtype: {itemTechType}", true);
            return null;
        }

        private void TrackNewFCSConnectable(FCSConnectableDevice obj)
        {
            SubRoot newSeaBase = obj.GetComponentInParent<SubRoot>(); //obj?.gameObject?.transform?.parent?.gameObject;
            var fcsConnectableBase = FindManager(newSeaBase?.GetComponentInChildren<PrefabIdentifier>().Id);
            QuickLogger.Debug($"FCSConnectable Base Found: {newSeaBase?.name}",true);
            QuickLogger.Debug($"FCSConnectable found in base: {Habitat?.name}",true);

            if (fcsConnectableBase?.Habitat == Habitat)
            {
                QuickLogger.Debug("Subscribing to OnContainerUpdate");
                obj.GetStorage().OnContainerUpdate += OnFCSConnectableContainerUpdate;
                QuickLogger.Debug("Adding FCSConnectable");
                FCSConnectables.Add(obj.GetPrefabIDString(), obj);
                QuickLogger.Debug("Added FCSConnectable");
            }
        }

        private void OnFCSConnectableContainerUpdate(int arg1, int arg2)
        {
            Mod.OnBaseUpdate?.Invoke();
        }

        private void GetFCSConnectables()
        {
            //Clear the list
            FCSConnectables.Clear();

            //Check if there is a base connected
            if (Habitat != null)
            {
                var baseConnectable = Habitat.gameObject.AddComponent<FCSConnectableDevice>();
                baseConnectable.InitializeBase(null, this, null, Habitat.gameObject.gameObject?.GetComponentInChildren<PrefabIdentifier>());
                FCSTechFabricator.FcTechFabricatorService.PublicAPI.RegisterDevice(baseConnectable, InstanceID, Mod.DSSTabID);

                //var connectableDevices = Habitat.GetComponentsInChildren<FCSConnectableDevice>().ToList();

                //foreach (var device in connectableDevices)
                //{
                //    FCSConnectables.Add(device.GetPrefabIDString(), device);
                //}
            }
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            var food = item.item.GetComponentInChildren<Eatable>();

            if (food != null)
            {
                bool successful = false;
                if ((!food.decomposes || item.item.GetTechType() == TechType.CreepvinePiece) && CanBeStored(DumpContainer.GetCount() + 1, item.item.GetTechType()))
                {
                    var rackController = FindValidRack(item.item.GetTechType(), 1,out var slot);
                    if (rackController == null) return false;
                    rackController.AddItemToAServer(item, slot);
                    successful = true;
                }
                else
                {
                    foreach (KeyValuePair<string, FCSConnectableDevice> fcsConnectable in FCSConnectables)
                    {
                        if (fcsConnectable.Value.CanBeStored(1, item.item.GetTechType()) && fcsConnectable.Value.GetTechType() == Mod.GetSeaBreezeTechType())
                        {
                            var result = fcsConnectable.Value.AddItemToContainer(new InventoryItem(item.item), out string reason);
                            successful = true;

                            if (!result)
                            {
                                QuickLogger.Error(reason);
                            }
                            break;
                        }
                    }
                }
                
                if (!successful)
                {
                    QuickLogger.Message(string.Format(AuxPatchers.NoEmptySeaBreezeFormat(), item.item.GetTechType()), true);
                    return false;
                }
            }
            else
            {
                var rackController = FindValidRack(item.item.GetTechType(),1, out var slot);
                if (rackController == null) return false;
                rackController.AddItemToAServer(item,slot);
            }

            return true;
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            var successful = false;
            var food = pickupable.GetComponentInChildren<Eatable>();

            if (food != null)
            {

                QuickLogger.Debug($"Food Check {CanBeStored(DumpContainer.GetCount() + 1, pickupable.GetTechType())}",true);

                if ((!food.decomposes || food.GetComponent<Pickupable>().GetTechType() == TechType.CreepvinePiece) && CanBeStored(DumpContainer.GetCount() + 1, pickupable.GetTechType()))
                {
                    successful =  true;
                }
                else
                {
                    foreach (KeyValuePair<string, FCSConnectableDevice> seaBreeze in FCSConnectables)
                    {
                        if (seaBreeze.Value.GetTechType() != Mod.GetSeaBreezeTechType()) continue;
                        if (!seaBreeze.Value.CanBeStored(1, pickupable.GetTechType())) continue;
                        successful =  true;
                    }
                }

                if (!successful)
                {
                    QuickLogger.Message(AuxPatchers.NoFoodItems(), true);
                }


                QuickLogger.Debug($"Food Allowed Result: {successful}",true);
                return successful;
            }

            QuickLogger.Debug($"{DumpContainer.GetCount() + 1}",true);

            if (!CanBeStored(DumpContainer.GetCount() + 1, pickupable.GetTechType()))
            {
                QuickLogger.Message(AuxPatchers.CannotBeStored(), true);
                return false;
            }

            return true;
        }

        public bool IsAllowedToRemoveItems()
        {
            return true;
        }

        public Pickupable RemoveItemFromContainer(TechType techType, int amount = 1)
        {
            if (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift) && QPatch.Configuration.Config.ExtractMultiplier != 0)
            {
                for (int i = 0; i < QPatch.Configuration.Config.ExtractMultiplier * 5; i++)
                {
                    return PerformTakeOperation(techType);
                }
            }
            else
            {
                return PerformTakeOperation(techType);
            }

            //No need to return any items
            return null;
        }
        
        internal void ChangeBaseName()
        {
            NameController.Show();
        }

        internal string GetBaseName()
        {
            return _baseName;
        }

        internal void SetBaseName(string baseName)
        { 
            _baseName = baseName;
        }

        internal string GetDefaultName()
        {
            return $"Base {Managers.Count}";
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

        public static IEnumerable<BaseSaveData> GetSaveData()
        {
            foreach (BaseManager manager in Managers)
            {
                yield return new BaseSaveData {BaseName = manager.GetBaseName(), InstanceID = manager.InstanceID, AllowDocking = manager.DockingManager.GetToggleState(), HasBreakerTripped = manager.GetHasBreakerTripped() };
            }
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

        internal RackSlot GetServerWithItem(TechType techType)
        {
            for (int i = 0; i < BaseRacks.Count; i++)
            {
                var serverWithItem = BaseRacks.ElementAt(i).GetServerWithItem(techType);
                if(serverWithItem == null) continue;
                return serverWithItem;
            }

            return null;
        }
        
        internal int GetItemCount(TechType techType)
        {
            int amount = 0;
            
            if (TrackedItems.ContainsKey(techType))
            {
                amount = TrackedItems[techType];
            }

            foreach (KeyValuePair<string, FCSConnectableDevice> connectable in FCSConnectables)
            {
                if(!connectable.Value.IsOperational() || !connectable.Value.IsVisible) continue;
                amount += connectable.Value.GetItemCount(techType);
            }

            
            return amount;
        }

        #region Operator Code

        internal void AddOperator(DSSOperatorController unit)
        {
            if (!BaseOperators.Contains(unit) && unit.IsConstructed)
            {
                BaseOperators.Add(unit);
                QuickLogger.Debug($"Add Unit : {unit.GetPrefabID()}", true);
            }
        }

        internal void RemoveOperator(DSSOperatorController unit)
        {
            BaseOperators.Remove(unit);
        }
        
        public static List<FCSOperation> Operations { get; set; } = new List<FCSOperation>();
        
        internal static void PerformOperations()
        {
            foreach (FCSOperation operation in Operations)
            {
                if (operation.FromDevice != null && (operation.FromDevice.IsOperational() || (operation.FromDevice.IsBase() && operation.FromDevice.InitializeBase()) && operation.ToDevice != null && operation.ToDevice.IsOperational() && operation.TechType != TechType.None)
                {
                    if (operation.FromDevice.ContainsItem(operation.TechType) && operation.ToDevice.CanBeStored(1, operation.TechType))
                    {
                        if (operation.FromDevice.IsBase())
                        {
                            operation.Manager.GivePlayerItem = false;
                        }
                        var pickupable = operation.FromDevice.RemoveItemFromContainer(operation.TechType, 1);
                        operation.ToDevice.AddItemToContainer(new InventoryItem(pickupable), out string reason);
                    }
                }
            }
        }

        private void RefreshOperators()
        {
            foreach (DSSOperatorController controller in BaseOperators)
            {
                controller.DisplayManager.LoadCommands();
            }
        }

        #endregion

        internal static void AddOperation(FCSOperation operation)
        {
            Operations.Add(operation);
            operation.Manager.RefreshOperators();
        }
    }
}
