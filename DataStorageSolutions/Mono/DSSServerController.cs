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
        public FCSFilteredStorage FCSFilteredStorage { get; private set; }

        #endregion

        private void AddDummy()
        {
            FCSFilteredStorage.AddItemToContainer(TechType.PowerCell.ToInventoryItem());
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
                    FCSFilteredStorage.TrackItem(pickupable);
                }
            }
            FCSFilteredStorage.ForceUpdateDisplay();
        }

        private int GetFreeSpace()
        {
            return QPatch.Configuration.Config.ServerStorageLimit - GetTotal();
        }

        public override void Initialize()
        {
            QuickLogger.Debug($"Initializing Server: {GetPrefabID()}");

            _storageRoot = gameObject;

            if (FCSFilteredStorage == null && _storageRoot != null)
            {
                FCSFilteredStorage = new FCSFilteredStorage();
                FCSFilteredStorage.Initialize(_storageRoot, UpdateScreen, QPatch.Configuration.Config.ServerStorageLimit);
            }

            if (DisplayManager == null)
            {
                DisplayManager = gameObject.AddComponent<DSSServerDisplay>();
                DisplayManager.Setup(this);
            }

            BaseManager.RegisterServer(this);

            IsInitialized = true;
        }

        internal int GetTotal()
        {
           return FCSFilteredStorage?.GetItemsWithin().Sum(x => x.Value) ?? 0;
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
            return true;
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            return FCSFilteredStorage.AddItemToContainer(item);
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
            return FCSFilteredStorage.RemoveItemFromContainer(techType, amount);
        }

        public Dictionary<TechType, int> GetItemsWithin()
        {
            return FCSFilteredStorage.GetItemsWithin();
        }

        public override void UpdateScreen()
        {
            DisplayManager?.UpdateDisplay();
        }
        
        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {

        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {

        }
        
        public bool ContainsItem(TechType techType)
        {
            return FCSFilteredStorage.ContainsItem(techType);
        }

        public override void Save(SaveData save)
        {

        }

        internal void SetSlot(int slotID)
        {
            _slot = slotID;
        }

        internal void Disconnect()
        {
            _slot = -1;
        }
        
        internal int GetSlotID()
        {
            return _slot;
        }
    }
}