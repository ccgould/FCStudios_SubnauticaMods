using System;
using System.Text;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Assets;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.LedLights.Mono
{
    internal class LedLightController : FcsDevice, IFCSSave<SaveData>, IHandTarget
    {
        private bool _runStartUpOnEnable;
        private Light _light;
        private LedLightDataEntry _savedData;
        private bool _isFromSave;
        private Constructable _buildable;
        private bool _nightSensor;
        public override bool IsInitialized { get; set; }

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, "LED", Mod.ModPackID);
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


                    if (_savedData != null)
                    {
                        _colorManager.ChangeColor(_savedData.Lum.Vector4ToColor(), ColorTargetMode.Emission);
                        transform.rotation = _savedData.Rotation.Vec4ToQuaternion();
                        _light.color = _savedData.Lum.Vector4ToColor();
                        if (Mod.GetSaveData().SaveVersion >= 1.1)
                        {
                            _light.intensity = _savedData.Intensity < 0.5f ? 0.5f : _savedData.Intensity;
                            if (_savedData.LightState)
                            {
                                TurnOnDevice();
                            }
                            else
                            {
                                TurnOffDevice();
                            }
                        }
                        _isFromSave = false;

                        _nightSensor = _savedData.NightSensor;
                    }
                }
                _runStartUpOnEnable = false;
            }
        }

        public override void Initialize()
        {
            QuickLogger.Info("Initializing", true);

            if (_buildable == null)
            {
                _buildable = gameObject.GetComponent<Constructable>();
            }

            if (_colorManager == null)
            {
                QuickLogger.Info($"Creating Color Component", true);
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol, AlterraHub.BaseSecondaryCol, AlterraHub.BaseLightsEmissiveController);
                MaterialHelpers.ChangeEmissionStrength(AlterraHub.BaseLightsEmissiveController, gameObject, _buildable.isInside ? 2.5f : 1.8f );
            }

            if (_light == null)
            {
                _light = gameObject.GetComponentInChildren<Light>();
                TurnOnDevice();
            }

            IsInitialized = true;
            QuickLogger.Info("Initialized", true);
        }

        private void Update()
        {
            if (_nightSensor)
            {
                if (DayNightCycle.main.IsDay())
                {
                    TurnOffDevice();
                }
                else
                {
                    TurnOnDevice();
                }
            }
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer)
        {
            if (!IsInitialized || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new LedLightDataEntry();
            }
            
            _savedData.Id = GetPrefabID();
            _savedData.Lum = _colorManager.GetLumColor().ColorToVector4();
            _savedData.Rotation = transform.rotation.QuaternionToVec4();
            _savedData.Intensity = _light.intensity;
            _savedData.NightSensor = _nightSensor;
            _savedData.LightState = _light.enabled;
            QuickLogger.Debug($"Saving ID {_savedData.Id}");
            newSaveData.LedLightDataEntries.Add(_savedData);
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

        private void RotateBounds(bool isVertical)
        {
            Vector3 size;
            Vector3 center;

            if (isVertical)
            {
                size = new Vector3(0.1145213f, 1.667081f, 0.1145213f);
                center = new Vector3(0f, 1.09443f, 0f);
            }
            else
            {
                size = new Vector3(1.653859f, 0.1378375f, 0.07659748f);
                center = new Vector3(0f, 0f, 0.09088595f);
            }

            GameObjectHelpers.SetConstructableBounds(gameObject, size, center);
        }


        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoDeserialize");

            if (_savedData == null)
            {
                ReadySaveData();
            }

            _isFromSave = true;
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

        public override bool CanDeconstruct(out string reason)
        {
            reason = String.Empty;
            return true;
        }

        public override void  OnHandHover(GUIHand hand)
        {
            if (!IsInitialized || !IsConstructed) return;

            base.OnHandHover(hand);
            
            if (_buildable.allowedOnWall)
            {
                var data = new[]
                {
                    AuxPatchers.ClickToRotate(),
                    AuxPatchers.PressToToggleLightFormat(GameInput.GetBindingName(GameInput.Button.AltTool, GameInput.BindingSet.Primary),
                        QPatch.Configuration.LEDLightBackwardKeyCode.ToString(),
                        QPatch.Configuration.LEDLightForwardKeyCode.ToString(),
                        QPatch.Configuration.LEDLightNightSensorToggleKeyCode.ToString(),
                        _light.intensity,
                        _nightSensor ? "Enabled":"Disabled")
                };
                data.HandHoverPDAHelperEx(GetTechType(),HandReticle.IconType.Hand);
            }
            else
            {
                var data = new[]
                {
                    AuxPatchers.PressToToggleLightFormat(GameInput.GetBindingName(GameInput.Button.AltTool, GameInput.BindingSet.Primary),
                        QPatch.Configuration.LEDLightBackwardKeyCode.ToString(),
                        QPatch.Configuration.LEDLightForwardKeyCode.ToString(),
                        QPatch.Configuration.LEDLightNightSensorToggleKeyCode.ToString(),
                        _light.intensity,
                        _nightSensor ? "Enabled" : "Disabled")
                };
                data.HandHoverPDAHelperEx(GetTechType(),HandReticle.IconType.Hand);
            }

            if (GameInput.GetButtonDown(GameInput.Button.AltTool))
            {
                if (_nightSensor)
                {
                    QuickLogger.ModMessage("Please disable Night Sensor.");
                    return;
                }

                if (_light.enabled)
                {
                    TurnOffDevice();
                }
                else
                {
                    TurnOnDevice();
                }

            }

            if (Input.GetKeyDown(QPatch.Configuration.LEDLightBackwardKeyCode))
            {
                if(Mathf.Approximately(_light.intensity,0.5f)) return;

                if (_light.intensity < 0.5f)
                {
                    _light.intensity = 0.5f;
                }

                _light.intensity -= 0.1f;
            }
            
            if (Input.GetKeyDown(QPatch.Configuration.LEDLightForwardKeyCode))
            {
                if (Mathf.Approximately(_light.intensity, 1.5f)) return;

                if (_light.intensity > 1.5f)
                {
                    _light.intensity = 1.5f;
                }

                _light.intensity += 0.1f;
            }


            if (Input.GetKeyDown(QPatch.Configuration.LEDLightNightSensorToggleKeyCode))
            {
                _nightSensor ^= true;
            }
        }

        public override void TurnOnDevice()
        {
            if (_light != null)
            {
                _light.enabled = true;
                MaterialHelpers.ChangeEmissionStrength(AlterraHub.BaseLightsEmissiveController, gameObject, _buildable.isInside ? 2.5f : 1.8f);
            }
        }

        public override void TurnOffDevice()
        {
            if (_light != null)
            {
                _light.enabled = false;
                MaterialHelpers.ChangeEmissionStrength(AlterraHub.BaseLightsEmissiveController, gameObject, 0f);
            }
        }

        public void OnHandClick(GUIHand hand)
        {
            if (!_buildable.allowedOnWall || !IsConstructed) return;

            if (transform.eulerAngles.z == 90f)
            {
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0f);
                RotateBounds(true);
            }
            else
            {
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 90f);
                RotateBounds(false);
            }
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            var result = _colorManager.ChangeColor(color, mode);

            if (result && mode == ColorTargetMode.Emission)
            {
                if (_light != null)
                {
                    _light.color = color;
                }
            }

            return result;

        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetLedLightEntrySaveData(GetPrefabID());
        }
    }
}
