using AE.MiniFountainFilter.Buildable;
using AE.MiniFountainFilter.Configuration;
using AE.MiniFountainFilter.Managers;
using FCSCommon.Utilities;
using System;
using System.Collections;
using FCSCommon.Enums;
using FCSTechFabricator.Enums;
using FCSTechFabricator.Extensions;
using FCSTechFabricator.Managers;
using UnityEngine;
using AnimationManager = AE.MiniFountainFilter.Managers.AnimationManager;
using AudioManager = AE.MiniFountainFilter.Managers.AudioManager;
using PowerManager = AE.MiniFountainFilter.Managers.PowerManager;

namespace AE.MiniFountainFilter.Mono
{
    internal class MiniFountainFilterController : MonoBehaviour, IConstructable, IProtoEventListener
    {
        private bool IsInitialized;
        private SaveDataEntry _saveData;
        private int _isRunning;
        private bool _isOperational;
        private bool _isInSub;
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private SaveDataEntry _data;
        internal ColorManager ColorManager { get; private set; }
        internal MFFDisplayManager DisplayManager { get; private set; }
        internal AnimationManager AnimationManager { get; private set; }
        internal MFFStorageManager StorageManager { get; private set; }
        internal PowerManager PowerManager { get; private set; }
        internal AudioManager AudioManager { get; private set; }
        internal TankManager TankManager { get; private set; }
        internal bool IsConstructed { get; private set; }
        public PlayerManager PlayerManager { get; private set; }
        public Action OnMonoUpdate { get; set; }

        private void OnEnable()
        {
            if (!_runStartUpOnEnable) return;

            if (!IsInitialized)
            {
                Initialize();
            }

            if (_data == null)
            {
                ReadySaveData();
            }

            if (_fromSave && _data != null)
            {
                QuickLogger.Info($"Loading from save: {Mod.FriendlyName}");
                TankManager.SetTankLevel(_data.TankLevel);
                ColorManager.SetColorFromSave(_data.BodyColor.Vector4ToColor());
                StorageManager.NumberOfBottles = _data.ContainerAmount;
                _isInSub = _data.IsInSub;
                QuickLogger.Info($"Loaded {Mod.FriendlyName}");
                _fromSave = false;
            }
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _data = Mod.GetSaveData(id);
        }

        private void Update()
        {
            OnMonoUpdate?.Invoke();
            PowerManager?.ConsumePower();
            StorageManager?.AttemptSpawnBottle();
            UpdateSystem();
        }
        private void Initialize()
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

            if (ColorManager == null)
            {
                ColorManager = new ColorManager();
                ColorManager.Initialize(gameObject, MiniFountainFilterBuildable.BodyMaterial);

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

        public bool CanDeconstruct(out string reason)
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

        public void OnConstructedChanged(bool constructed)
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
      
        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {Mod.FriendlyName}");
                Mod.Save();
                QuickLogger.Info($"Saved {Mod.FriendlyName}");
            }
        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            _fromSave = true;
            //QuickLogger.Info($"Loading {Mod.FriendlyName}");
            //var prefabIdentifier = GetComponent<PrefabIdentifier>();
            //var id = prefabIdentifier?.Id ?? string.Empty;
            //var data = Mod.GetSaveData(id);

            //if (data == null)
            //{
            //    QuickLogger.Info($"No save found for PrefabId {id}");
            //    return;
            //}

            //TankManager.SetTankLevel(data.TankLevel);
            //ColorManager.SetCurrentBodyColor(data.BodyColor.Vector4ToColor());
            //StorageManager.LoadContainer(data.ContainerAmount);
            //_isInSub = data.IsInSub;
            //QuickLogger.Info($"Loaded {Mod.FriendlyName}");
        }

        internal void Save(SaveData saveData)
        {
            var prefabIdentifier = GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier.Id;

            if (_saveData == null)
            {
                _saveData = new SaveDataEntry();
            }
            _saveData.ID = id;
            _saveData.BodyColor = ColorManager.GetColor().ColorToVector4();
            _saveData.TankLevel = TankManager.GetTankLevel();
            _saveData.ContainerAmount = StorageManager.NumberOfBottles;
            _saveData.IsInSub = _isInSub;
            saveData.Entries.Add(_saveData);
        }

        internal bool IsUnderwater()
        {
            if (_isInSub) return true;

            return base.transform.position.y < -1f;
        }

        private void UpdateSystem()
        {
            if (!IsInitialized) return;

            if (!IsConstructed && _isOperational)
            {
                QuickLogger.Debug("Update System: IsConstructed False", true);
                AudioManager?.StopMachineAudio();
                AnimationManager.SetBoolHash(_isRunning, false);
                DisplayManager.PowerOffDisplay();
                _isOperational = false;
                return;
            }

            if (PowerManager.GetPowerState() != FCSPowerStates.Powered || !IsUnderwater() && _isOperational)
            {
                QuickLogger.Debug("Update System: Powered and Underwater Else", true);

                AudioManager.StopMachineAudio();
                AnimationManager.SetBoolHash(_isRunning, false);
                DisplayManager.AboveWaterMessage();
                _isOperational = false;
                return;
            }

            if (PowerManager.GetPowerState() == FCSPowerStates.Powered && IsUnderwater() && !_isOperational)
            {
                QuickLogger.Debug("Update System: Powered and Underwater", true);

                AudioManager.PlayMachineAudio();
                AnimationManager.SetBoolHash(_isRunning, true);
                DisplayManager.PowerOnDisplay();
                _isOperational = true;
            }
        }

        internal bool GetIsOperational()
        {
            return _isOperational;
        }
    }
}
