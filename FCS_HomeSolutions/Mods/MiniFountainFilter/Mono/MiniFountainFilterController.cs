using System;
using System.Collections.Generic;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.MiniFountainFilter.Buildables;
using FCS_HomeSolutions.Mods.MiniFountainFilter.Managers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.MiniFountainFilter.Mono
{
    internal class MiniFountainFilterController : FcsDevice,IFCSSave<SaveData>, IHandTarget
    {
        private bool _isInSub;
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private MiniFountainFilterDataEntry _data;
        private MiniFountainFilterDataEntry _saveData;
        private ParticleSystem _xBubbles;
        private InterfaceInteraction _interactionHelper;
        private float _powerUsage;
        private FMOD_CustomLoopingEmitter _machineSound;
        internal MFFDisplayManager DisplayManager { get; private set; }
        internal MFFStorageManager StorageManager { get; private set; }
        internal TankManager TankManager { get; private set; }
        public PlayerManager PlayerManager { get; private set; }
        public Action OnMonoUpdate { get; set; }
        public override bool IsOperational => Manager != null && IsInitialized && IsConstructed && DisplayManager != null && _xBubbles != null && TankManager != null;


        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, MiniFountainFilterBuildable.MiniFountainFilterTabID, Mod.ModPackID);
        }

        public override Vector3 GetPosition()
        {
            return transform.position;
        }

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

            if (_fromSave)
            {
                TankManager.SetTankLevel(_data.TankLevel);
                _colorManager.LoadTemplate(_data.ColorTemplate);
                StorageManager.NumberOfBottles = _data.ContainerAmount;
                _isInSub = _data.IsInSub;
                QuickLogger.Info($"Loaded {MiniFountainFilterBuildable.MiniFountainFilterFriendly}");
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
            StorageManager?.AttemptSpawnBottle();
        }

        public override void Initialize()
        {
            _isInSub = Player.main.IsInSubmarine();

            if (StorageManager == null)
            {
                StorageManager = new MFFStorageManager();
                StorageManager.Initialize(this);
            }

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol);
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

            if (_machineSound == null)
            {
                _machineSound = FModHelpers.CreateCustomLoopingEmitter(gameObject, "water_filter_loop", "event:/sub/base/water_filter_loop");
            }


            if (DisplayManager == null)
            {
                DisplayManager = gameObject.AddComponent<MFFDisplayManager>();
                DisplayManager.Setup(this);
            }

            _xBubbles = GameObjectHelpers.FindGameObject(gameObject, "xBubbles").GetComponent<ParticleSystem>();
            var canvas = gameObject.GetComponentInChildren<Canvas>();
            _interactionHelper = canvas.gameObject.AddComponent<InterfaceInteraction>();
            InvokeRepeating(nameof(UpdateSystem),1f,1f);
            IsInitialized = true;
            
            QuickLogger.Debug($"Initialized");
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
                QuickLogger.Info($"Saving {MiniFountainFilterBuildable.MiniFountainFilterFriendly}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {MiniFountainFilterBuildable.MiniFountainFilterFriendly}");
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
            _saveData.ColorTemplate = _colorManager.SaveTemplate();
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
            if (!IsOperational || Manager.GetPowerState() == PowerSystem.Status.Offline)
            {
                DisplayManager.TurnOffDisplay();
                ToggleEffectsAndSound(false);
                return;
            }

            if (!IsUnderWater() || TankManager.IsFull())
            {
                DisplayManager.TurnOnDisplay();
                ToggleEffectsAndSound(false);
                return;
            }

            DisplayManager.TurnOnDisplay();
            ToggleEffectsAndSound(true);
        }

        private void ToggleEffectsAndSound(bool isRunning)
        {
            if (_xBubbles == null || _machineSound == null) return;

            if (isRunning)
            {
                if (!_xBubbles.isPlaying) _xBubbles.Play();
                if (!_machineSound._playing) _machineSound.Play();
            }
            else
            {
                if (_xBubbles.isPlaying) _xBubbles?.Stop();
                if (_machineSound._playing) _machineSound.Stop();
            }
        }

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            return _colorManager.ChangeColor(template);
        }

        public override void OnHandHover(GUIHand hand)
        {
            if (!IsInitialized || !IsConstructed || _interactionHelper.IsInRange) return;
            base.OnHandHover(hand);

            var data = new[]
            {
                AlterraHub.PowerPerMinute(_powerUsage)
            };
            data.HandHoverPDAHelperEx(GetTechType());
        }

        public void OnHandClick(GUIHand hand)
        {
            
        }

        public override float GetPowerUsage()
        {
            if (!IsOperational || TankManager.IsFull() || !IsUnderWater())
            {
                _powerUsage = 0f;
            }
            else
            {
                _powerUsage = Mathf.Round(QPatch.Configuration.MiniFountainFilterEnergyPerSec * 60) / 10f;
            }
            return  _powerUsage;
        }
    }
}
