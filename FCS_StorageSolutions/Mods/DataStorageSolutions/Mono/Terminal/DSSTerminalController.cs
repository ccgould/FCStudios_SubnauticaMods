using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_StorageSolutions.Configuration;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal
{
    internal class DSSTerminalController : FcsDevice, IFCSSave<SaveData>
    {
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private DSSTerminalDataEntry _saveData;
        private DSSTerminalDisplayManager _display;
        public override bool IsOperational => IsInitialized && IsConstructed;
        public BulkMultipliers BulkMultiplier { get; set; } = BulkMultipliers.TimesOne;

        public override float GetPowerUsage()
        {
            if (Manager == null || Manager.GetBreakerState() || !IsConstructed) return 0f;

            return 0.01f;
        }

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.DSSTerminalTabID, Mod.ModPackID);
            _display.Setup(this);
            Manager.OnPowerStateChanged += OnPowerStateChanged;
            Manager.OnBreakerStateChanged += OnBreakerStateChanged;
            if (Manager.GetBreakerState())
            {
                _display?.HibernateDisplay();
            }

            UpdateScreenState();
            _display?.Refresh();
        }

        public override void OnDestroy()
        {
            if (Manager != null)
            {
                Manager.OnPowerStateChanged -= OnPowerStateChanged;
                Manager.OnBreakerStateChanged -= OnBreakerStateChanged;
            }
            base.OnDestroy();
        }

        private void OnBreakerStateChanged(bool isTripped)
        {
            if (!isTripped && Manager.GetPowerState() != PowerSystem.Status.Offline)
            {
                _display.PowerOnDisplay();
            }
            else
            {
                _display.HibernateDisplay();
            }
        }

        private void OnPowerStateChanged(PowerSystem.Status obj)
        {
            UpdateScreenState();
        }

        private void UpdateScreenState()
        {
            if (Manager == null) return;

            if (Manager.GetPowerState() == PowerSystem.Status.Offline)
            {
                _display?.TurnOffDisplay();
                return;
            }
            _display?.TurnOnDisplay();
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
                _colorManager.LoadTemplate(_saveData.ColorTemplate);
                _fromSave = false;
            }
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _saveData = Mod.GetDSSTerminalSaveData(id);
        }

        public override void Initialize()
        {
            if (_colorManager == null)
            {
                _colorManager = gameObject.EnsureComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol, AlterraHub.BaseSecondaryCol);
            }

            if (_display == null)
            {
                _display = gameObject.EnsureComponent<DSSTerminalDisplayManager>();
            }

            IPCMessage += s =>
            {
                QuickLogger.Debug($"[Terminal - {UnitID}] IPCMessage: {s}",true);

                if (s.Equals("ItemUpdateDisplay"))
                {
                    _display.Refresh();
                }

                //if (s.Equals("DeviceBuiltUpdate"))
                //{
                //    _display.Refresh();
                //    _display.RefreshCraftingGrid();
                //}

                //if (s.Equals("RefreshCraftingGrid"))
                //{
                //    _display.RefreshCraftingGrid();
                //}
            };
            MaterialHelpers.ChangeEmissionColor(string.Empty, gameObject, Color.cyan);

            InvokeRepeating(nameof(UpdateScreenState), 1, 1);
            IsInitialized = true;

            QuickLogger.Debug($"Initialized");
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {Mod.DSSTerminalFriendlyName}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {Mod.DSSTerminalFriendlyName}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            _fromSave = true;
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            var prefabIdentifier = GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier.Id;

            if (_saveData == null)
            {
                _saveData = new DSSTerminalDataEntry();
            }
            _saveData.ID = id;
            _saveData.ColorTemplate = _colorManager.SaveTemplate();

            newSaveData.DSSTerminalDataEntries.Add(_saveData);
        }

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            return _colorManager.ChangeColor(template);
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

        public void ShowMessage(string message)
        {
            _display.ShowMessage(message);
        }
    }
}