using System;
using System.Collections;
using System.Collections.Generic;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.MiniFountainFilter.Buildables;
using FCS_HomeSolutions.MiniFountainFilter.Managers;
using FCS_HomeSolutions.ModManagers;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace FCS_HomeSolutions.MiniFountainFilter.Mono
{
    internal class MiniFountainFilterController : FcsDevice,IFCSSave<SaveData>
    {
        private bool IsInitialized;
        private int _isRunning;
        private bool _isOperational;
        private bool _isInSub;
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private MiniFountainFilterDataEntry _data;
        private MiniFountainFilterDataEntry _saveData;
        private ParticleSystem _xBubbles;
        private Dictionary<string, FcsDevice> _registeredDevices;
        internal MFFDisplayManager DisplayManager { get; private set; }
        internal AnimationManager AnimationManager { get; private set; }
        internal MFFStorageManager StorageManager { get; private set; }
        internal PowerManager PowerManager { get; private set; }
        internal AudioManager AudioManager { get; private set; }
        internal TankManager TankManager { get; private set; }
        public PlayerManager PlayerManager { get; private set; }
        public Action OnMonoUpdate { get; set; }


        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.MiniFountainFilterTabID, Mod.ModName);
        }

        public override Vector3 GetPosition()
        {
            return transform.position;
        }

        private void OnEnable()
        {
            _registeredDevices = FCSAlterraHubService.PublicAPI.GetRegisteredDevices();

            if (!_runStartUpOnEnable) return;
            
            if (!IsInitialized)
            {
                Initialize();
            }

            if (_data == null)
            {
                ReadySaveData();
            }

            if (_fromSave)
            {
                TankManager.SetTankLevel(_data.TankLevel);
                _colorManager.ChangeColor(_data.Body.Vector4ToColor());
                StorageManager.NumberOfBottles = _data.ContainerAmount;
                _isInSub = _data.IsInSub;
                QuickLogger.Info($"Loaded {Mod.MiniFountainFilterFriendly}");
                _fromSave = false;
            }
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _data = Mod.GetMiniFountainFilterSaveData(id);
        }

        private void Update()
        {
            OnMonoUpdate?.Invoke();
            PowerManager?.ConsumePower();
            StorageManager?.AttemptSpawnBottle();
            UpdateSystem();
        }

        public override void Initialize()
        {
            _isRunning = Animator.StringToHash("IsRunning");
            _isInSub = Player.main.IsInSubmarine();

            if (StorageManager == null)
            {
                StorageManager = new MFFStorageManager();
                StorageManager.Initialize(this);
            }

            if (PowerManager == null)
            {
                PowerManager = new PowerManager();
                PowerManager.Initialize(this);
                StartCoroutine(UpdatePowerState());
            }

            AnimationManager = gameObject.GetComponent<AnimationManager>();

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, ModelPrefab.BodyMaterial);
            }

            if (TankManager == null)
            {
                TankManager = new TankManager();
                TankManager.Initialize(this);
            }

            if (PlayerManager == null)
            {
                PlayerManager = new PlayerManager();
                PlayerManager.Initialize();
            }

            if (AudioManager == null)
            {
                AudioManager = new AudioManager(gameObject.GetComponent<FMOD_CustomLoopingEmitter>());
            }

            if (DisplayManager == null)
            {
                DisplayManager = gameObject.AddComponent<MFFDisplayManager>();
                DisplayManager.Setup(this);
            }

            _xBubbles = GameObjectHelpers.FindGameObject(gameObject, "xBubbles").GetComponent<ParticleSystem>();

            IsInitialized = true;
            
            QuickLogger.Debug($"Initialized");
        }
        
        private IEnumerator UpdatePowerState()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);
                PowerManager.UpdatePowerState();
            }
        }

        public override bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;

            if (StorageManager == null)
            {
                return true;
            }

            if (!StorageManager.CanDeconstruct())
            {
                reason = MiniFountainFilterBuildable.UnitNotEmpty();
                return false;
            };

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
      
        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {Mod.MiniFountainFilterFriendly}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {Mod.MiniFountainFilterFriendly}");
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
                _saveData = new MiniFountainFilterDataEntry();
            }
            _saveData.Id = id;
            _saveData.Body = _colorManager.GetColor().ColorToVector4();
            _saveData.TankLevel = TankManager.GetTankLevel();
            _saveData.ContainerAmount = StorageManager.NumberOfBottles;
            _saveData.IsInSub = _isInSub;
            newSaveData.MiniFountainFilterEntries.Add(_saveData);
        }

        public override bool IsUnderWater()
        {
            if (_isInSub) return true;
            if (Manager.DeviceBuilt("BUU", out var device))
            {
                foreach (KeyValuePair<string, FcsDevice> pair in device)
                {
                    if(pair.Value != null && pair.Value.IsUnderWater())
                        return true;
                }
            }
            return base.IsUnderWater();
        }
        
        private void UpdateSystem()
        {
            if (!IsInitialized) return;

            if (!IsConstructed || PowerManager.GetPowerState() != FCSPowerStates.Powered && _isOperational)
            {
                DisplayManager.TurnOffDisplay();
                _isOperational = false;
            }
            
            if (!IsUnderWater() && _isOperational)
            {
                DisplayManager.AboveWaterMessage();
                _isOperational = false;
            }

            if (PowerManager.GetPowerState() == FCSPowerStates.Powered && IsUnderWater() && !_isOperational)
            {
                AudioManager.PlayMachineAudio();
                AnimationManager.SetBoolHash(_isRunning, true);
                DisplayManager.PowerOnDisplay();
                _isOperational = true;
            }

            if (!_isOperational)
            {
                AudioManager?.StopMachineAudio();
                AnimationManager.SetBoolHash(_isRunning, false);
            }

            UpdateBubbles();
        }

        private void UpdateBubbles()
        {
            if (_isOperational && !_xBubbles.isPlaying)
            {
                _xBubbles.Play();
            }
            else if (!_isOperational && _xBubbles.isPlaying)
            {
                _xBubbles.Stop();
            }

        }

        internal bool GetIsOperational()
        {
            return _isOperational;
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }
    }
}
