using System;
using System.Collections.Generic;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_StorageSolutions.Configuration;
using FCS_StorageSolutions.Helpers;
using FCS_StorageSolutions.Mods.AlterraStorage.Buildable;
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
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.DSSTabID, Mod.ModName);
            _display.Setup(this);
            Manager.OnPowerStateChanged += OnPowerStateChanged;
            Manager.OnBreakerStateChanged += OnBreakerStateChanged;
            if (Manager.GetBreakerState())
            {
                _display.HibernateDisplay();
            }

            UpdateScreenState();
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
            _saveData = Mod.GetDSSTerminalSaveData(id);
        }

        public override void Initialize()
        {
            if (_colorManager == null)
            {
                _colorManager = gameObject.EnsureComponent<ColorManager>();
                _colorManager.Initialize(gameObject, ModelPrefab.BodyMaterial, ModelPrefab.SecondaryMaterial);
            }

            if (_display == null)
            {
                _display = gameObject.EnsureComponent<DSSTerminalDisplayManager>();
            }
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
            _saveData.Body = _colorManager.GetColor().ColorToVector4();
            _saveData.SecondaryBody = _colorManager.GetSecondaryColor().ColorToVector4();



            newSaveData.DSSTerminalDataEntries.Add(_saveData);
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
    }
}