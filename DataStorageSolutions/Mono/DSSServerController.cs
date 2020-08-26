using System;
using System.Collections.Generic;
using DataStorageSolutions.Abstract;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Model;
using FCSCommon.Interfaces;
using FCSCommon.Utilities;
using FCSTechFabricator.Enums;
using FCSTechFabricator.Interfaces;
using FCSTechFabricator.Objects;
using Oculus.Newtonsoft.Json;

namespace DataStorageSolutions.Mono
{
    internal class DSSServerController : DataStorageSolutionsController, IFCSStorage, IBaseUnit
    {
        private DSSRackController _mono;
        private int _slot;
        private TechType _techType = TechType.None;
        private bool _runStartUpOnEnable = true;
        private bool _fromSave;
        private SaveDataEntry _savedData;
        private HashSet<ObjectData> _items = new HashSet<ObjectData>();
        private List<Filter> _filters;
        private ServerData _data;

        [JsonIgnore] public override BaseManager Manager { get; set; }
        [JsonIgnore] internal int StorageLimit => QPatch.Configuration.Config.ServerStorageLimit;
        [JsonIgnore] public int GetContainerFreeSpace => GetFreeSpace();
        [JsonIgnore] public bool IsFull => CheckIfFull();

        [JsonIgnore] public TechType TechType => GetTechType();
        [JsonIgnore] internal bool IsMounted => _mono != null;
        [JsonIgnore] public DSSServerDisplay DisplayManager { get; private set; }

        [JsonIgnore] Action<int, int> IFCSStorage.OnContainerUpdate { get; set; }
        public FCSFilteredStorage FCSFilteredStorage { get; private set; }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetSaveData(GetPrefabIDString());
            LoadSaveData();
        }
         
        private void OnDestroy()
        {
            var id = GetPrefabIDString();
            if (Mod.TrackedServers != null && Mod.TrackedServers.Contains(id))
            {
                Mod.TrackedServers.Remove(id);
            }
        }

        private void Awake()
        {
            if (_runStartUpOnEnable)
            {
                if (!IsInitialized)
                {
                    if (_fromSave)
                    {
                        if (_savedData == null)
                        {
                            ReadySaveData();
                        }
                    }

                    Initialize();

                    _runStartUpOnEnable = false;

                }
            }
        }

        private void LoadSaveData()
        {
            var id = GetPrefabIDString();

            if (Mod.Servers.ContainsKey(id))
            {
                _data = Mod.Servers[id];
            }
        }

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

        private int GetFreeSpace()
        {
            return StorageLimit - FCSFilteredStorage?.Items?.Count ?? 0;
        }

        internal int GetTotal()
        {
            return FCSFilteredStorage?.Items?.Count ?? 0;
        }

        internal string GetPrefabIDString()
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
            AddItemToContainer(item.item.GetTechType());

            UpdateServerData(GetPrefabIDString());

            Destroy(item.item);
            return false;
        }

        private void UpdateServerData(string id)
        {
            if (!Mod.TrackedServers.Contains(id))
            {
                Mod.TrackedServers.Add(id);
            }

            if (Mod.Servers.ContainsKey(id))
            {
                Mod.Servers[id].Server = FCSFilteredStorage.Items;
                Mod.Servers[id].ServerFilters = FCSFilteredStorage.Filters;
            }
            else
            {
                Mod.Servers.Add(id, new ServerData{ServerFilters = FCSFilteredStorage.Filters, Server = FCSFilteredStorage.Items });
            }
        }

        private bool FindObjectDataMatch(TechType item, out ObjectData objectData)
        {
            bool objectDataMatch = false;
            objectData = null;
            QuickLogger.Debug($"Trying to add item to container {item}", true);
            foreach (ObjectData data in FCSFilteredStorage.Items)
            {
                if (data.TechType == item)
                {
                    objectData = data;
                    objectDataMatch = true;
                    break;
                }
            }

            return objectDataMatch;
        }

        internal void AddItemToContainer(TechType item, bool initializer = false)
        {
            QuickLogger.Debug("Adding to container", true);

            if (!IsFull)
            {
                FCSFilteredStorage.Items.Add(new ObjectData { TechType = item});
                UpdateScreen();
                Mod.OnContainerUpdate?.Invoke(_mono);
            }
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
            throw new NotImplementedException();
        }

        public Dictionary<TechType, int> GetItemsWithin()
        {
            throw new NotImplementedException();
        }
        
        public override void Initialize()
        {
            if (FCSFilteredStorage == null)
            {
                FCSFilteredStorage = gameObject.GetComponent<FCSFilteredStorage>();
                FCSFilteredStorage.Initialize(_data?.ServerFilters,UpdateScreen);
                FCSFilteredStorage.Items = _data?.Server;
                FCSFilteredStorage.OnFiltersUpdate += OnFiltersUpdate;
                FCSFilteredStorage.OnItemsUpdate += OnItemsUpdate;
            }

            if (DisplayManager == null)
            {
                DisplayManager = gameObject.AddComponent<DSSServerDisplay>();
                DisplayManager.Setup(this);
            }

            IsInitialized = true;
        }

        public override void UpdateScreen()
        {
            DisplayManager.UpdateDisplay();
        }

        private void OnItemsUpdate(HashSet<ObjectData> obj)
        {
            UpdateServerData(GetPrefabIDString());
        }

        private void OnFiltersUpdate(List<Filter> obj)
        {
            UpdateServerData(GetPrefabIDString());
        }

        public override void Save(SaveData newSaveData)
        {
            if (!IsInitialized) return;

            var id = GetPrefabIDString();

            if (_savedData == null)
            {
                _savedData = new SaveDataEntry();
            }

            _savedData.ID = id;
            _savedData.ServerData = FCSFilteredStorage.Items;
            _savedData.Filters = FCSFilteredStorage.Filters;
            newSaveData.Entries.Add(_savedData);
        }
     
        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            var id = GetPrefabIDString();
            
            QuickLogger.Debug($"In OnProtoSerialize: Saving Server {id}");
            
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving Server {id}");
                Mod.Save();
                QuickLogger.Info($"Saved Server {id}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoDeserialize");

            if (_savedData == null)
            {
                ReadySaveData();
            }

            var id = GetPrefabIDString();
            QuickLogger.Info($"Loading Server {id}");
            _fromSave = true;

        }

        public bool ContainsItem(TechType techType)
        {
            return FindObjectDataMatch(techType, out var objectData);
        }
    }
}