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
        private float _targetPos;
        protected const float Speed = 3f;
        protected GameObject Tray;
        protected Dictionary<string,DSSSlotController> Slots;
        private Text _storageAmount;
        private List<GameObject> _meters;
        private Image _percentageBar;
        private GameObject _canvas;
        private bool _isVisible;
        protected abstract float OpenPos { get;}
        protected abstract float ClosePos { get; }
        protected Transform SlotsLocation;
        public override bool IsOperational => IsInitialized && IsConstructed;
        public override bool IsRack { get; } = true;
        public override bool IsVisible => _isVisible;

        public bool IsOpen => _targetPos > ClosePos;

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
            MoveTray();
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
                        if (SavedData.IsTrayOpen)
                        {
                            Open();
                        }
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
            _storageAmount = gameObject.GetComponentInChildren<Text>();
            _canvas = gameObject.GetComponentInChildren<Canvas>()?.gameObject;
            Slots = new Dictionary<string,DSSSlotController>();
            _meters = new List<GameObject>();
            _percentageBar = GameObjectHelpers.FindGameObject(gameObject, "Preloader").GetComponent<Image>();
            
            var meters = GameObjectHelpers.FindGameObject(gameObject, "Meters").transform;

            foreach (Transform meter in meters)
            {
                _meters.Add(meter.gameObject);
            }
            int i = 1;

            foreach (Transform slot in SlotsLocation)
            {
                var meter = _meters[i - 1];
                var slotName = $"Slot {i++}";
                var slotController = slot.gameObject.AddComponent<DSSSlotController>();
                slotController.Initialize(slotName, this, meter);
                Slots.Add(slotName, slotController);
            }

            _targetPos = Tray.transform.localPosition.x;

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
                if (controller.Value != null && controller.Value.IsOccupied)
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

        protected abstract void MoveTray();

        private void Open()
        {
            _targetPos = OpenPos;
        }

        private void Close()
        {
            _targetPos = ClosePos;
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }

        public void OnHandHover(GUIHand hand)
        {
            if (!IsConstructed || !IsInitialized) return;
            HandReticle main = HandReticle.main;
            main.SetIcon(HandReticle.IconType.Hand);
            main.SetInteractText(IsOpen ? AuxPatchers.CloseServerRack() : AuxPatchers.OpenServerRack());
        }

        public void OnHandClick(GUIHand hand)
        {
            if (!IsConstructed || !IsInitialized) return;
            if (IsOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        public override void TurnOnDevice()
        {
            _isVisible = true;
            if (!_canvas.activeSelf)
            {
                _canvas.SetActive(true);
            }
        }

        public override void TurnOffDevice()
        {
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