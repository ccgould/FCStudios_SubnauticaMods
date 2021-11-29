using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Objects;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.LedLights.Mono
{
    internal class LedLightController : FcsDevice, IFCSSave<SaveData>, IHandTarget
    {
        private bool _runStartUpOnEnable;
        private List<Light> _light = new();
        private LedLightDataEntry _savedData;
        private bool _isFromSave;
        private Constructable _buildable;
        private bool _nightSensor;
        //private GameObject _fogCone;
        private Transform _rotor;
        private Transform _tiler;
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
                        _colorManager.LoadTemplate(_savedData.ColorTemplate);
                        transform.rotation = _savedData.Rotation.Vec4ToQuaternion();
                        
                        if (_savedData.RotorRot != Vector3.zero && _savedData.TilerRot != Vector3.zero && _rotor != null && _tiler != null)
                        {
                            _rotor.localEulerAngles = _savedData.RotorRot.Vec3ToVector3();
                            _tiler.localEulerAngles = _savedData.TilerRot.Vec3ToVector3();
                        }

                        foreach (var light in _light)
                        {
                            if(light == null) continue; 
                            light.color = _savedData.ColorTemplate.EmissionColor.Vector4ToColor();
                            light.intensity = _savedData.Intensity < 0.5f ? 0.5f : _savedData.Intensity;
                        }

                        if (Mod.GetSaveData().SaveVersion >= 1.1)
                        {
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

            if (!_light.Any())
            {
                _light = gameObject.GetComponentsInChildren<Light>(true).ToList();
                foreach (Light light in _light)
                {
                    var lsq = light.gameObject.EnsureComponent<LightShadowQuality>();
                    lsq.light = light;
                    lsq.qualityPairs = new[]
                    {
                        new LightShadowQuality.ShadowQualityPair
                        {
                            lightShadows = LightShadows.None,
                            qualitySetting = 0
                        },
                        new LightShadowQuality.ShadowQualityPair
                        {
                            lightShadows = LightShadows.Hard,
                            qualitySetting = 2
                        }
                    };


                    var rgL = gameObject.AddComponent<RegistredLightSource>();
                    rgL.hostLight = light;
                }
                TurnOnDevice();
            }

            ////_fogCone = GameObjectHelpers.FindGameObject(gameObject, "floodlight_fog");
            //if(_fogCone != null)
            //    MaterialHelpers.CreateFloodLightCone(_fogCone);

            _rotor = GameObjectHelpers.FindGameObject(gameObject, "rotor")?.transform;
            _tiler = GameObjectHelpers.FindGameObject(gameObject, "tiller")?.transform;

            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);

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
            _savedData.ColorTemplate = _colorManager.SaveTemplate();
            _savedData.Rotation = transform.rotation.QuaternionToVec4();
            _savedData.RotorRot = _rotor?.localEulerAngles.ToVec3() ?? new Vec3();
            _savedData.TilerRot = _tiler?.localEulerAngles.ToVec3() ?? new Vec3();
            _savedData.Intensity = _light[0].intensity;
            _savedData.NightSensor = _nightSensor;
            _savedData.LightState = _light[0].enabled;
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

        public const float RotationSpeedS = 25f;//speed of rotation
        public const float ro1 = 0f;
        public const float ro2 = 180f;
        public const float rot1 = 0f;
        public const float rot2 = 90f;

        public float InputForRotationTil, InputForRotationRot;//storing input in these variable

        private Vector3 _tilterPos = new Vector3(0f, 2.131469f, 0.003314057f);
        private Vector3 _rotPos = new Vector3(0.1214196f,0f, 3.439538e-17f);

        void RotationUpdate(Vector2 axis)
        {
            if (_rotor == null || _tiler == null) return;

            //just taking input
            InputForRotationRot += axis.x * RotationSpeedS * Time.deltaTime;
            InputForRotationTil += axis.y * RotationSpeedS * Time.deltaTime;

            InputForRotationRot = Mathf.Clamp(InputForRotationRot, rot1, rot2);
            _rotor.localEulerAngles = new Vector3(0f, 90.00001f, InputForRotationRot);

            _rotor.localPosition = _rotPos;
            _tiler.localPosition = _tilterPos;

            InputForRotationTil = Mathf.Clamp(InputForRotationTil, ro1, ro2);
            _tiler.localEulerAngles = new Vector3(0f, -90.00001f, InputForRotationTil);
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
            if (hand.IsTool())
            {
                var data = new[]
                {
                    "Please empty hands"
                };
                data.HandHoverPDAHelperEx(GetTechType(), HandReticle.IconType.HandDeny);
                return;
            }
            
            if (_buildable.allowedOnWall)
            {
                var data = new[]
                {
                    AuxPatchers.ClickToRotate(),
                    AuxPatchers.PressToToggleLightFormat(GameInput.GetBindingName(GameInput.Button.AltTool, GameInput.BindingSet.Primary),
                        QPatch.Configuration.LEDLightBackwardKeyCode.ToString(),
                        QPatch.Configuration.LEDLightForwardKeyCode.ToString(),
                        QPatch.Configuration.LEDLightNightSensorToggleKeyCode.ToString(),
                        _light[0].intensity,
                        _nightSensor ? "Enabled":"Disabled")
                }; 
                data.HandHoverPDAHelperEx(GetTechType(),HandReticle.IconType.Hand);
            }
            else if (_rotor != null && _tiler != null)
            {
                var data = new[]
                {
                    AuxPatchers.PressToToggleLightFormat(GameInput.GetBindingName(GameInput.Button.AltTool, GameInput.BindingSet.Primary),
                        QPatch.Configuration.LEDLightBackwardKeyCode.ToString(),
                        QPatch.Configuration.LEDLightForwardKeyCode.ToString(),
                        QPatch.Configuration.LEDLightNightSensorToggleKeyCode.ToString(),
                        _light[0].intensity,
                        _nightSensor ? "Enabled" : "Disabled"),
                    "CTRL + Arrow Keys to adjust light"
                };
                data.HandHoverPDAHelperEx(GetTechType(), HandReticle.IconType.Hand);

            }
            else
            {
                var data = new[]
                {
                    AuxPatchers.PressToToggleLightFormat(GameInput.GetBindingName(GameInput.Button.AltTool, GameInput.BindingSet.Primary),
                        QPatch.Configuration.LEDLightBackwardKeyCode.ToString(),
                        QPatch.Configuration.LEDLightForwardKeyCode.ToString(),
                        QPatch.Configuration.LEDLightNightSensorToggleKeyCode.ToString(),
                        _light[0].intensity,
                        _nightSensor ? "Enabled" : "Disabled")
                };
                data.HandHoverPDAHelperEx(GetTechType(), HandReticle.IconType.Hand);

            }

            if (GameInput.GetButtonDown(GameInput.Button.AltTool))
            {
                if (_nightSensor)
                {
                    QuickLogger.ModMessage("Please disable Night Sensor.");
                    return;
                }

                if (_light[0].enabled)
                {
                    TurnOffDevice();
                }
                else
                {
                    TurnOnDevice();
                }
            }

            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    RotationUpdate(Vector2.left);
                }
                if (Input.GetKey(KeyCode.RightArrow))
                {
                    RotationUpdate(Vector2.right);
                }
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    RotationUpdate(Vector2.up);
                }
                if (Input.GetKey(KeyCode.DownArrow))
                {
                    RotationUpdate(Vector2.down);
                }
            }
            else
            {
                if (Input.GetKeyDown(QPatch.Configuration.LEDLightBackwardKeyCode))
                {
                    if (Mathf.Approximately(_light[0].intensity, 0.5f)) return;

                    if (_light[0].intensity < 0.5f)
                    {
                        ChangeIntensity(0.5f);
                    }

                    ChangeIntensity(_light[0].intensity - 0.1f);
                }

                if (Input.GetKeyDown(QPatch.Configuration.LEDLightForwardKeyCode))
                {
                    if (Mathf.Approximately(_light[0].intensity, 1.5f)) return;

                    if (_light[0].intensity > 1.5f)
                    {
                        ChangeIntensity(1.5f);
                    }

                    ChangeIntensity(_light[0].intensity + 0.1f);
                }
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
                foreach (var light in _light)
                {
                    if (light == null) continue;
                    light.enabled = true;
                }
                //_fogCone?.SetActive(true);
                MaterialHelpers.ChangeEmissionStrength(AlterraHub.BaseLightsEmissiveController, gameObject, _buildable.isInside ? 2.5f : 1.8f);
            }
        }

        public override void TurnOffDevice()
        {
            if (_light != null)
            {
                foreach (var light in _light)
                {
                    if (light == null) continue;
                    light.enabled = false;
                }
                //_fogCone?.SetActive(false);
                MaterialHelpers.ChangeEmissionStrength(AlterraHub.BaseLightsEmissiveController, gameObject, 0f);
            }
        }

        private void ChangeIntensity(float amount)
        {
            foreach (var light in _light)
            {
                if (light == null) continue;
                light.intensity = amount;
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

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            var result = _colorManager.ChangeColor(template);

            if (_light != null)
            {
                foreach (var light in _light)
                {
                    if (light == null) continue;
                    light.color = template.EmissionColor;
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