using System;
using System.IO;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_EnergySolutions.Buildable;
using FCS_EnergySolutions.Configuration;
using FCSCommon.Components;
using FCSCommon.Enums;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Json.ExtensionMethods;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCS_EnergySolutions.PowerStorage.Mono
{
    internal class PowerStorageController : FcsDevice, IFCSSave<SaveData>, IHandTarget
    {
        private bool _runStartUpOnEnable;
        private bool _isFromSave;
        private PowerStorageDataEntry _savedData;

        internal PowerCellCharger PowercellCharger { get; private set; }
        private PowerSupply _powercellSupply;
        private FCSToggleButton _chargeModeToggle;
        private FCSToggleButton _autoModeToggle;
        private FCSToggleButton _disChargeModeToggle;
        private InterfaceInteration _interactionChecker;
        private PowerChargerMode _mode;
        private ProtobufSerializer _serializer;
        internal Image PowerTotalMeterRing { get; set; }
        internal Text PowerTotalMeterPercent { get; set; }
        internal Text PowerTotalMeterTotal { get; set; }

        private void Update()
        {
            if (DayNightCycle.main.deltaTime == 0)
            {
                return;
            }

            CheckMode();
        }

        public override Vector3 GetPosition()
        {
            return transform.position;
        }

        private void CheckMode()
        {
            if(Manager?.Habitat == null)return;

            switch (_mode)
            {
                case PowerChargerMode.ChargeMode:
                    _powercellSupply.SetAllowedToCharge(true);
                    ChangeStatusLights(true);
                    break;
                case PowerChargerMode.DischargeMode:
                    _powercellSupply.SetAllowedToCharge(false);
                    break;
                case PowerChargerMode.Auto:
                    _powercellSupply.SetAllowedToCharge(Manager.GetPower() - _powercellSupply.GetPower() > 25);
                    break;
            }
        }

        private void ChangeStatusLights(bool value)
        {
            _colorManager.ChangeColor(value ? Color.cyan : Color.yellow, ColorTargetMode.Emission);
        }

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.PowerStorageTabID, Mod.ModName);
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
                    if (_savedData == null)
                    {
                        ReadySaveData();
                    }

                    _colorManager.ChangeColor(_savedData.Body.Vector4ToColor());
                    _colorManager.ChangeColor(_savedData.SecondaryBody.Vector4ToColor(), ColorTargetMode.Secondary);

                    switch (_savedData.Mode)
                    {
                        case PowerChargerMode.ChargeMode:
                            _chargeModeToggle.OnButtonClick?.Invoke(null,null);
                            _chargeModeToggle?.Select();
                            break;
                        case PowerChargerMode.DischargeMode:
                            _disChargeModeToggle.OnButtonClick?.Invoke(null, null);
                            _disChargeModeToggle?.Select();
                            break;
                        case PowerChargerMode.Auto:
                            _autoModeToggle.OnButtonClick?.Invoke(null, null);
                            _autoModeToggle?.Select();
                            break;
                    }

                    if (_savedData.Data != null)
                    {
                        _powercellSupply.Load(_serializer, _savedData.Data);
                    }
                }

                _runStartUpOnEnable = false;
            }
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetPowerStorageSaveData(GetPrefabID());
        }

        public override void Initialize()
        {
            if (IsInitialized) return;

            PowerTotalMeterRing = GameObjectHelpers.FindGameObject(gameObject, "PSRingFront").GetComponent<Image>();
            PowerTotalMeterPercent = GameObjectHelpers.FindGameObject(gameObject, "Percentage").GetComponent<Text>();
            PowerTotalMeterTotal = GameObjectHelpers.FindGameObject(gameObject, "BatteryTotal").GetComponent<Text>();

            _interactionChecker = gameObject.GetComponentInChildren<Canvas>().gameObject.AddComponent<InterfaceInteration>();

            var chargeMode = GameObjectHelpers.FindGameObject(gameObject, "ChargeMode");
            _chargeModeToggle = chargeMode.gameObject.AddComponent<FCSToggleButton>();
            _chargeModeToggle.ButtonMode = InterfaceButtonMode.Background;
            _chargeModeToggle.STARTING_COLOR = Color.gray;
            _chargeModeToggle.HOVER_COLOR = Color.white;
            _chargeModeToggle.TextLineOne = AuxPatchers.PowerStorageChargeMode();
            _chargeModeToggle.TextLineTwo = AuxPatchers.PowerStorageChargeModeDesc();
            _chargeModeToggle.IsRadial = true;
            _chargeModeToggle.BtnName = "ChargeMode";
            _chargeModeToggle.OnButtonClick += (s, o) =>
            {
                _mode = PowerChargerMode.ChargeMode;
                _autoModeToggle.DeSelect();
                _disChargeModeToggle.DeSelect();
            };

            var dischargeMode = GameObjectHelpers.FindGameObject(gameObject, "DischargeMode");
            _disChargeModeToggle = dischargeMode.gameObject.AddComponent<FCSToggleButton>();
            _disChargeModeToggle.ButtonMode = InterfaceButtonMode.Background;
            _disChargeModeToggle.STARTING_COLOR = Color.gray;
            _disChargeModeToggle.HOVER_COLOR = Color.white;
            _disChargeModeToggle.TextLineOne = AuxPatchers.PowerStorageDischargeMode();
            _disChargeModeToggle.TextLineTwo = AuxPatchers.PowerStorageDischargeModeDesc();
            _disChargeModeToggle.IsRadial = true;
            _disChargeModeToggle.BtnName = "DischargeMode";
            _disChargeModeToggle.OnButtonClick += (s, o) =>
            {
                _mode = PowerChargerMode.DischargeMode;
                _autoModeToggle.DeSelect();
                _chargeModeToggle.DeSelect();
            };

            var autoMode = GameObjectHelpers.FindGameObject(gameObject, "AutoText");
            _autoModeToggle = autoMode.gameObject.AddComponent<FCSToggleButton>();
            _autoModeToggle.ButtonMode = InterfaceButtonMode.RadialButton;
            _autoModeToggle.TextLineOne = AuxPatchers.PowerStorageAutoMode();
            _autoModeToggle.TextLineTwo = AuxPatchers.PowerStorageAutoModeDesc();
            _autoModeToggle.IsRadial = true;
            _autoModeToggle.BtnName = "AutoMode";
            _autoModeToggle.OnButtonClick += (s, o) =>
            {

                _mode = PowerChargerMode.Auto;
                _chargeModeToggle.DeSelect();
                _disChargeModeToggle.DeSelect();
            };

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, ModelPrefab.BodyMaterial, ModelPrefab.SecondaryMaterial, ModelPrefab.EmissiveControllerMaterial);
            }

            if (PowercellCharger == null)
            {
                PowercellCharger = gameObject.AddComponent<PowerCellCharger>();
            }

            if (_powercellSupply == null)
            {
                _powercellSupply = gameObject.AddComponent<PowerSupply>();
                _powercellSupply.Initialize(this);
            }


            PowercellCharger.Initialize(this, GameObjectHelpers.FindGameObject(gameObject, "meshes").GetChildren(), GameObjectHelpers.FindGameObject(gameObject, "BatteryMeters").GetChildren(), _powercellSupply);

            _autoModeToggle.OnButtonClick.Invoke(null,null);

            IsInitialized = true;
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

            _serializer = serializer;

            if (_savedData == null)
            {
                ReadySaveData();
            }
            
            _isFromSave = true;
        }

        public override bool CanDeconstruct(out string reason)
        {
            if (_powercellSupply != null && _powercellSupply.HasPowercells())
            {
                reason = AuxPatchers.PowerStorageNotEmpty();
                return false;
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

        public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            if (!IsInitialized || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new PowerStorageDataEntry();
            }

            _savedData.Id = GetPrefabID();

            QuickLogger.Debug($"Saving ID {_savedData.Id}", true);
            _savedData.Body = _colorManager.GetColor().ColorToVector4();
            _savedData.SecondaryBody = _colorManager.GetSecondaryColor().ColorToVector4();
            _savedData.Data = _powercellSupply.Save(serializer);
            _savedData.Mode = _mode;
            newSaveData.PowerStorageEntries.Add(_savedData);
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }

        public void OnHandHover(GUIHand hand)
        {
            if (!IsInitialized || !IsConstructed || _interactionChecker == null ||_interactionChecker.IsInRange) return;
            HandReticle main = HandReticle.main;
            main.SetInteractText(AuxPatchers.PowerStorageClickToAddPowercells());
            main.SetIcon(HandReticle.IconType.Hand);
        }

        public void OnHandClick(GUIHand hand)
        {
            if (_interactionChecker.IsInRange) return;
            _powercellSupply.OpenStorage();
        }
    }

    internal enum PowerChargerMode
    {
        ChargeMode = 0,
        DischargeMode = 1,
        Auto = 2
    }

    internal class InterfaceInteration : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public bool IsInRange { get; set; }

        public void OnPointerEnter(PointerEventData eventData)
        {
            IsInRange = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            IsInRange = false;
        }

        private void OnDisable()
        {
            IsInRange = false;
        }
    }
}
