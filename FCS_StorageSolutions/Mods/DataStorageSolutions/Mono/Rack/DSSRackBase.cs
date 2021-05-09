﻿using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_StorageSolutions.Configuration;
using FCS_StorageSolutions.Helpers;
using FCS_StorageSolutions.Mods.AlterraStorage.Buildable;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Rack
{
    internal abstract class DSSRackBase : FcsDevice,IFCSSave<SaveData>,IHandTarget, IDSSRack,IFCSDumpContainer
    {
        private bool _runStartUpOnEnable;
        private Text _storageAmount;
        private List<GameObject> _readers;
        private Image _percentageBar;
        private GameObject _canvas;
        private bool _isVisible;
        private DumpContainerSimplified _dumpContainer;
        private FCSStorage _storage;
        private bool _isBeingDestroyed;
        
        protected abstract int StorageWidth { get; }
        protected abstract int StorageHeight { get; }
        protected bool IsFromSave;
        protected abstract DSSServerRackDataEntry SavedData { get; set; }
        protected Dictionary<string,DSSSlotController> Slots;
        
        public override bool IsOperational => IsInitialized && IsConstructed;
        public override bool IsRack { get; } = true;
        public override bool IsVisible => _isVisible;

        public override float GetPowerUsage()
        {
            if (Manager == null || Manager.GetBreakerState() || !IsConstructed) return 0f;

            return 0.01f + Slots.Count(x => x.Value != null && x.Value.IsOccupied) * 0.01f;
        }

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.DSSTabID, Mod.ModName);
            Manager.OnPowerStateChanged += OnPowerStateChanged;
            Manager.OnBreakerStateChanged += OnBreakerStateChanged;
            UpdateStorageCount();
            UpdateScreenState();
            _storage?.CleanUpDuplicatedStorageNoneRoutine();
            Mod.CleanDummyServers();
            _canvas.FindChild("Home").FindChild("UnitID").GetComponentInChildren<Text>().text = $"UNIT ID: {UnitID}";
        }

        private void OnBreakerStateChanged(bool value)
        {
            UpdateScreenState();
        }

        private void OnPowerStateChanged(PowerSystem.Status obj)
        {
            UpdateScreenState();
        }

        private void UpdateScreenState()
        {
            if (Manager.GetBreakerState() || Manager.GetPowerState() == PowerSystem.Status.Offline)
            {
                TurnOffDevice();
            }
            else
            {
                TurnOnDevice();
            }
        }

        private void RegisterServers()
        {
            if(Manager == null) return;
            foreach (KeyValuePair<string, DSSSlotController> controller in Slots)
            {
                if (controller.Value != null && controller.Value.IsOccupied && controller.Value.GetServer() != null)
                {
                    Manager.RegisterServerInBase(controller.Value.GetServer());
                    
                }
            }

            foreach (KeyValuePair<string, DSSSlotController> controller in Slots)
            {
                if (controller.Value != null && controller.Value.IsOccupied && controller.Value.GetTransceiver() != null)
                {
                    Manager.AddTransceiver(controller.Value.GetTransceiver());
                }
            }
            
        }

        private void Update()
        {

        }
        
        private void OnEnable()
        {
            if (_runStartUpOnEnable)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }

                if (IsFromSave)
                {
                    if (SavedData == null)
                    {
                        ReadySaveData();
                    }

                    if (SavedData != null)
                    {
                        _colorManager.ChangeColor(SavedData.BodyColor.Vector4ToColor());
                        _colorManager.ChangeColor(SavedData.SecondaryColor.Vector4ToColor(),ColorTargetMode.Secondary);
                    }
                }

                _runStartUpOnEnable = false;
            }
        }

        public override void OnDestroy()
        {
            if (Manager != null)
            {
                Manager.OnPowerStateChanged -= OnPowerStateChanged;
                Manager.OnBreakerStateChanged -= OnBreakerStateChanged;
            }

            base.OnDestroy();
            _isBeingDestroyed = true;
        }

        public override Vector3 GetPosition()
        {
            return transform.position;
        }

        public override void Initialize()
        { 
            if(IsInitialized) return;
            _canvas = gameObject.GetComponentInChildren<Canvas>()?.gameObject;

            if (_canvas == null) return;

            Slots = new Dictionary<string,DSSSlotController>();
            
            _percentageBar = _canvas.FindChild("Home").FindChild("BGPreloader").FindChild("Preloader").GetComponent<Image>();
            _storageAmount = _canvas.FindChild("Home").FindChild("BGPreloader").FindChild("Amount").GetComponentInChildren<Text>();
            
            var insertBtn = _canvas.FindChild("Home").FindChild("InsertButton").GetComponent<Button>();
            insertBtn.onClick.AddListener((() =>
            {
                foreach (KeyValuePair<string, DSSSlotController> slot in Slots)
                {
                    if (!slot.Value.IsOccupied)
                    {
                        _dumpContainer.OpenStorage();
                        break;
                    }

                    QuickLogger.ModMessage("Rack is Full");
                }
            }));
            
            CreateDumpContainer();
            
            FindReaderDisplays();

            CreateSlots();

            CreateStorage();

            CreateColorManager();

            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseEmissiveDecalsController, gameObject, Color.cyan);
            
            InvokeRepeating(nameof(RegisterServers), 1f, 1f);
            InvokeRepeating(nameof(UpdateScreenState), 1, 1);
            IsInitialized = true;
        }

        private void CreateColorManager()
        {
            if (_colorManager == null)
            {
                _colorManager = gameObject.EnsureComponent<ColorManager>();
                _colorManager.Initialize(gameObject, ModelPrefab.BodyMaterial, ModelPrefab.SecondaryMaterial);
            }
        }

        private void CreateDumpContainer()
        {
            if (_dumpContainer == null)
            {
                _dumpContainer = gameObject.EnsureComponent<DumpContainerSimplified>();
                _dumpContainer.Initialize(transform, $"Add server to rack", this, StorageWidth, StorageHeight, "Storage");
            }
        }

        private void CreateStorage()
        {
            if (_storage == null)
            {
                _storage = gameObject.AddComponent<FCSStorage>();
                _storage.Initialize(Slots.Count);
                _storage.ItemsContainer.onAddItem += OnServerAddedToStorage;
                _storage.ItemsContainer.onRemoveItem += OnServerRemovedFromStorage;
            }
        }

        private void CreateSlots()
        {
            var meters = GameObjectHelpers.FindGameObject(gameObject, "SlotGrid").transform;
            var metersTemp = new List<GameObject>();

            foreach (Transform meter in meters)
            {
                metersTemp.Add(meter.gameObject);
            }

            for (var i = 0; i < metersTemp.Count; i++)
            {
                QuickLogger.Debug($"");
                GameObject slot = metersTemp[i];
                var meter = metersTemp[i];
                var slotName = $"Slot {i + 1}";
                var slotController = slot.AddComponent<DSSSlotController>();
                slotController.Initialize(slotName, this, meter, _readers?[i]);

                Slots.Add(slotName, slotController);
            }
        }

        private void FindReaderDisplays()
        {
            var readers = gameObject.FindChild("ReaderCanvases")?.transform;
            if (readers != null)
            {
                _readers = new List<GameObject>();
                foreach (Transform reader in readers)
                {
                    _readers.Add(reader.gameObject);
                }
            }
        }

        private void OnServerAddedToStorage(InventoryItem item)
        {
            foreach (KeyValuePair<string, DSSSlotController> slot in Slots)
            {
                if(slot.Value.IsOccupied) continue;
                slot.Value.MountServerToRack(item);
                break;
            }
            //
            QuickLogger.Debug($"Server Added {item.item.GetTechName()}",true);
        }

        public void UpdateStorageCount()
        {
            QuickLogger.Debug("UpdateStorageCount",true);
            if (Slots == null) return;
            var storageTotal = 0;
            var storageAmount = 0;
            foreach (KeyValuePair<string, DSSSlotController> controller in Slots)
            {
                if (controller.Value != null && controller.Value.IsOccupied && controller.Value.IsServer)
                {
                    storageTotal += 48;
                    storageAmount += controller.Value.GetStorageAmount();
                }
            }

            _storageAmount.text = AuxPatchers.AlterraStorageAmountFormat(storageAmount, storageTotal);
            _percentageBar.fillAmount = (float)storageAmount / (float)storageTotal;
        }

        public bool HasSpace(int amount)
        {
            //TODO Deal with filters
            return Slots.Any(x => x.Value.IsOccupied && x.Value.HasSpace(amount));
        }

        public bool AddItemToRack(InventoryItem item)
        {
            try
            {
                var result = TransferHelpers.AddItemToRack(this, item,1);
                if (!result)
                {
                    PlayerInteractionHelper.GivePlayerItem(item);
                }
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
                QuickLogger.Error(e.Source);
                return false;
            }

            return true;
        }

        public int GetFreeSpace()
        {
            int amount = 0;
            foreach (var controller in Slots)
            {
                if (controller.Value != null && controller.Value.IsOccupied)
                {
                    amount += controller.Value.GetFreeSpace();
                }
            }

            return amount;
        }

        public bool ItemAllowed(TechType techType, out ISlotController server)
        {
            server = null;
            if (Slots == null) return false;

            foreach (KeyValuePair<string, DSSSlotController> controller in Slots)
            {
                if (controller.Value != null && controller.Value.IsOccupied && !controller.Value.IsFull && controller.Value.IsTechTypeAllowed(techType))
                {
                    server = controller.Value;
                    return true;
                }
            }

            return false;
        }

        public override int GetItemCount(TechType techType)
        {
            int amount = 0;
            foreach (KeyValuePair<string, DSSSlotController> controller in Slots)
            {
                if (controller.Value != null && controller.Value.IsOccupied)
                {
                    amount += controller.Value.GetItemCount(techType);
                }
            }

            return amount;
        }

        public bool HasItem(TechType techType)
        {
            if (Slots == null) return false;

            foreach (KeyValuePair<string, DSSSlotController> controller in Slots)
            {
                if (controller.Value != null && controller.Value.IsOccupied)
                {
                    if (controller.Value.HasItem(techType))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public Pickupable RemoveItemFromRack(TechType techType)
        {
            foreach (KeyValuePair<string, DSSSlotController> controller in Slots)
            {
                if (controller.Value != null && controller.Value.IsOccupied)
                {
                    if (controller.Value.HasItem(techType))
                    {
                        return controller.Value.RemoveItemFromServer(techType);
                    }
                }

            }

            return null;

        }
        
        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoSerialize");

            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {GetPrefabID()}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {GetPrefabID()}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            
            if (SavedData == null)
            {
                ReadySaveData();
            }

            if (!IsInitialized)
            {
                Initialize();
            }
        }

        public override bool CanDeconstruct(out string reason)
        {
            if (Slots != null)
            {
                if (Slots.Any(x => x.Value != null && x.Value.IsOccupied))
                {
                    reason = AuxPatchers.RackNotEmpty();
                    return false;
                }
            }

            reason = string.Empty;
            return true;
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

        public virtual void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            try
            {
                if (!IsInitialized || !IsConstructed) return;

                if (SavedData == null)
                {
                    SavedData = new DSSServerRackDataEntry();
                }
                SavedData.SaveVersion = "2.0";
            }
            catch (Exception e)
            {
                QuickLogger.Error($"Failed to save {UnitID}:");
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
                QuickLogger.Error(e.InnerException);
            }
        }

        protected virtual void ReadySaveData()
        {

        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }

        public void OnHandHover(GUIHand hand)
        {
            if (!IsConstructed || !IsInitialized) return;
            //HandReticle main = HandReticle.main;
            //main.SetIcon(HandReticle.IconType.Default);
            //main.SetInteractText("N/A");
        }

        public void OnHandClick(GUIHand hand)
        {
        }

        public override void TurnOnDevice()
        {
            if (_canvas == null) return;

            _isVisible = true;
            
            if (!_canvas.activeSelf)
            {
                _canvas.SetActive(true);
            }
        }

        public override void TurnOffDevice()
        {
            if (_canvas == null) return;
            _isVisible = false;
            if (_canvas.activeSelf)
            {
                _canvas.SetActive(false);
            }
        }

        public IEnumerable<KeyValuePair<string, DSSSlotController>> GetSlots()
        {
            return Slots;
        }

        public byte[] Save(ProtobufSerializer serializer)
        {
            return _storage.Save(serializer);
        }
        
        public void RestoreItems(ProtobufSerializer serializer, byte[] data)
        {
#if SUBNAUTICA_STABLE
            _storage.RestoreItems(serializer, data);
#else
                    StartCoroutine(_storageContainer.RestoreItemsAsync(_serializer, _savedData.Data));
#endif
        }

        public void RestoreItems(ProtobufSerializer serializer, List<byte[]> data)
        {
#if SUBNAUTICA_STABLE
            _storage.RestoreItems(serializer, data);
#else
                    StartCoroutine(_storageContainer.RestoreItemsAsync(_serializer, _savedData.Data));
#endif
        }

        public bool IsAllowedToAdd(TechType techType, bool verbose)
        {
            QuickLogger.Debug($"Can Be Stored Result: {_storage.CanBeStored(_dumpContainer.GetItemCount() + 1, techType)}", true);
            if (_storage.IsFull || !_storage.CanBeStored(_dumpContainer.GetItemCount() + 1, techType)) return false;
            return techType == Mod.GetDSSServerTechType() || techType == Mod.GetTransceiverTechType();
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            QuickLogger.Debug($"Can Be Stored Result: {_storage.CanBeStored(_dumpContainer.GetItemCount() + 1, pickupable.GetTechType())}",true);
            return IsAllowedToAdd(pickupable.GetTechType(), verbose);
        }

        private void OnServerRemovedFromStorage(InventoryItem item)
        {
            //TODO Fix
            //var server = item.item.gameObject.GetComponent<DSSServerController>();
            //var slot = server.GetSlot();
            //slot.ClearSlot();
        }

        /// <summary>
        /// Event when the DumpContainer has an item added
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool AddItemToContainer(InventoryItem item)
        {
            return _storage.AddItemToContainer(item);
        }
    }
}