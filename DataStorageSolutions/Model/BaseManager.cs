using System;
using System.Collections.Generic;
using System.Linq;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Interfaces;
using DataStorageSolutions.Mono;
using DataStorageSolutions.Structs;
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

        internal static List<BaseManager> Managers { get; } = new List<BaseManager>();
        internal static readonly List<IBaseAntenna> BaseAntennas = new List<IBaseAntenna>();
        internal string InstanceID { get; }
        internal readonly HashSet<DSSRackController> BaseUnits = new HashSet<DSSRackController>();
        internal readonly SubRoot Habitat;
        internal DumpContainer DumpContainer { get; private set; }
        public int GetContainerFreeSpace { get; }
        public bool IsFull { get; }
        public NameController NameController { get; private set; }

        
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
            QuickLogger.Debug("In OnProtoDeserialize");
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
                QuickLogger.Debug("RackController is null", true);
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

            if (manager == null)
            {
                QuickLogger.Debug("No manager found on base");
            }

            return manager ?? CreateNewManager(subRoot);
        }

        internal static BaseManager FindManager(string instanceID)
        {
            QuickLogger.Debug($"Trying to find a Base with the ID {instanceID}",true);

            var manager = Managers.Find(x => x.InstanceID == instanceID);

            if (manager == null)
            {
                QuickLogger.Debug($"No manager was found with the instanceID {instanceID}",true);
            }

            return manager;
        }
        
        internal bool HasAntenna()
        {
            if (Habitat.isCyclops)
            {
                return true;
            }

            return GetCurrentBaseAntenna() != null;
        }

        internal void AddUnit(DSSRackController unit)
        {
            if (!BaseUnits.Contains(unit) && unit.IsConstructed)
            {
                BaseUnits.Add(unit);
                QuickLogger.Debug($"Add Unit : {unit.GetPrefabIDString()}", true);
            }
        }
        
        internal static void RemoveUnit(DSSRackController unit)
        {
            foreach (BaseManager manager in Managers)
            {
                if (!manager.BaseUnits.Contains(unit)) continue;
                manager.BaseUnits.Remove(unit);
                QuickLogger.Debug($"Removed Unit : {unit.GetPrefabIDString()}", true);
            }
        }

        internal static void AddAntenna(DSSAntennaController unit)
        {
            if (!BaseAntennas.Contains(unit) && unit.IsConstructed)
            {
                BaseAntennas.Add(unit);
                QuickLogger.Debug($"Add Unit : {unit.GetPrefabIDString()}", true);
            }
        }

        internal static void RemoveAntenna(DSSAntennaController unit)
        {
            if (BaseAntennas.Contains(unit))
            {
                BaseAntennas.Remove(unit);
            }
        }

        internal Dictionary<TechType,int> GetItemsWithin()
        {
            try
            {
                Dictionary<TechType, int> data = new Dictionary<TechType, int>();

                foreach (DSSRackController rackController in BaseUnits)
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
            QuickLogger.Debug("In TakeItem",true);

            foreach (DSSRackController baseUnit in BaseUnits)
            {
                QuickLogger.Debug($"Base: {baseUnit?.Manager?.Habitat?.GetSubName()} || TechType: {techType}");

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

        public IBaseAntenna GetCurrentBaseAntenna()
        {
            QuickLogger.Debug($"Current ID: {InstanceID}",true);
            return BaseAntennas.FirstOrDefault(antenna => antenna?.Manager.InstanceID == InstanceID);
        }
        
        public bool CanBeStored(int amount, TechType techType)
        {
            return FindValidRack(techType) != null;
        }

        private DSSRackController FindValidRack(TechType itemTechType)
        {
            //Check the filtered racks first
            foreach (DSSRackController baseUnit in BaseUnits)
            {
                if (!baseUnit.IsFull && baseUnit.IsTechTypeAllowedInRack(itemTechType) && baseUnit.HasFilters())
                {
                    QuickLogger.Debug($"Item: {itemTechType} is allowed in server rack {baseUnit.GetPrefabIDString()} is Filtered: {baseUnit.HasFilters()}", true);
                    return baseUnit;
                }
            }

            //Check the filtered racks first then the unfiltered
            foreach (DSSRackController baseUnit in BaseUnits)
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
                if (!Mathf.Approximately(food.foodValue, 0))
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
    }
}
