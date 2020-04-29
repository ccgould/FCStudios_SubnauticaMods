using System;
using System.Collections.Generic;
using System.Linq;
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
        private List<ObjectData> _currentServer;
        private int _rackDoor;
        private int _buttonState;
        private GameObject _drives;


        public int GetContainerFreeSpace { get; }
        public bool IsFull => GetIsFull();
        public bool IsRackSlotsFull => GetIsRackFull();
        public override bool IsConstructed => _isConstructed;
        public SubRoot SubRoot { get; private set; }
        public BaseManager Manager { get; private set; }
        public TechType TechType => GetTechType();
        internal AnimationManager AnimationManager { get; private set; }
        internal DSSRackDisplayController DisplayManager { get; private set; }
        internal DumpContainer DumpContainer { get; private set; }
        internal ColorManager ColorManager { get; private set; }

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
            BaseManager.RemoveUnit(this);
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
            int amount = _servers.Count(server => server.IsOccupied);

            return amount >= _servers.Length;
        }

        private bool GetIsFull()
        {
            var server = _servers.Any(x => x.Server != null);
            if (server == false) return true;
            var storage = GetTotalStorage();
            if (storage.x >= storage.y) return true;
            return false;
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

        private bool AddServerToSlot(List<ObjectData> server,List<Filter> filters, int slotID)
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
            SetSlotOccupiedState(slotID, true);
            QuickLogger.Debug($"Current ID Occupied Stat = {slotByID.IsOccupied}");
            QuickLogger.Debug($"Data Count = {server.Count}");

            slotByID.Server = new List<ObjectData>(server);
            if (filters != null)
            {
                slotByID.Filter = new List<Filter>(filters);
            }

            return true;
        }

        private void SetSlotOccupiedState(int slotID, bool state)
        {
            GetSlotByID(slotID).IsOccupied = state;
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
            return _servers?.Length ?? 0;
        }

        /// <summary>
        /// Gets a server that is not full.
        /// </summary>
        /// <returns></returns>
        internal RackSlot GetUsableServer(TechType techType)
        {
            return _servers?.FirstOrDefault(x => !x.IsFull() && x.IsAllowedToAdd(techType) && x.IsOccupied);
        }

        internal bool AddServer(List<ObjectData> server,List<Filter> filters)
        {
            if (server == null)
            {
                QuickLogger.Error("Server was null while trying to add to the container operation cancelled!");
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

        private void OnContainerUpdate()
        {
            DisplayManager.UpdateContainerAmount();
            Mod.OnBaseUpdate?.Invoke();
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
            Manager.AddUnit(this);
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
            AddToBaseManager();

            FindSlots();

            Mod.OnContainerUpdate += OnContainerUpdate;

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
                DumpContainer.Initialize(transform, AuxPatchers.DriveReceptacle(), AuxPatchers.NotAllowed(), AuxPatchers.RackFull(), this, 1, 1);
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
            var result = _servers.Count(x => x.IsOccupied);
            return result > 0;
        }

        internal Dictionary<TechType, int> GetStoredData()
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

        public bool HasItem(TechType techType)
        {
            QuickLogger.Debug($"Checking for TechType: {techType}", true);
            foreach (var rackSlot in _servers)
            {
                if (rackSlot == null || rackSlot.Server == null)
                {
                    QuickLogger.Debug("Server is null", true);
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
            foreach (var rackSlot in _servers)
            {
                if (rackSlot == null || rackSlot.Server == null)
                {
                    QuickLogger.Debug("Server is null", true);
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
            foreach (RackSlot rackSlot in _servers)
            {
                if (rackSlot.Server == null) continue;
                foreach (var objectData in rackSlot.Server)
                {
                    if (objectData == data) return rackSlot;
                }
            }

            return null;
        }

        public void ToggleRackState(bool forceOpen = false)
        {
            if (!forceOpen)
            {
                AnimationManager.SetIntHash(_rackDoor, AnimationManager.GetIntHash(_rackDoor) < 1 ? 1 : 0);
                AnimationManager.SetBoolHash(_buttonState, AnimationManager.GetBoolHash(_buttonState) != true);
            }
            else
            {
                if (AnimationManager.GetIntHash(_rackDoor) != 0) return;
                AnimationManager.SetIntHash(_rackDoor, 1);
                AnimationManager.SetBoolHash(_buttonState, AnimationManager.GetBoolHash(_buttonState) != true);
            }
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

        public bool CanBeStored(int amount)
        {
            return IsRackSlotsFull;
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            var originalController = item.item.GetComponent<DSSServerController>();

            //var serverClone = GameObject.Instantiate(item.item);
            //serverClone.gameObject.SetActive(true);
            //serverClone.GetComponent<Rigidbody>().isKinematic = true;

            //var controller = serverClone.GetComponent<DSSServerController>();
            //controller.Items = new List<ObjectData>(originalController.Items);
            //controller.DisplayManager.UpdateDisplay();
            var server = AddServer(originalController.Items,originalController.Filters);
            Destroy(item.item.gameObject);
            return server;
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return pickupable.GetTechType() == QPatch.Server.TechType && !IsRackSlotsFull;
        }

        internal void AddItemToAServer(InventoryItem item)
        {
            var server = GetUsableServer(item.item.GetTechType());
            if (server == null) return;
            server.Add(MakeObjectData(item,server.Id));
            Destroy(item.item.gameObject);
        }

        private ObjectData MakeObjectData(InventoryItem item,int slot)
        {
            var go = item.item.gameObject;
            
            var objectType = FindSaveDataObjectType(go);

            ObjectData result;

            switch (objectType)
            {
                case SaveDataObjectType.Item:
                    result = new ObjectData {DataObjectType = objectType,TechType = item.item.GetTechType()};
                    break;
                case SaveDataObjectType.PlayerTool:
                    result = new ObjectData { DataObjectType = objectType, TechType = item.item.GetTechType(),PlayToolData = GetPlayerToolData(item)};
                    break;
                case SaveDataObjectType.Eatable:
                    result = new ObjectData { DataObjectType = objectType, TechType = item.item.GetTechType(), EatableEntity = GetEatableData(item)};
                    break;
                case SaveDataObjectType.Server:
                    result = new ObjectData { DataObjectType = objectType, TechType = item.item.GetTechType(),  ServerData= GetServerData(item) };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return result;
        }

        private List<ObjectData> GetServerData(InventoryItem item)
        {
            var data = item.item.GetComponent<DSSServerController>().Items;
            return new List<ObjectData>(data);
        }

        private EatableEntities GetEatableData(InventoryItem item)
        {
            var eatableEntity = new EatableEntities();
            eatableEntity.Initialize(item.item,false);
            return eatableEntity;
        }

        private PlayerToolData GetPlayerToolData(InventoryItem item)
        {
            var energyMixin = item.item.GetComponentInChildren<EnergyMixin>();
            
            var playerToolData = new PlayerToolData {TechType = item.item.GetTechType()};

            if (energyMixin == null) return playerToolData;

            var batteryGo = energyMixin.GetBattery().gameObject;
            var techType = batteryGo.GetComponent<TechTag>().type;
            var iBattery = batteryGo.GetComponent<IBattery>();
            playerToolData.BatteryInfo = new BatteryInfo(techType,iBattery,string.Empty);

            return playerToolData;
        }

        private SaveDataObjectType FindSaveDataObjectType(GameObject go)
        {
            SaveDataObjectType objectType;

            if (go.GetComponent<Eatable>())
            {
                objectType = SaveDataObjectType.Eatable;
            }
            else if (go.GetComponent<DSSServerController>()) // Must check for Server first before playertool
            {
                objectType = SaveDataObjectType.Server;
            }
            else if (go.GetComponent<PlayerTool>())
            {
                objectType = SaveDataObjectType.PlayerTool;
            }
            else
            {
                objectType = SaveDataObjectType.Item;
            }

            return objectType;
        }

        public ObjectDataTransferData GetItemDataFromServer(TechType techType)
        {
            QuickLogger.Debug($"Checking for TechType: {techType}", true);
            foreach (var rackSlot in _servers)
            {
                if (rackSlot?.Server == null)
                {
                    QuickLogger.Debug("Server is null", true);
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
    }

    internal class ServerData
    {
        public List<ObjectData> Server { get; set; }
        public List<Filter> ServerFilters { get; set; }
    }
}