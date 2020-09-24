using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using DataStorageSolutions.Abstract;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Model;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Interfaces;
using FCSCommon.Utilities;
using FCSTechFabricator.Interfaces;
using FCSTechFabricator.Objects;
using Oculus.Newtonsoft.Json;
using UnityEngine;

namespace DataStorageSolutions.Mono
{
    internal class DSSServerController : DataStorageSolutionsController, IFCSStorage, IBaseUnit
    {
        #region Private Fields

        private int _slot = -1;
        private TechType _techType = TechType.None;
        private bool _runStartUpOnEnable = true;
        private GameObject _storageRoot;

        #endregion

        #region Public Properties

        public override BaseManager Manager { get; set; }
        internal int StorageLimit => QPatch.Configuration.Config.ServerStorageLimit;
        public int GetContainerFreeSpace => GetFreeSpace();
        public bool IsFull => CheckIfFull();
        public TechType TechType => GetTechType();
        internal bool IsMounted => _slot > -1;
        public DSSServerDisplay DisplayManager { get; private set; }
        Action<int, int> IFCSStorage.OnContainerUpdate { get; set; }

        private FCSFilteredStorage _fcsFilteredStorage;

        #endregion

        private void AddDummy()
        {
            _fcsFilteredStorage.AddItemToContainer(TechType.PowerCell.ToInventoryItem());
        }
        
        #region Unity Methods

        private void OnDestroy()
        {
            BaseManager.UnRegisterServer(this);
        }

        private void Awake()
        {
            if (!_runStartUpOnEnable || IsInitialized) return;
            
            Initialize();

            GetStoredItems();
            
            _runStartUpOnEnable = false;
        }

        #endregion
        
        private TechType GetTechType()
        {
            if (_techType != TechType.None) return _techType;

            var techTag = gameObject.GetComponentInChildren<TechTag>();
            _techType = techTag != null ? techTag.type : TechType.None;

            return _techType;
        }
        
        private bool CheckIfFull()
        {
            return GetTotal() >= StorageLimit;
        }

        private void GetStoredItems()
        {
            foreach (UniqueIdentifier uniqueIdentifier in _storageRoot.GetComponentsInChildren<UniqueIdentifier>(true))
            {
                var pickupable = uniqueIdentifier.gameObject.EnsureComponent<Pickupable>();
                if (pickupable?.GetTechType() != Mod.ServerClassID.ToTechType())
                {
                    QuickLogger.Debug($"Found item {pickupable?.GetTechType().ToString()}");
                    _fcsFilteredStorage.TrackItem(pickupable);
                }
            }
            _fcsFilteredStorage.ForceUpdateDisplay();
        }

        private int GetFreeSpace()
        {
            return QPatch.Configuration.Config.ServerStorageLimit - GetTotal();
        }

        public override void Initialize()
        {
            QuickLogger.Debug($"Initializing Server: {GetPrefabID()}");

            _storageRoot = gameObject;

            if (_fcsFilteredStorage == null && _storageRoot != null)
            {
                _fcsFilteredStorage = new FCSFilteredStorage();
                _fcsFilteredStorage.Initialize(_storageRoot, UpdateScreen, QPatch.Configuration.Config.ServerStorageLimit);
            }

            if (DisplayManager == null)
            {
                DisplayManager = gameObject.AddComponent<DSSServerDisplay>();
                DisplayManager.Setup(this);
            }

            _slot = BaseManager.FindSlotId(GetPrefabID());

            BaseManager.RegisterServer(this);

            IsInitialized = true;
        }

        internal int GetTotal()
        {
           return _fcsFilteredStorage?.GetItemsWithin().Sum(x => x.Value) ?? 0;
        }

        public override string GetPrefabID()
        {
            if (string.IsNullOrEmpty(_prefabId))
            {
                var id = GetComponentInChildren<PrefabIdentifier>() ?? GetComponentInParent<PrefabIdentifier>();
                _prefabId = id != null ? id.Id : string.Empty;
            }

            QuickLogger.Debug($"Tried to get  PrefabId string and got: {_prefabId}");

            return _prefabId;
        }

        public bool CanBeStored(int amount,TechType techType = TechType.None)
        {
            return _fcsFilteredStorage.CanBeStored(amount,techType);
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            return _fcsFilteredStorage.AddItemToContainer(item);
        }
        
        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return true;
        }

        public bool IsAllowedToRemoveItems()
        {
            return true;
        }

        public Pickupable RemoveItemFromContainer(TechType techType, int amount)
        {
            return _fcsFilteredStorage.RemoveItemFromContainer(techType, amount);
        }

        public Dictionary<TechType, int> GetItemsWithin()
        {
            return _fcsFilteredStorage.GetItemsWithin();
        }

        public override void UpdateScreen()
        {
            DisplayManager?.UpdateDisplay();
        }
        
        public bool ContainsItem(TechType techType)
        {
            return _fcsFilteredStorage.ContainsItem(techType);
        }

        public override void Save(SaveData save)
        {

        }

        internal void ConnectToDevice(BaseManager manager,int slotID)
        {
            Manager = manager;
            _slot = slotID;
            GetStoredItems();
        }

        internal void DisconnectFromDevice()
        {
            Manager = null;
            _slot = -1;
        }

        internal int GetSlotID()
        {
            return _slot;
        }

        public bool HasFilters()
        {
            return _fcsFilteredStorage.HasFilters();
        }

        public bool HasItem(TechType techType)
        {
            return _fcsFilteredStorage.HasItem(techType);
        }

        public HashSet<Filter> GetFilters()
        {
            return _fcsFilteredStorage.Filters;
        }

        public void SetFilters(HashSet<Filter> dataServerFilters)
        {
            _fcsFilteredStorage.Filters = dataServerFilters;
        }
    }
}