using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataStorageSolutions.Abstract;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Interfaces;
using DataStorageSolutions.Model;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Interfaces;
using FCSCommon.Utilities;
using FCSTechFabricator.Interfaces;
using FCSTechFabricator.Objects;
using ProtoBuf;
using UnityEngine;
using UnityEngine.SceneManagement;
using UWE;

namespace DataStorageSolutions.Mono
{
    [ProtoContract]
    [SkipProtoContractCheck]
    internal class DSSServerController : MonoBehaviour, IProtoEventListener, IFCSStorage, IBaseUnit
    {
        #region Private Fields
        
        private ISlot _slot = null;
        private TechType _techType = TechType.None;
        private bool _runStartUpOnEnable = true;
        #endregion

        #region Public Properties

        public BaseManager Manager { get; set; }
        internal int StorageLimit => QPatch.Configuration.Config.ServerStorageLimit;
        public int GetContainerFreeSpace => GetFreeSpace();
        public bool IsFull => CheckIfFull();
        public TechType TechType => GetTechType();
        internal bool IsMounted => _slot != null;
        public DSSServerDisplay DisplayManager { get; private set; }
        Action<int, int> IFCSStorage.OnContainerUpdate { get; set; }

        private FCSFilteredStorage _fcsFilteredStorage;
        private byte[] _storageRootBytes;
        private string _prefabId;
        private GameObject _storageRootGameObject;
        private int _slotID = -1;

        #endregion
        
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

        private void OnEnable()
        {
          
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
            //foreach (UniqueIdentifier uniqueIdentifier in _storageRoot.transform)
            //{
            //    var pickupable = uniqueIdentifier.gameObject.EnsureComponent<Pickupable>();
            //    if (pickupable?.GetTechType() != Mod.ServerClassID.ToTechType())
            //    {
            //        QuickLogger.Debug($"Found item {pickupable?.GetTechType().ToString()}");
            //        _fcsFilteredStorage.TrackItem(pickupable);
            //    }
            //}
            _fcsFilteredStorage.ForceUpdateDisplay();
            CleanUpDuplicatedStorageNoneRoutine();
        }

        private int GetFreeSpace()
        {
            return QPatch.Configuration.Config.ServerStorageLimit - GetTotal();
        }

        public void Initialize()
        {
            QuickLogger.Debug($"Initializing Server: {GetPrefabID()}");
            GetStorageRoot()?.EnsureComponent<StoreInformationIdentifier>();
            if (_fcsFilteredStorage == null && GetStorageRoot() != null)
            {
                _fcsFilteredStorage = new FCSFilteredStorage();
                _fcsFilteredStorage.Initialize(GetStorageRoot(), UpdateScreen, QPatch.Configuration.Config.ServerStorageLimit);
            }

            if (DisplayManager == null)
            {
                DisplayManager = gameObject.AddComponent<DSSServerDisplay>();
                DisplayManager.Setup(this);
            }

            BaseManager.RegisterServer(this);
            
            IsInitialized = true;
        }

        private GameObject GetStorageRoot()
        {
            if (_storageRootGameObject == null)
            {
                _storageRootGameObject = GameObjectHelpers.FindGameObject(gameObject, "StorageRoot");
            }

            return _storageRootGameObject;

        }

        public bool IsInitialized { get; set; }

        internal int GetTotal()
        {
           return _fcsFilteredStorage?.GetItemsWithin().Sum(x => x.Value) ?? 0;
        }

        public string GetPrefabID()
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

        public void UpdateScreen()
        {
            DisplayManager?.UpdateDisplay();
            if (IsMounted)
            {
                ((RackSlot)_slot).UpdateRackScreen();
            }
        }
        
        public bool ContainsItem(TechType techType)
        {
            return _fcsFilteredStorage.ContainsItem(techType);
        }

        internal IEnumerable<string> GetItemsPrefabID()
        {
            return _fcsFilteredStorage.GetItemsPrefabID();
        }

        internal void ConnectToDevice(BaseManager manager,RackSlot slot)
        {
            Manager = manager;
            _slot = slot;
            _slotID = slot?.Id ?? -1;
            GetStoredItems();
        }

        internal void DisconnectFromDevice()
        {
            Manager = null;
            _slot = null;
            _slotID = -1;
        }

        internal int GetSlotID()
        {
            return _slotID;
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

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Info($"In ProtoDeserialize: {GetPrefabID()}");
            ResetInventory();
            _slotID = Mod.GetServerSaveData(_prefabId).SlotID;
            StorageHelper.RestoreItems(serializer, Mod.GetServerSaveData(_prefabId).Bytes, _fcsFilteredStorage.GetContainer());
        }

        private IEnumerator CleanUpDuplicatedStorage()
        {
            QuickLogger.Debug("Cleaning Duplicates",true);
            Transform hostTransform = transform;
            StoreInformationIdentifier[] sids = gameObject.GetComponentsInChildren<StoreInformationIdentifier>(true);
            int num;
            for (int i = sids.Length - 1; i >= 0; i = num - 1)
            {
                StoreInformationIdentifier storeInformationIdentifier = sids[i];
                if (storeInformationIdentifier != null && storeInformationIdentifier.name.StartsWith("SerializerEmptyGameObject", StringComparison.OrdinalIgnoreCase))
                {
                    Destroy(storeInformationIdentifier.gameObject);
                    yield return null;
                }
                num = i;
            }
            yield break;
        }

        private void CleanUpDuplicatedStorageNoneRoutine()
        {
            QuickLogger.Debug("Cleaning Duplicates", true);
            Transform hostTransform = transform;
            StoreInformationIdentifier[] sids = gameObject.GetComponentsInChildren<StoreInformationIdentifier>(true);
#if DEBUG
            QuickLogger.Debug($"SIDS: {sids.Length}", true);
#endif

            int num;
            for (int i = sids.Length - 1; i >= 0; i = num - 1)
            {
                StoreInformationIdentifier storeInformationIdentifier = sids[i];
                if (storeInformationIdentifier != null && storeInformationIdentifier.name.StartsWith("SerializerEmptyGameObject", StringComparison.OrdinalIgnoreCase))
                {
                    Destroy(storeInformationIdentifier.gameObject);
                    QuickLogger.Debug($"Destroyed Duplicate", true);
                }
                num = i;
            }
        }

        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.ModMessage($"In ProtoSerialize: {GetPrefabID()}");
            //CoroutineHost.StartCoroutine(this.CleanUpDuplicatedStorage());
        }

        public byte[] GetStorageBytes(ProtobufSerializer serializer)
        {
            QuickLogger.ModMessage($"Getting Storage Bytes");
            return _storageRootBytes = StorageHelper.Save(serializer, GetStorageRoot());
        }
        
        private void ResetInventory()
        {
            _fcsFilteredStorage.Clear();
            StorageHelper.RenewIdentifier(GetStorageRoot());
        }

        public int GetItemCount(TechType item)
        {
            return _fcsFilteredStorage?.GetItemCount(item) ?? 0;
        }

        public ItemsContainer GetItemsContainer()
        {
           return _fcsFilteredStorage.GetContainer();
        }

        public string GetFormatData()
        {
            return _fcsFilteredStorage.FormatFiltersData();
        }

        public ISlot GetSlot()
        {
            return _slot;
        }
    }
}