using System;
using System.Collections.Generic;
using System.Linq;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Interfaces;
using DataStorageSolutions.Mono;
using DataStorageSolutions.Structs;
using FCSCommon.Enums;
using FCSCommon.Utilities;
using FCSTechFabricator.Components;
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
        internal readonly HashSet<DSSTerminalController> BaseTerminals = new HashSet<DSSTerminalController>();
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
        public NameController NameController { get; private set; }
        internal Action<bool> OnBreakerToggled { get; set; }
        
        private void Initialize(SubRoot habitat)
        {
            ReadySaveData();

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
                DumpContainer = new DumpContainer();
                DumpContainer.Initialize(habitat.transform, AuxPatchers.BaseDumpReceptacle(), AuxPatchers.NotAllowed(),
                    AuxPatchers.CannotBeStored(), this, 4, 4);
            }
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
        
        private static void GetStoredData(DSSRackController rackController, Dictionary<TechType, int> data)
        {
            if (rackController == null)
            {
                return;
            }

            foreach (KeyValuePair<TechType, int> storedItems in rackController.GetStoredData())
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

            OnBreakerToggled?.Invoke(_hasBreakerTripped);
        }

        public BaseManager(SubRoot habitat)
        {
            if (habitat == null) return;
            Habitat = habitat;
            InstanceID = habitat.gameObject.gameObject?.GetComponentInChildren<PrefabIdentifier>()?.Id;
            Initialize(habitat);
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

        private int GetAntennaCount()
        {
            int i = 0;

            for (int j = 0; j < BaseAntennas.Count; j++)
            {
                i++;
            }

            return i;
        }

        internal Dictionary<TechType,int> GetItemsWithin()
        {
            try
            {
                Dictionary<TechType, int> data = new Dictionary<TechType, int>();

                foreach (DSSRackController rackController in BaseRacks)
                {
                    if(rackController == null) continue;
                    GetStoredData(rackController, data);
                }

                return data;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.StackTrace);
                return null;
            }
        }

        public void TakeItem(TechType techType)
        {
            foreach (DSSRackController baseUnit in BaseRacks)
            {
                if (baseUnit.HasItem(techType))
                {
                    var data = baseUnit.GetItemDataFromServer(techType);
                    baseUnit.GivePlayerItem(techType,data);
                    break;
                }
            }
        }

        public void OpenDump(TransferData data)
        {
           DumpContainer.OpenStorage();
        }

        public IBaseAntenna GetCurrentBaseAntenna(bool ignoreVisible = false)
        {
            return ignoreVisible ? BaseAntennas.FirstOrDefault(antenna => antenna != null && antenna.Manager.InstanceID == InstanceID) : 
                BaseAntennas.FirstOrDefault(antenna => antenna != null && antenna.Manager.InstanceID == InstanceID && antenna.IsVisible());
        }
        
        public bool CanBeStored(int amount, TechType techType)
        {
            return FindValidRack(techType) != null;
        }

        private DSSRackController FindValidRack(TechType itemTechType)
        {
            //Check the filtered racks first
            foreach (DSSRackController baseUnit in BaseRacks)
            {
                if (!baseUnit.IsFull && baseUnit.IsTechTypeAllowedInRack(itemTechType) && baseUnit.HasFilters())
                {
                    QuickLogger.Debug($"Item: {itemTechType} is allowed in server rack {baseUnit.GetPrefabIDString()} is Filtered: {baseUnit.HasFilters()}", true);
                    return baseUnit;
                }
            }

            //Check the filtered racks first then the unfiltered
            foreach (DSSRackController baseUnit in BaseRacks)
            {
                if (!baseUnit.IsFull && baseUnit.IsTechTypeAllowedInRack(itemTechType))
                {
                    QuickLogger.Debug($"Item: {itemTechType} is allowed in server rack {baseUnit.GetPrefabIDString()} is Filtered: {baseUnit.HasFilters()}", true);
                    return baseUnit;
                }
            }

            QuickLogger.Debug($"No qualified server rack is found to hold techtype: {itemTechType}", true);
            return null;
        }
        
        public bool AddItemToContainer(InventoryItem item)
        {
            var rackController = FindValidRack(item.item.GetTechType());
            if (rackController == null) return false;
            rackController.AddItemToAServer(item);
            return true;
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            var food = pickupable.GetComponentInChildren<Eatable>();

            if (food != null)
            {
                if (food.decomposes && food.kDecayRate > 0)
                {
                    QuickLogger.Message(AuxPatchers.NoFoodItems(),true);
                    return false;
                }
            }
            
            if (!CanBeStored(1,pickupable.GetTechType()))
            {
                QuickLogger.Message(AuxPatchers.CannotBeStored(), true);
                return false;
            }

            return true;
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
                yield return new BaseSaveData {BaseName = manager.GetBaseName(), InstanceID = manager.InstanceID};
            }
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
    }
}
