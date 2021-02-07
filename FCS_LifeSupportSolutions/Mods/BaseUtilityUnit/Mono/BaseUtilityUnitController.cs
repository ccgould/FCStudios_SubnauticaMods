using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.OreConsumer;
using FCS_AlterraHub.Registration;
using FCS_LifeSupportSolutions.Buildable;
using FCS_LifeSupportSolutions.Configuration;
using FCSCommon.Controllers;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using System;
using FCS_AlterraHub.Enumerators;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_LifeSupportSolutions.Mods.BaseUtilityUnit.Mono
{
    internal class BaseUtilityUnitController : FcsDevice, IFCSSave<SaveData>
    {
        private bool _runStartUpOnEnable;
        private bool _isFromSave;
        private BaseUtilityEntry _savedData;
        private Text _percent;
        private Image _percentBar;
        private ParticleSystem[] _bubbles;
        private AnimationManager _animationManager;
        private int _isRunningHash;
        private MotorHandler _fanMotor;
        private AudioManager _audioManager;
        private bool _prevPowerState;
        private Canvas _canvas;
        public OxygenManager OxygenManager { get; set; }
        public override bool IsOperational => IsConstructed && Manager != null && IsInitialized;


        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.BaseUtilityUnitTabID, Mod.ModName);
            if (Manager == null)
            {
                TurnOffDevice();
            }
        }

        public override Vector3 GetPosition()
        {
            return transform.position;
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

                    OxygenManager.SetO2Level(_savedData.O2Level);
                    _colorManager.ChangeColor(_savedData.Body.Vector4ToColor());
                    _colorManager.ChangeColor(_savedData.SecondaryBody.Vector4ToColor(), ColorTargetMode.Secondary);
                }

                _runStartUpOnEnable = false;
            }
        }

        public override void Initialize()
        {
            _isRunningHash = Animator.StringToHash("IsRunning");
            _canvas = gameObject.GetComponentInChildren<Canvas>();
            _fanMotor = GameObjectHelpers.FindGameObject(gameObject, "FanRotor").AddComponent<MotorHandler>();
            _fanMotor.Initialize(200);

            _bubbles = gameObject.GetComponentsInChildren<ParticleSystem>();

            if (_audioManager == null)
            {
                _audioManager = new AudioManager(gameObject.EnsureComponent<FMOD_CustomLoopingEmitter>());
            }


            if (_animationManager == null)
            {
                _animationManager = gameObject.AddComponent<AnimationManager>();
            }
            
            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, ModelPrefab.BodyMaterial, ModelPrefab.SecondaryMaterial);
            }

            if (OxygenManager == null)
            {
                OxygenManager = gameObject.AddComponent<OxygenManager>();
                OxygenManager.Initialize(this);
                OxygenManager.OnOxygenUpdated += (amount, percentage) =>
                {
                    _percent.text = $"{percentage:P0}";
                    _percentBar.fillAmount = percentage;
                };
            }

            InvokeRepeating(nameof(UpdateAnimation),1f,1f);

            var takeOxygenBtnObj = InterfaceHelpers.FindGameObject(gameObject, "Button");
            var takeButton = InterfaceHelpers.CreateButton(takeOxygenBtnObj, "TakeBTN", InterfaceButtonMode.Background,
                GivePlayerOxygen, new Color(0.2784314f, 0.2784314f, 0.2784314f), new Color(0, 1, 1, 1), 5,
                AuxPatchers.TakeOxygen(), AuxPatchers.TakeOxygenDesc());
            takeButton.InteractionRequirement = InteractionRequirement.IsOutSide;
            
            _percent = InterfaceHelpers.FindGameObject(gameObject, "percentage").GetComponent<Text>();
            _percentBar = InterfaceHelpers.FindGameObject(gameObject, "PreLoader_Bar_Front").GetComponent<Image>();

            MaterialHelpers.ChangeEmissionStrength(ModelPrefab.EmissiveControllerMaterial,gameObject,5f);

            IsInitialized = true;
        }

        private void UpdateAnimation()
        {
            var currentState = Manager.HasEnoughPower(GetPowerUsage());
            if (_prevPowerState == currentState) return;
            _prevPowerState = currentState;

            foreach (ParticleSystem bubble in _bubbles)
            {
                if (currentState)
                {
                    bubble.Play();
                    _fanMotor.StartMotor();
                    _audioManager.PlayMachineAudio();
                }
                else
                {
                    bubble.Stop();
                    _fanMotor.StopMotor();
                    _audioManager.StopMachineAudio();
                }
            }

            _animationManager.SetBoolHash(_isRunningHash, currentState);
        }

        private void GivePlayerOxygen(string arg1, object arg2)
        {
            OxygenManager.GivePlayerO2();
        }

        public override float GetPowerUsage()
        {
            return 0.5066666666666667f;
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

            if (_savedData == null)
            {
                ReadySaveData();
            }

            _isFromSave = true;
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
                _savedData = new BaseUtilityEntry();
            }

            _savedData.Id = GetPrefabID();
            _savedData.O2Level = OxygenManager.GetO2Level();
            _savedData.Body = _colorManager.GetColor().ColorToVector4();
            _savedData.SecondaryBody = _colorManager.GetSecondaryColor().ColorToVector4();
            QuickLogger.Debug($"Saving ID {_savedData.Id}");
            newSaveData.BaseUtilityUnitEntries.Add(_savedData);
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetBaseUtilityUnitSaveData(GetPrefabID());
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }

        public override void TurnOffDevice()
        {
            _canvas.gameObject.SetActive(false);
        }
    }
}
