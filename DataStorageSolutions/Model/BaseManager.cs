using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Interfaces;
using DataStorageSolutions.Mono;
using FCSCommon.Utilities;
using FCSTechFabricator.Components;
using FCSTechFabricator.Interfaces;
using Oculus.Newtonsoft.Json;
using UnityEngine;

namespace DataStorageSolutions.Model
{
    internal class BaseManager : IFCSStorage
    {
        internal static List<BaseManager> Managers { get; } = new List<BaseManager>();
        internal int InstanceID { get; }
        internal readonly List<DSSRackController> BaseUnits = new List<DSSRackController>();
        internal static readonly List<IBaseAntenna> BaseAntennas = new List<IBaseAntenna>();
        internal readonly SubRoot Habitat;

        private string _baseName;

        public DumpContainer DumpContainer { get; private set; }

        public BaseManager(SubRoot habitat)
        {
            Habitat = habitat;
            InstanceID = habitat.GetInstanceID();
            if (DumpContainer == null)
            {
                DumpContainer = new DumpContainer();
                DumpContainer.Initialize(habitat.transform, AuxPatchers.BaseDumpReceptacle(), AuxPatchers.NotAllowed(), AuxPatchers.DriveFull(), this, 4, 4);
            }
        }

        internal static BaseManager FindManager(SubRoot subRoot)
        {

            //if (!subRoot.isBase) return null; //Disabled to allow Cyclops Operation

            QuickLogger.Debug($"Processing SubRoot = {subRoot.GetInstanceID()} || Name {subRoot.GetSubName()}");

            var pre = subRoot.gameObject.GetComponent<PrefabIdentifier>();

            var manager = Managers.Find(x => x.InstanceID == subRoot.GetInstanceID() && x.Habitat == subRoot);

            if (manager == null)
            {
                QuickLogger.Debug("No manager found on base");
            }

            return manager ?? CreateNewManager(subRoot);
        }

        private static BaseManager CreateNewManager(SubRoot habitat)
        {
            QuickLogger.Debug($"Creating new manager", true);
            var manager = new BaseManager(habitat);
            Managers.Add(manager);
            QuickLogger.Debug($"Manager Count = {Managers.Count}", true);
            return manager;
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

        internal void AddAntenna(DSSAntennaController unit)
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

        private static void CollectServerItems(KeyValuePair<TechType,int> storedItem, Dictionary<TechType, int> data)
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
            return BaseAntennas.FirstOrDefault(antenna => antenna?.Manager == this);
        }
        public int GetContainerFreeSpace { get; }
        public bool IsFull { get; }
        public bool CanBeStored(int amount)
        {
            //TODO Allow Filtering
            return BaseUnits.Any(x => x.IsFull != true);
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            foreach (DSSRackController rackController in BaseUnits)
            {
                if (!rackController.IsFull)
                {
                    rackController.AddItemToAServer(item);
                    return true;
                }
            }

            return false;
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            var food = pickupable.GetComponentInChildren<Eatable>();
            if (food != null)
            {
                if (!Mathf.Approximately(food.foodValue, 0))
                {
                    return false;
                }
            }

            return CanBeStored(1);
        }

        internal string GetBaseName()
        {
            return _baseName;
        }

        internal void SetBaseName(string baseName)
        { 
            _baseName = baseName;
        }
    }
}
