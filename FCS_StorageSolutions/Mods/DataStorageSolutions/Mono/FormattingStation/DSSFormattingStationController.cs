using System.Collections.Generic;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_StorageSolutions.Configuration;
using FCS_StorageSolutions.Mods.AlterraStorage.Buildable;
using FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Server;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.FormattingStation
{
    internal class DSSFormattingStationController : FcsDevice,IFCSSave<SaveData>,IFCSDumpContainer
    {
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private DSSFormattingStationDataEntry _saveData;
        private DSSServerController _mountedServer;
        private DSSFormattingStationDisplay _displayManager;
        private FCSStorage _storageManager;
        private Transform _slot;
        public override bool IsOperational => IsInitialized && IsConstructed;
        private bool IsMounted => _mountedServer != null;

        public override float GetPowerUsage()
        {
            if (Manager == null || !IsConstructed || Manager.GetBreakerState()) return 0f;
            return 0.01f + (IsMounted ? 0.01f : 0f);
        }

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.DSSTabID, Mod.ModName);
            _storageManager.CleanUpDuplicatedStorageNoneRoutine();
            Manager.OnPowerStateChanged += OnPowerStateChanged;
            Manager.OnBreakerStateChanged += OnBreakerStateChanged;
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
            if (Manager.GetBreakerState() || Manager.GetPowerState() != PowerSystem.Status.Normal)
            {
                _displayManager?.TurnOffDisplay();
            }
            else
            {
                _displayManager?.TurnOnDisplay();
            }
        }


        private void OnEnable()
        {
            if (!_runStartUpOnEnable) return;

            if (!IsInitialized)
            {
                Initialize();
            }

            if (_saveData == null)
            {
                ReadySaveData();
            }

            if (_fromSave)
            {
                _colorManager.ChangeColor(_saveData.Body.Vector4ToColor());
                _colorManager.ChangeColor(_saveData.SecondaryBody.Vector4ToColor(), ColorTargetMode.Secondary);
                _fromSave = false;
            }
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _saveData = Mod.GetDSSFormattingStationSaveData(id);
        }

        public override void Initialize()
        {
            _slot = gameObject.FindChild("Slot")?.transform;

            if (_slot != null)
            {
                var trigger = _slot.gameObject.AddComponent<SlotTrigger>();
                trigger.Initialize(this);
            }
            

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, ModelPrefab.BodyMaterial, ModelPrefab.SecondaryMaterial);
            }

            if (_displayManager == null)
            {
                _displayManager = gameObject.AddComponent<DSSFormattingStationDisplay>();
                _displayManager.Setup(this);
            }

            if (_storageManager == null)
            {
                _storageManager = gameObject.AddComponent<FCSStorage>();
                _storageManager.Initialize(1,_slot?.gameObject);
            }

            IsInitialized = true;

            QuickLogger.Debug($"Initialized");
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {Mod.DSSFormattingStationFriendlyName}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {Mod.DSSFormattingStationFriendlyName}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            _fromSave = true;

            if (_saveData == null)
            {
                ReadySaveData();
            }

            Initialize();

            _storageManager.RestoreItems(serializer, _saveData.Bytes);
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            var prefabIdentifier = GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier.Id;
            if (_saveData == null)
            {
                _saveData = new DSSFormattingStationDataEntry();
            }
            _saveData.ID = id;
            _saveData.Body = _colorManager.GetColor().ColorToVector4();
            _saveData.SecondaryBody = _colorManager.GetSecondaryColor().ColorToVector4();
            _saveData.Bytes = _storageManager.Save(serializer);
            newSaveData.DSSFormattingStationDataEntries.Add(_saveData);
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }

        public override bool CanDeconstruct(out string reason)
        {
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
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            QuickLogger.Debug("Checking if allowed to add");
            return pickupable.GetTechType() == Mod.GetDSSServerTechType();
        }

        public override bool AddItemToContainer(InventoryItem item)
        {
            QuickLogger.Debug($"Adding {item.item.GetTechType()}");
            _mountedServer = item.item.gameObject.GetComponent<DSSServerController>();
            _storageManager.AddItem(item);
            _mountedServer.DockServer(Manager,_slot);
            _displayManager.GoToPage(FormattingStationPages.Home);
            _displayManager.AddFilterController.ResetFilters(_mountedServer?.GetFilters());
            _displayManager.UpdateFilters();
            return _mountedServer != null;
        }

        public void DockServer(DSSServerController serverController)
        {
            QuickLogger.Debug($"Trying to mount Server {serverController.UnitID}");
            _mountedServer = serverController;
            _mountedServer.IsBeingFormatted = true;
            serverController.DockServer(Manager,_slot);
            _displayManager.GoToPage(FormattingStationPages.Home);
            _displayManager.AddFilterController.ResetFilters(_mountedServer?.GetFilters());
            _displayManager.UpdateFilters();
        }

        public void UnDockServer()
        {
            _mountedServer = null;
            PlayerInteractionHelper.GivePlayerItem(
                _storageManager.ItemsContainer.RemoveItem(Mod.GetDSSServerTechType()));
        }

        public bool IsServerMounted()
        {
            return _mountedServer != null;
        }

        public void AddFilter(Filter filter)
        {
            _mountedServer?.AddFilter(filter);
        }

        public HashSet<Filter> GetFilters()
        {
            return _mountedServer?.GetFilters();
        }

        public void RemoveFilter(Filter filter)
        {
            _mountedServer.RemoveFilter(filter);
        }
    }

    internal enum FormattingStationPages
    {
        Home,
        AddServer,
        Filter
    }

    internal class SlotTrigger : MonoBehaviour
    {
        private DSSFormattingStationController _mono;

        internal void Initialize(DSSFormattingStationController mono)
        {
            _mono = mono;
        }

        private void OnTriggerStay(Collider collider)
        {
            var serverController = collider.GetComponentInParent<DSSServerController>();
            if (_mono != null && serverController != null && !_mono.IsServerMounted())
            {
                QuickLogger.Debug($"OnTriggerStay: {serverController}");
                _mono.DockServer(serverController);
            }
        }

        private void OnTriggerExit(Collider collider)
        {
            _mono.UnDockServer();
        }
    }
}
