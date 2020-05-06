using System;
using System.Collections.Generic;
using System.Linq;
using DataStorageSolutions.Abstract;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Model;
using FCSCommon.Abstract;
using FCSCommon.Enums;
using FCSCommon.Extensions;
using FCSCommon.Interfaces;
using FCSCommon.Utilities;
using FCSTechFabricator.Interfaces;
using Oculus.Newtonsoft.Json;
using PriorityQueueInternal;

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
        private List<ObjectData> _items = new List<ObjectData>();
        private List<Filter> _filters;
        private ServerData _data;

        [JsonIgnore] public override BaseManager Manager { get; set; }
        [JsonIgnore] internal int StorageLimit => QPatch.Configuration.Config.ServerStorageLimit;
        [JsonIgnore] public int GetContainerFreeSpace => GetFreeSpace();
        [JsonIgnore] public bool IsFull => CheckIfFull();
        [JsonProperty] internal List<ObjectData> Items
        {
            get => _items;
            set
            {
                QuickLogger.Debug($"Server Value: {value.Count}"); 
                _items = value;

                UpdateServerData(GetPrefabIDString());
            }
        }

        [JsonIgnore] public TechType TechType => GetTechType();
        [JsonIgnore] internal bool IsMounted => _mono != null;
        [JsonIgnore] public DSSServerDisplay DisplayManager { get; private set; }
        [JsonProperty] internal List<Filter> Filters
        {
            get => _filters ?? (_filters = _data?.ServerFilters);
            set
            {
                QuickLogger.Debug($"Filter Value: {value.Count}");

                _filters = value;

                UpdateServerData(GetPrefabIDString());
            }
        }

        //private void OnEnable()
        //{
        //    if (_runStartUpOnEnable)
        //    {
        //        if (!IsInitialized)
        //        {
        //            Initialize();
        //        }

        //        if (_fromSave)
        //        {
        //            if (_savedData == null)
        //            {
        //                ReadySaveData();
        //            }

        //            if (_savedData != null)
        //            {
        //                Items = _savedData.ServerData.Items;
        //            }
        //        }
        //        _runStartUpOnEnable = false;
        //        IsInitialized = true;
        //    }
        //}

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetSaveData(GetPrefabIDString());
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
                    Initialize();

                    if (_fromSave)
                    {
                        if (_savedData == null)
                        {
                            ReadySaveData();
                        }
                    }
                    _runStartUpOnEnable = false;

                }
                IsInitialized = true;
            }
        }

        private void LoadSaveData()
        {
            var id = GetPrefabIDString();

            if (Mod.Servers.ContainsKey(id))
            {
                _data = Mod.Servers[id];
                Items = _data.Server;
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
            return StorageLimit - Items.Count;
        }

        internal void DemoAddItem()
        {
            for (int i = 0; i < 5; i++)
            {
                AddItemToContainer(TechType.Glass.ToInventoryItem());
            }
        }

        internal void ConnectToRack(DSSRackController mono, int slot)
        {
            _mono = mono;
            _slot = slot;
        }

        internal void DisconnectFromRack()
        {
            _mono = null;
            _slot = 0;
        }

        internal int GetTotal()
        {
            return Items.Count;
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
                Mod.Servers[id].Server = Items;
                Mod.Servers[id].ServerFilters = Filters;
            }
            else
            {
                Mod.Servers.Add(id, new ServerData{ServerFilters = Filters, Server = Items});
            }
        }

        private bool FindObjectDataMatch(TechType item, out ObjectData objectData)
        {
            bool objectDataMatch = false;
            objectData = null;
            QuickLogger.Debug($"Trying to add item to container {item}", true);
            foreach (ObjectData data in Items)
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
                Items.Add(new ObjectData { TechType = item});
                DisplayManager.UpdateDisplay();
                Mod.OnContainerUpdate?.Invoke();
            }
        }

        internal void RemoveItemFromContainer(TechType item)
        {
            QuickLogger.Debug("Taking From Container", true);

            var objectDataMatch = FindObjectDataMatch(item, out var objectData);

            if (objectDataMatch)
            {
#if SUBNAUTICA
                var itemSize = CraftData.GetItemSize(item);
#elif BELOWZERO
            var itemSize = TechData.GetItemSize(item);
#endif
                if (Inventory.main.HasRoomFor(itemSize.x, itemSize.y))
                {

                    var pickup = CraftData.InstantiateFromPrefab(item).GetComponent<Pickupable>();
                    Inventory.main.Pickup(pickup);
                    DeleteItemFromContainer(item);
                    Mod.OnContainerUpdate?.Invoke();
                }
            }
        }

        internal void DeleteItemFromContainer(TechType item)
        {
            var objectDataMatch = FindObjectDataMatch(item, out var objectData);
            if (!objectDataMatch) return;
            Items.Remove(objectData);
            DisplayManager.UpdateDisplay();
            UpdateServerData(GetPrefabIDString());
            Mod.OnContainerUpdate?.Invoke();
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return true;
        }
        
        internal void Load(Dictionary<TechType, int> savedDataContainer)
        {
            if (savedDataContainer == null) return;
            foreach (KeyValuePair<TechType, int> pair in savedDataContainer)
            {
                if (pair.Value == 0)
                {
                    AddItemToContainer(pair.Key, true);
                    continue;
                }

                for (int i = 0; i < pair.Value; i++)
                {
                    AddItemToContainer(pair.Key);
                }
            }
        }

        public bool HasItems()
        {
            return Items.Count > 0;
        }

        internal int GetSlot()
        {
            return _slot;
        }

        public override void Initialize()
        {
            if(DisplayManager == null)
            {
                DisplayManager = gameObject.AddComponent<DSSServerDisplay>();
                DisplayManager.Setup(this);
            }
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
            _savedData.ServerData = Items;
            _savedData.Filters = Filters;
            newSaveData.Entries.Add(_savedData);
        }
        internal int GetSeverCount()
        {
            return Mod.Servers?.Count() ?? 0;
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

            LoadSaveData();

            var id = GetPrefabIDString();
            QuickLogger.Info($"Loading Server {id}");
            _fromSave = true;

        }

        public bool HasItem(TechType techType)
        {
            return FindObjectDataMatch(techType, out var objectData);
        }
    }
}
