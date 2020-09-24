using System;
using System.CodeDom;
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
        public bool IsFull => GetIsFull();
        public bool IsRackSlotsFull => GetIsRackFull();
        internal Action OnUpdate;
        private bool _allowedToNotify = true;
        private Dictionary<TechType, int> _rackItemsTracker = new Dictionary<TechType, int>();
        Action<int, int> IFCSStorage.OnContainerUpdate { get; set; }

        public override BaseManager Manager { get; set; }

        public TechType TechType => GetTechType();
        internal AnimationManager AnimationManager { get; private set; }
        internal DSSRackDisplayController DisplayManager { get; private set; }
        public override DumpContainer DumpContainer { get; set; }
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
            IsInitialized = false;
            BaseManager.RemoveRack(this);
        }

        private void Update()
        {
            OnUpdate?.Invoke();
        }

        #endregion

        private int CalculateContainerFreeSpace()
        {
            var storage = GetTotalStorage();
            return (int)storage.y - (int)storage.x;
        }

        private void LoadRack()
        {
            QuickLogger.Debug("Load Rack");
            if (_savedData.RackServers == null) return;

            _allowedToNotify = false;

            QuickLogger.Debug($"Save Data Count: {_savedData.RackServers.Count()} || Global Servers Count: { BaseManager.GlobalServers.Count}");

            foreach (string server in _savedData.RackServers)
            {
                foreach (DSSServerController controller in BaseManager.GlobalServers)
                {
                    if (controller.GetPrefabID() == server)
                    {

                        var pickup = controller.gameObject.GetComponent<Pickupable>();
                        if (pickup != null)
                            AddServerToSlot(pickup.ToInventoryItem(),controller.GetSlotID());
                    }
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

        private bool AddServerToSlot(InventoryItem server, int slotID)
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

            slotByID.ConnectServer(server);

            UpdateScreen();
            return true;
        }

        public override void UpdateScreen()
        {
            DisplayManager?.UpdateContainerAmount();
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
        
        private void SendNotification()
        {
            if (_allowedToNotify)
            {
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
            _savedData.RackServers = GetServers();
            newSaveData.Entries.Add(_savedData);
        }

        private IEnumerable<string> GetServers()
        {
            foreach (RackSlot rackSlot in _servers)
            {
                if(!rackSlot.IsOccupied)continue;
                yield return rackSlot.GetConnectedServer().GetPrefabID();
            }
        }

        internal void AddToBaseManager(BaseManager managers = null)
        {
            Manager = managers ?? BaseManager.FindManager(gameObject);
            
            Manager.RegisterRack(this);
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
            
            _rackDoor = Animator.StringToHash("WallMountRackDriveState");
            _buttonState = Animator.StringToHash("ButtonState");

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
        
        #region Rack Operations
        public bool AddItemToContainer(InventoryItem item)
        {
            AddServerToSlot(item, GetFreeSlotID());
            return true;
        }

        public Pickupable RemoveItemFromContainer(TechType techType, int amount)
        {
            return GetSlotWithItem(techType)?.RemoveItemFromServer(techType);
        }

        private RackSlot GetSlotWithItem(TechType techType)
        {
            return _servers.FirstOrDefault(x => x.IsOccupied && x.GetConnectedServer().HasItem(techType));
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return CanBeStored(DumpContainer.GetCount(), pickupable.GetTechType());
        }

        public bool CanBeStored(int amount, TechType techType = TechType.None)
        {
            return !IsRackSlotsFull && CanHoldServerAmount(amount);
        }

        public Vector2 GetTotalStorage()
        {
            QuickLogger.Debug("Getting Total Storage", true);
            var amount = 0;
            var storage = 0;

            foreach (var rackSlot in _servers)
            {
                if (rackSlot == null || !rackSlot.IsOccupied) continue;
                amount += rackSlot.GetTotal();
                storage += QPatch.Configuration.Config.ServerStorageLimit;
            }

            return new Vector2(amount, storage);
        }

        public bool IsTechTypeAllowedInRack(TechType techType)
        {
            if (_servers.Where(rackSlot => rackSlot.HasFilters()).Any(rackSlot => rackSlot.IsAllowedToAdd(techType)))
            {
                return true;
            }

            return _servers.Where(rackSlot => !rackSlot.HasFilters()).Any(rackSlot => rackSlot.IsAllowedToAdd(techType));
        }

        public bool IsAllowedToRemoveItems()
        {
            return true;
        }

        internal bool HasFilters()
        {
            return _servers.Any(rackSlot => rackSlot != null && rackSlot.HasFilters());
        }

        private bool HasServers()
        {
            var result = _servers?.Count(x => x != null && x.IsOccupied);
            return result > 0;
        }

        internal bool IsRackOpen()
        {
            return AnimationManager.GetIntHash(_rackDoor) == 1;
        }

        public bool ContainsItem(TechType techType)
        {
            return _rackItemsTracker.Any(x => x.Key== techType);
        }

        #endregion


        public Dictionary<TechType, int> GetItemsWithin()
        {
            return _rackItemsTracker;
        }
        
        #region Server Operations

        internal Dictionary<TechType,int> GetTrackedItems()
        {
            return _rackItemsTracker;
        }

        internal void AddItemToAServer(InventoryItem item,int slot = -1)
        {
            if (slot >= -1) return;
            _servers[slot].AddItemToServer(item);
        }

        #endregion
        
        internal void FillRack()
        {
            for (int i = 0; i < _servers.Length; i++)
            {
                if (!_servers[i].IsOccupied)
                {
                    AddServerToSlot(Mod.ServerClassID.ToTechType().ToInventoryItem(), i);
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
                        _servers[i].AddItemToServer(Mod.AllTechTypes[index].ToInventoryItem());
                    }
                }
            }
        }
    }
}