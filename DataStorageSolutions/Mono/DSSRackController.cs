using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using DataStorageSolutions.Abstract;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Helpers;
using DataStorageSolutions.Model;
using FCSCommon.Utilities;
using FCSCommon.Controllers;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Interfaces;
using FCSCommon.Objects;
using FCSTechFabricator.Components;
using FCSTechFabricator.Extensions;
using FCSTechFabricator.Interfaces;
using FCSTechFabricator.Managers;
using FCSTechFabricator.Objects;
using UnityEngine;

namespace DataStorageSolutions.Mono
{
    internal class DSSRackController : DataStorageSolutionsController, IBaseUnit, IFCSStorage
    {
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private SaveDataEntry _savedData;
        private readonly RackSlot[] _servers = new RackSlot[6];
        private bool _isConstructed;
        private string _prefabID;
        private TechType _techType = TechType.None;
        private int _rackDoor;
        private int _buttonState;
        private GameObject _drives;
        private DSSAudioHandler _audioManager;
        private BaseManager _manager;
        private const int RackDoorStateClosed = 0;
        private const int RackDoorStateOpen = 1;
        private Dictionary<TechType,int> _trackedItems = new Dictionary<TechType, int>();
        public int GetContainerFreeSpace { get; }
        public bool IsFull => GetIsFull();
        public bool IsRackSlotsFull => GetIsRackFull();
        public override bool IsConstructed => _isConstructed;
        internal Action OnUpdate;
        Action<int, int> IFCSStorage.OnContainerUpdate { get; set; }

        public override BaseManager Manager
        {
            get => _manager;
            set
            {
                _manager = value;
                //Because the way the Terminal is lazy loaded. I choose to lazy load the power manager based on the manager setter
                if (value != null)
                {
                    PowerManager?.Initialize(this, QPatch.Configuration.Config.ScreenPowerUsage);
                }

            }
        }

        public TechType TechType => GetTechType();
        internal AnimationManager AnimationManager { get; private set; }
        internal DSSRackDisplayController DisplayManager { get; private set; }
        internal DumpContainer DumpContainer { get; private set; }
        internal ColorManager ColorManager { get; private set; }
        public PowerManager PowerManager { get; private set; }

        #region Unity

        private void OnEnable()
        {
            if (_runStartUpOnEnable)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }

                if (_fromSave)
                {
                    if (_savedData == null)
                    {
                        ReadySaveData();
                    }

                    if (_savedData != null)
                    {
                        QuickLogger.Debug("Loading Rack Data");
                        LoadRack();
                        ColorManager.SetMaskColorFromSave(_savedData.BodyColor.Vector4ToColor());
                    }
                }
                _runStartUpOnEnable = false;
                IsInitialized = true;
            }
        }
        
        private void OnDestroy()
        {
            IsInitialized = false;
            BaseManager.RemoveRack(this);
        }

        private void Update()
        {
            OnUpdate?.Invoke();
            if (IsConstructed && PowerManager != null)
            {
                PowerManager?.UpdatePowerState();
                PowerManager?.ConsumePower();
            }
        }

        #endregion

        private void LoadRack()
        {
            QuickLogger.Debug("Load Rack");
            if (_savedData.Servers == null) return;

            QuickLogger.Debug($"Save Data Count: {_savedData.Servers.Count}");

            foreach (ServerData data in _savedData.Servers)
            {
                if (data != null)
                {
                    var slot = GetSlotByID(GetFreeSlotID());
                    AddServer(data.Server, data.ServerFilters);
                }
            }
        }

        private TechType GetTechType()
        {
            if (_techType != TechType.None) return _techType;

            var techTag = gameObject.GetComponentInChildren<TechTag>();
            _techType = techTag != null ? techTag.type : TechType.None;

            return _techType;
        }

        private bool GetIsRackFull()
        {
            if (_servers == null)
            {
                QuickLogger.Error("Rack Servers Array is null. Please let FCS Studios know about this issue");
                return true;
            }

            var amount = _servers.Count(server => server.IsOccupied);

            return amount >= _servers.Length;
        }

        private bool GetIsFull()
        {
            var server = _servers.Any(x => x.Server != null);
            if (server == false) return true;
            var storage = GetTotalStorage();
            return storage.x >= storage.y;
        }

        private RackSlot GetSlotByID(int slotID)
        {
            return _servers[slotID];
        }

        private bool FindSlots()
        {
            try
            {
                _drives = GameObjectHelpers.FindGameObject(gameObject, "Drives");

                for (int i = 0; i < _drives.transform.childCount; i++)
                {
                    _servers[i] = new RackSlot(this, i, _drives.transform.GetChild(i));
                }
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return false;
            }

            return true;
        }

        private bool AddServerToSlot(HashSet<ObjectData> server,List<Filter> filters, int slotID)
        {
            var slotByID = this.GetSlotByID(slotID);

            if (slotByID == null)
            {
                return false;
            }

            if (slotByID.IsOccupied)
            {
                return false;
            }

            QuickLogger.Debug($"Adding Server to slot {slotByID.Id}", true);
            GetSlotByID(slotID).IsOccupied = true;
            QuickLogger.Debug($"Current ID Occupied Stat = {slotByID.IsOccupied}");
            QuickLogger.Debug($"Data Count = {server.Count}");

            slotByID.Server = new HashSet<ObjectData>(server);
            if (filters != null)
            {
                slotByID.Filter = new List<Filter>(FilterList.GetNewVersion(filters));
            }

            return true;
        }

        private int GetFreeSlotID()
        {
            for (int i = 0; i < _servers.Length; i++)
            {
                if (!_servers[i].IsOccupied)
                {
                    return i;
                }
            }

            return 0;
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetSaveData(GetPrefabIDString());
        }

        private IEnumerable<ServerData> SaveRackData()
        {
            foreach (RackSlot server in _servers)
            {
                if (server.Server != null)
                {
                    yield return new ServerData{Server = server.Server, ServerFilters = server.Filter };
                }
            }
        }

        private void CheckIfRemoved()
        {
            foreach (RackSlot rackSlot in _servers)
            {
                if (rackSlot != null && rackSlot.Slot != null)
                {
                    if (rackSlot.IsOccupied && rackSlot.Slot.childCount == 0)
                    {
                        rackSlot.DisconnectFromRack();
                    }
                }
            }
        }

        internal void UpdatePowerUsage()
        {
            PowerManager.UpdatePowerUsage((QPatch.Configuration.Config.ServerPowerUsage * GetServerCount() + QPatch.Configuration.Config.RackPowerUsage));
        }

        internal bool IsRackOpen()
        {
            return AnimationManager.GetIntHash(_rackDoor) == 1;
        }

        /// <summary>
        /// Gets the amount of servers in the rack
        /// </summary>
        /// <returns><see cref="int"/> of the amount of servers</returns>
        internal int GetServerCount()
        {
            return _servers.Count(rackSlot => rackSlot != null && rackSlot.IsOccupied);
        }

        /// <summary>
        /// Gets a server that is not full.
        /// </summary>
        /// <returns></returns>
        internal RackSlot GetUsableServer(TechType techType)
        {
            QuickLogger.Debug($"Getting Usable Server in Rack ID: {_prefabID}", true);
            return _servers?.FirstOrDefault(x => !x.IsFull() && x.IsAllowedToAdd(techType) && x.IsOccupied);
        }

        internal bool AddServer(HashSet<ObjectData> server,List<Filter> filters)
        {
            if (server == null)
            {
                QuickLogger.Error("Server was null while trying to add to the container operation canceled!");
                return false;
            }

            QuickLogger.Debug($"In Add Server Adding", true);
            var freeSlotId = GetFreeSlotID();
            var success = AddServerToSlot(server,filters, freeSlotId);

            QuickLogger.Debug($"Success Result = {success}");

            if (success)
            {
                //server.transform.SetParent(slot.Slot, false);
                //server.transform.localPosition = new Vector3(server.transform.localPosition.x, server.transform.localPosition.y + -0.0397f, server.transform.localPosition.z);
                DisplayManager.UpdateContainerAmount();
                QuickLogger.Debug("Made it");
                Mod.OnBaseUpdate?.Invoke();
            }

            return success;
        }

        private void OnContainerUpdate(DSSRackController dssRackController)
        {
            if (dssRackController == this)
            {
                DisplayManager.UpdateContainerAmount();
                Mod.OnBaseUpdate?.Invoke();
            }
        }

        public override void Save(SaveData newSaveData)
        {
            if (!IsInitialized
                || !IsConstructed) return;

            var id = GetPrefabIDString();

            if (_savedData == null)
            {
                _savedData = new SaveDataEntry();
            }

            _savedData.ID = id;
            _savedData.BodyColor = ColorManager.GetMaskColor().ColorToVector4();
            _savedData.Servers = SaveRackData().ToList();
            newSaveData.Entries.Add(_savedData);
        }

        internal void AddToBaseManager(BaseManager managers = null)
        {
            if (SubRoot == null)
            {
                SubRoot = GetComponentInParent<SubRoot>();
            }

            Manager = managers ?? BaseManager.FindManager(SubRoot);
            
            Manager.AddRack(this);
        }

        public string GetPrefabIDString()
        {
            if (!string.IsNullOrEmpty(_prefabID)) return _prefabID;

            var id = GetComponentInChildren<PrefabIdentifier>() ?? GetComponentInParent<PrefabIdentifier>();
            _prefabID = id != null ? id.Id : string.Empty;

            return _prefabID;
        }

        public override void Initialize()
        {
            FindSlots();

            _audioManager = new DSSAudioHandler(transform);
            
            Mod.OnContainerUpdate += OnContainerUpdate;

            _rackDoor = Animator.StringToHash("WallMountRackDriveState");
            _buttonState = Animator.StringToHash("ButtonState");

            InvokeRepeating(nameof(CheckIfRemoved), 0.5f, 0.5f);

            if (PowerManager == null)
            {
                PowerManager = new PowerManager();
                PowerManager.OnPowerUpdate += OnPowerUpdate;
            }

            if (ColorManager == null)
            {
                ColorManager = gameObject.AddComponent<ColorManager>();
                ColorManager.Initialize(gameObject, DSSModelPrefab.BodyMaterial);
            }

            if (AnimationManager == null)
            {
                AnimationManager = gameObject.AddComponent<AnimationManager>();
            }

            if (DisplayManager == null)
            {
                DisplayManager = gameObject.AddComponent<DSSRackDisplayController>();
                DisplayManager.Setup(this);
            }

            if (DumpContainer == null)
            {
                DumpContainer = gameObject.AddComponent<DumpContainer>();
                DumpContainer.Initialize(transform, AuxPatchers.DriveReceptacle(), AuxPatchers.NotAllowed(), AuxPatchers.RackFull(), this, 1, 1);
            }

            AddToBaseManager();
        }
        
        private void OnPowerUpdate(FCSPowerStates state,BaseManager manager)
        {
            switch (state)
            {
                case FCSPowerStates.Powered:
                    DisplayManager.PowerOnDisplay();
                    break;
                case FCSPowerStates.Tripped:
                    DisplayManager.PowerOffDisplay();
                    break;
                case FCSPowerStates.Unpowered:
                    DisplayManager.PowerOffDisplay();
                    break;
            }
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoSerialize");

            if (!Mod.IsSaving())
            {
                var id = GetPrefabIDString();
                QuickLogger.Info($"Saving {id}");
                Mod.Save();
                QuickLogger.Info($"Saved {id}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoDeserialize");

            if (_savedData == null)
            {
                ReadySaveData();
            }

            _fromSave = true;
        }

        public override void OnConstructedChanged(bool constructed)
        {

            _isConstructed = constructed;

            if (constructed)
            {
                if (isActiveAndEnabled)
                {
                    if (!IsInitialized)
                    {
                        Initialize();
                    }

                    IsInitialized = true;
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        public override bool CanDeconstruct(out string reason)
        {
            if (HasServers())
            {
                reason = AuxPatchers.HasItemsMessage();
                return false;
            }
            reason = string.Empty;
            return true;
        }

        private bool HasServers()
        {
            var result = _servers?.Count(x => x != null && x.IsOccupied);
            return result > 0;
        }
        
        public bool HasItem(TechType techType)
        {
            QuickLogger.Debug($"Checking for TechType: {techType}", true);
            foreach (var rackSlot in _servers)
            {
                if (rackSlot == null || rackSlot.Server == null)
                {
                    continue;
                }

                if (rackSlot.Server.Any(x => x.TechType == techType))
                {
                    return true;
                }
            }

            return false;
        }

        public RackSlot GetServerWithItem(TechType techType)
        {
            QuickLogger.Debug($"Checking for TechType: {techType}", true);
            for (var i = 0; i < _servers.Length; i++)
            {
                var rackSlot = _servers[i];
                if (rackSlot == null || rackSlot.Server == null)
                {
                    continue;
                }

                if (rackSlot.Server.Any(x => x.TechType == techType))
                {
                    return rackSlot;
                }
            }

            return null;
        }
        
        private RackSlot GetServerWithObjectData(ObjectData data)
        {
            for (int i = 0; i < _servers.Length; i++)
            {
                if (_servers[i].Server == null) continue;

                for (int j = 0; j < _servers[i].Server.Count; j++)
                {
                    if (_servers[i].Server.ElementAt(j) == data) return _servers[i];
                }
            }

            return null;
        }

        public void ToggleRackState(bool forceOpen = false)
        {
            if (!forceOpen)
            {
                AnimationManager.SetIntHash(_rackDoor, AnimationManager.GetIntHash(_rackDoor) < RackDoorStateOpen ? RackDoorStateOpen : RackDoorStateClosed);
                AnimationManager.SetBoolHash(_buttonState, AnimationManager.GetBoolHash(_buttonState) != true);
            }
            else
            {
                if (AnimationManager.GetIntHash(_rackDoor) == RackDoorStateOpen) return;
                AnimationManager.SetIntHash(_rackDoor, RackDoorStateOpen);
                AnimationManager.SetBoolHash(_buttonState, AnimationManager.GetBoolHash(_buttonState) != true);
            }

            if(AnimationManager.GetIntHash(_rackDoor) < 0) return;
            _audioManager.PlaySound(Convert.ToBoolean(AnimationManager.GetIntHash(_rackDoor)));
        }

        public Vector2 GetTotalStorage()
        {
            QuickLogger.Debug("Getting Total Storage", true);
            var amount = 0;
            var storage = 0;

            foreach (var rackSlot in _servers)
            {
                if (rackSlot == null || !rackSlot.IsOccupied || rackSlot.Server ==null) continue;
                amount += rackSlot.Server.Count;
                storage += QPatch.Configuration.Config.ServerStorageLimit;
            }

            return new Vector2(amount, storage);
        }

        public bool CanBeStored(int amount,TechType techType = TechType.None)
        {
            return IsRackSlotsFull;
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            var originalController = item.item.GetComponent<DSSServerController>();
            var server = AddServer(originalController.Items,originalController.Filters);
            Destroy(item.item.gameObject);
            return server;
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return pickupable.GetTechType() == QPatch.Server.TechType && !IsRackSlotsFull;
        }

        public Pickupable RemoveItemFromContainer(TechType techType, int amount)
        {
            throw new NotImplementedException();
        }

        public Dictionary<TechType, int> GetItemsWithin()
        {
            var result = new Dictionary<TechType, int>();

            foreach (RackSlot rackSlot in _servers)
            {
                if (rackSlot.Server == null) continue;
                foreach (ObjectData data in rackSlot.Server)
                {
                    if (result.ContainsKey(data.TechType))
                    {
                        result[data.TechType] += 1;
                    }
                    else
                    {
                        result.Add(data.TechType, 1);
                    }
                }
            }
            return result;
        }

        internal void AddItemToAServer(InventoryItem item)
        {
            QuickLogger.Debug($"Adding Item to Server {item.item.GetTechType()}");
            var techType = item.item.GetTechType();
            var server = GetUsableServer(techType);

            if (server == null)
            {
                QuickLogger.Debug("Usable server returned null",true);
                return;
            }

            server.Add(DSSHelpers.MakeObjectData(item,server.Id));
            AddToTrackedItems(techType);
            Destroy(item.item.gameObject);
        }

        private void AddToTrackedItems(TechType techType)
        {
            if (_trackedItems.ContainsKey(techType))
            {
                _trackedItems[techType] += 1;
            }
        }

        internal void RemoveFromTrackedItems(TechType techType)
        {
            if (_trackedItems.ContainsKey(techType))
            {
                if (_trackedItems[techType] == 1)
                {
                    _trackedItems.Remove(techType);
                }
                else
                {
                    _trackedItems[techType] -= 1;
                }
                
            }
        }

        public ObjectDataTransferData GetItemDataFromServer(TechType techType)
        {
            QuickLogger.Debug($"Checking for TechType: {techType}", true);
            foreach (var rackSlot in _servers)
            {
                if (rackSlot?.Server == null)
                {
                    continue;
                }

                foreach (var item in rackSlot.Server)
                {
                    if (item.TechType == techType)
                    {
                        return new ObjectDataTransferData { data = item, IsServer = false };
                    }
                }
            }

            return new ObjectDataTransferData();
        }

        public bool GivePlayerItem(TechType techType, ObjectDataTransferData data)
        {
            return DSSHelpers.GivePlayerItem(techType, data, GetServerWithObjectData);
        }

        public bool IsTechTypeAllowedInRack(TechType techType)
        {
            if (_servers.Where(rackSlot => rackSlot.HasFilters).Any(rackSlot => rackSlot.IsAllowedToAdd(techType)))
            {
                return true;
            }

            return _servers.Where(rackSlot => !rackSlot.HasFilters).Any(rackSlot => rackSlot.IsAllowedToAdd(techType));
        }

        internal bool HasFilters()
        {
            return _servers.Any(rackSlot => rackSlot != null && rackSlot.HasFilters);
        }

        public int GetItemCount(TechType techType)
        {
            int amount = 0;

            for (int i = 0; i < _servers.Length; i++)
            {
                amount += _servers[i].GetItemCount(techType);
            }

            return amount;
        }

        public bool CanHoldItem(int amount,TechType itemTechType,bool checkFilters = false)
        {
            var storage = GetTotalStorage();
            if (!IsTechTypeAllowedInRack(itemTechType) || GetIsFull() || storage.x + amount > storage.y)
            {
                return false;
            }

            if (checkFilters)
            {
                foreach (RackSlot rackSlot in _servers)
                {
                    if (rackSlot == null || !rackSlot.IsOccupied || rackSlot.IsFull() || !rackSlot.HasFilters) continue;
                    if (rackSlot.IsAllowedToAdd(itemTechType)) return true;
                }
            }
            else
            {
                foreach (RackSlot rackSlot in _servers)
                {
                    if (rackSlot == null || !rackSlot.IsOccupied || rackSlot.IsFull() || rackSlot.HasFilters) continue;
                    if (rackSlot.IsAllowedToAdd(itemTechType)) return true;
                }
            }

            return false;
        }

        public bool ContainsItem(TechType techType)
        {
            throw new NotImplementedException();
        }
    }
}