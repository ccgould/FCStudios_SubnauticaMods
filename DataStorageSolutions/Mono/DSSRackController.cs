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
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Interfaces;
using FCSCommon.Objects;
using FCSTechFabricator.Abstract;
using FCSTechFabricator.Components;
using FCSTechFabricator.Enums;
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
        private string _prefabID;
        private TechType _techType = TechType.None;
        private int _rackDoor;
        private int _buttonState;
        private GameObject _drives;
        private DSSAudioHandler _audioManager;
        private const int RackDoorStateClosed = 0;
        private const int RackDoorStateOpen = 1;
        public int GetContainerFreeSpace => CalculateContainerFreeSpace();

        private int CalculateContainerFreeSpace()
        {
            var storage = GetTotalStorage();
            return (int)storage.y - (int)storage.x;
        }

        public bool IsFull => GetIsFull();
        public bool IsRackSlotsFull => GetIsRackFull();
        internal Action OnUpdate;
        private bool _allowedToNotify = true;
        Action<int, int> IFCSStorage.OnContainerUpdate { get; set; }

        public override BaseManager Manager { get; set; }

        public TechType TechType => GetTechType();
        internal AnimationManager AnimationManager { get; private set; }
        internal DSSRackDisplayController DisplayManager { get; private set; }
        public override DumpContainer DumpContainer { get; set; }
        internal ColorManager ColorManager { get; private set; }
        public override FCSPowerManager PowerManager { get; set; }

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
        }

        #endregion

        private void LoadRack()
        {
            QuickLogger.Debug("Load Rack");
            if (_savedData.Servers == null) return;

            _allowedToNotify = false;

            QuickLogger.Debug($"Save Data Count: {_savedData.Servers.Count}");

            foreach (ServerData data in _savedData.Servers)
            {
                if (data != null)
                {
                    var slot = GetSlotByID(GetFreeSlotID());
                    AddServer(data.Server, data.ServerFilters, data.SlotID, true);
                }
            }

            _allowedToNotify = true;
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

            GetSlotByID(slotID).IsOccupied = true;

            slotByID.LoadServer(server);
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
                    yield return new ServerData{Server = server.Server, ServerFilters = server.Filter, SlotID = server.Id};
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

        internal bool AddServer(HashSet<ObjectData> server,List<Filter> filters, int suppliedSlot = 0, bool useSuppliedSlot = false)
        {
            if (server == null)
            {
                QuickLogger.Error("Server was null while trying to add to the container operation canceled!");
                return false;
            }

            QuickLogger.Debug($"In Add Server Adding", true);

            if (!AddServerToSlot(server, filters, GetEmptySlot(suppliedSlot, useSuppliedSlot))) return false;

            DisplayManager.UpdateContainerAmount();
            QuickLogger.Debug("Made it");

            SendNotification();
            return true;

        }

        private void SendNotification()
        {
            if (_allowedToNotify)
            {
                Mod.OnBaseUpdate?.Invoke();
            }

        }

        private int GetEmptySlot(int suppliedSlot, bool useSuppliedSlot)
        {
            int assignedSlot;

            if (useSuppliedSlot && !GetSlotByID(suppliedSlot).IsOccupied)
            {
                assignedSlot = suppliedSlot;
            }
            else
            {
                assignedSlot = GetFreeSlotID();
            }

            return assignedSlot;
        }

        private void OnContainerUpdate(DSSRackController dssRackController)
        {
            if (dssRackController == this)
            {
                DisplayManager.UpdateContainerAmount();

                SendNotification();
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
            Manager = managers ?? BaseManager.FindManager(gameObject);
            
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
            
            //Mod.OnContainerUpdate += OnContainerUpdate;

            _rackDoor = Animator.StringToHash("WallMountRackDriveState");
            _buttonState = Animator.StringToHash("ButtonState");

            InvokeRepeating(nameof(CheckIfRemoved), 0.5f, 0.5f);

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
                DumpContainer.Initialize(transform, AuxPatchers.DriveReceptacle(), AuxPatchers.NotAllowed(), AuxPatchers.RackFull(), this, 1, 6);
            }

            AddToBaseManager();
        }
        
        private void OnPowerUpdate(FCSPowerStates state,BaseManager manager)
        {
            if (!IsConstructed) return;

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

            IsConstructed = constructed;

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

            if (!IsConstructed) return null;
            
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

        private bool CanHoldServerAmount(int amount)
        {
            if (_servers == null)
            {
                QuickLogger.Error("Rack Servers Array is null. Please let FCS Studios know about this issue");
                return true;
            }

            var occupiedAmount = _servers.Count(server => server.IsOccupied);

            return occupiedAmount + amount < _servers.Length;
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
            return !IsRackSlotsFull && CanHoldServerAmount(amount);
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            var originalController = item.item.GetComponent<DSSServerController>();
            var server = AddServer(originalController.FCSFilteredStorage.Items,originalController.FCSFilteredStorage.Filters);
            Destroy(item.item.gameObject);
            return server;
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return CanBeStored(DumpContainer.GetCount(), pickupable.GetTechType());
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

        internal void AddItemToAServer(InventoryItem item,int slot = -1)
        {
            QuickLogger.Debug($"Adding Item to Server {item.item.GetTechType()}");
            var techType = item.item.GetTechType();
            var server = slot == -1 ? GetUsableServer(techType) : GetSlotByID(slot);


            if (server == null)
            {
                QuickLogger.Debug("Usable server returned null",true);
                return;
            }

            server.Add(DSSHelpers.MakeObjectData(item,server.Id));
            Destroy(item.item.gameObject);
        }

        public ObjectDataTransferData GetItemDataFromServer(TechType techType, out RackSlot slot)
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
                        slot = rackSlot;
                        return new ObjectDataTransferData { data = item, IsServer = false };
                    }
                }
            }

            slot = null;
            return new ObjectDataTransferData();
        }

        public bool GivePlayerItem(TechType techType, ObjectDataTransferData data)
        {
            QuickLogger.Debug($"Giving Player Item {techType}",true);
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
        
        internal bool CanHoldItem(int amount,TechType itemTechType, out int slotID, int filterAmount = 0, bool checkFilters = false)
        {
            slotID = -1;
            var storage = GetTotalStorage();

            QuickLogger.Debug($"Server Rack: {_prefabID} Total: {storage.y} || Trying: { storage.x + amount}",true);

            if (!IsTechTypeAllowedInRack(itemTechType) || GetIsFull() || storage.x + amount > storage.y)
            {
                return false;
            }

            if (checkFilters)
            {
                foreach (RackSlot rackSlot in _servers)
                {
                    QuickLogger.Debug($"Is TechType Allowed {rackSlot.IsAllowedToAdd(itemTechType)}",true);
                    if (rackSlot == null || !rackSlot.IsOccupied || rackSlot.IsFull() || !rackSlot.HasFilters || !rackSlot.CanHoldAmount(filterAmount + 1)) continue;
                    QuickLogger.Debug("Checking Filters",true);
                    if (rackSlot.IsAllowedToAdd(itemTechType))
                    {
                        QuickLogger.Debug($"Found valid Filtered Server {rackSlot.Id} || {GetPrefabIDString()}");
                        slotID = rackSlot.Id;
                        return true;
                    }
                }
            }
            else
            {
                foreach (RackSlot rackSlot in _servers)
                {
                    QuickLogger.Debug($"Is TechType Allowed {rackSlot.IsAllowedToAdd(itemTechType)}", true);
                    if (rackSlot == null || !rackSlot.IsOccupied || rackSlot.IsFull() || rackSlot.HasFilters) continue;
                    if (rackSlot.IsAllowedToAdd(itemTechType))
                    {
                        slotID = rackSlot.Id;
                        return true;
                    }
                }
            }

            return false;
        }

        public bool ContainsItem(TechType techType)
        {
            throw new NotImplementedException();
        }


        internal void FillRack()
        {
            for (int i = 0; i < _servers.Length; i++)
            {
                if (!_servers[i].IsOccupied)
                {
                    AddServer(new HashSet<ObjectData>(), null, i);
                }
            }

            FillWithDummyData();
        }

        private void FillWithDummyData()
        {
            var random = new System.Random();
            for (int i = 0; i < _servers.Length; i++)
            {
                if (_servers[i].IsOccupied && !_servers[i].IsFull())
                {
                    for (int j = 0; j < QPatch.Configuration.Config.ServerStorageLimit; j++)
                    {
                        int index = random.Next(Mod.AllTechTypes.Count);
                        _servers[i].Add(DSSHelpers.MakeObjectData(Mod.AllTechTypes[index].ToInventoryItem(), _servers[i].Id));
                    }
                }
            }
        }
    }
}