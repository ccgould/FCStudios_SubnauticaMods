using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
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
    internal abstract class DSSRackBase : FcsDevice,IFCSSave<SaveData>,IHandTarget, IDSSRack
    {
        private bool _runStartUpOnEnable;
        protected bool _isFromSave;
        private bool _isBeingDestroyed;
        protected abstract DSSServerRackDataEntry SavedData { get; set; }
        protected Dictionary<string,DSSSlotController> Slots;
        private Text _storageAmount;
        private readonly List<GameObject> _meters = new List<GameObject>();
        private readonly List<GameObject> _readers = new List<GameObject>();
        private Image _percentageBar;
        private GameObject _canvas;
        private bool _isVisible;
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

                if (_isFromSave)
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
            Slots = new Dictionary<string,DSSSlotController>();
            _percentageBar = _canvas.FindChild("Home").FindChild("BGPreloader").FindChild("Preloader").GetComponent<Image>();
            _storageAmount = _canvas.FindChild("Home").FindChild("BGPreloader").FindChild("Amount").GetComponentInChildren<Text>();

            var insertBTN = _canvas.FindChild("Home").FindChild("InsertButton").GetComponent<Button>();
            insertBTN.onClick.AddListener((() =>
            {
                foreach (KeyValuePair<string, DSSSlotController> slot in Slots)
                {
                    if (!slot.Value.IsOccupied)
                    {
                        slot.Value.OpenContainer();
                        break;
                    }

                    QuickLogger.ModMessage("Rack is Full");
                }
            }));

            var meters = GameObjectHelpers.FindGameObject(gameObject, "SlotGrid").transform;
            QuickLogger.Debug($"Meters Count: {meters?.childCount}");
            foreach (Transform meter in meters)
            {
                QuickLogger.Debug($"Meters Name: {meter?.name}");
                _meters.Add(meter.gameObject);
            }

            var readers = gameObject.FindChild("ReaderCanvases").transform;
            QuickLogger.Debug($"Readers Count: {readers?.childCount}");
            foreach (Transform reader in readers)
            {
                QuickLogger.Debug($"Readers Name: {reader?.name}");
                _readers.Add(reader.gameObject);
            }


            for (var i = 0; i < _meters.Count; i++)
            {
                GameObject slot = _meters[i];
                var meter = _meters[i];
                var slotName = $"Slot {i+1}";
                var slotController = slot.AddComponent<DSSSlotController>();
                slotController.Initialize(slotName, this, meter, _readers[i]);

                Slots.Add(slotName, slotController);
            }

            if (_colorManager == null)
            {
                _colorManager = gameObject.EnsureComponent<ColorManager>();
                _colorManager.Initialize(gameObject,ModelPrefab.BodyMaterial,ModelPrefab.SecondaryMaterial);
            }
            InvokeRepeating(nameof(RegisterServers), 1f, 1f);
            InvokeRepeating(nameof(UpdateScreenState), 1, 1);
            IsInitialized = true;
        }

        public void UpdateStorageCount()
        {
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
    }
}