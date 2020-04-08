using AMMiniMedBay.Buildable;
using AMMiniMedBay.Display;
using AMMiniMedBay.Enumerators;
using AMMiniMedBay.Models;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using System;
using AMMiniMedBay.Configuration;
using FCSCommon.Abstract;
using FCSTechFabricator.Extensions;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace AMMiniMedBay.Mono
{
    internal class AMMiniMedBayController: FCSController
    {
        private GameObject _scanner;
        private float _processTime = 3.0f;
        private Constructable _buildable;
        private float _timeCurrDeltaTime;
        private PrefabIdentifier _prefabId;
        private float _healthPartial;
        private HealingStatus _healingStatus;
        private bool _isHealing;
        private AMMiniMedBayDisplay _display;
        private NitrogenLevel _nitrogenLevel;
        private Color _currentBodyColor = new Color(0.796875f, 0.796875f, 0.796875f, 0.99609375f);
        private float _nitrogenPartial;
        private bool _runStartUpOnEnable;
        private SaveDataEntry _data;

        public override Action OnMonoUpdate { get; set; }
        public override bool IsInitialized { get; set; }

        public override bool IsConstructed => _buildable != null && _buildable.constructed;

        internal AMMiniMedBayAudioManager AudioHandler { get; set; }

        internal AMMiniMedBayContainer Container { get; private set; }

        internal AMMiniMedBayPowerManager PowerManager { get; set; }
        internal int PageHash { get; set; }
        internal AMMiniMedBayAnimationManager AnimationManager { get; set; }

        internal void HealPlayer()
        {
            if (!IsPlayerInTrigger)
            {
                QuickLogger.Info(LanguageHelpers.GetLanguage(AMMiniMedBayBuildable.NotInPositionMessageKey), true);
                return;
            }

            if (_nitrogenLevel.GetNitrogenEnabled())
            {
                _nitrogenPartial = _nitrogenLevel.safeNitrogenDepth / _processTime;
            }

            if (!PowerManager.GetIsPowerAvailable()) return;

            if (Player.main.liveMixin.IsFullHealth()) return;

            Player.main.playerController.SetEnabled(false);
            var remainder = Player.main.liveMixin.maxHealth - Player.main.liveMixin.health;
            _healthPartial = remainder / _processTime;
            PowerManager.SetHasBreakerTripped(false);
            UpdateIsHealing(true);
        }
        
        private void UpdateIsHealing(bool value)
        {
            _isHealing = value;

            if (_isHealing)
            {
                AudioHandler.PlayScanAudio();
            }
            else
            {
                AudioHandler.StopScanAudio();
            }
        }

        private void OnEnable()
        {
            QuickLogger.Debug("OnEnable Activated");

            if (_runStartUpOnEnable)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }

                if (_display != null)
                {
                    _display.Setup(this);
                    _runStartUpOnEnable = false;
                }

                if (_data == null)
                    ReadySaveData();

                if (_data != null)
                {
                    QuickLogger.Debug("Add Kits From Load");
                    Container.NumberOfFirstAids = _data.SCA;

                    QuickLogger.Debug("Set Time To Spawn");
                    Container.SetTimeToSpawn(_data.TTS);
                }
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
            if (_display != null)
            {
                _display.UpdatePlayerHealthPercent(IsPlayerInTrigger ? Mathf.CeilToInt(Player.main.liveMixin.health) : 0);
            }

            OnMonoUpdate?.Invoke();

            if (!_isHealing) return;

            _healingStatus = HealingStatus.Healing;

            QuickLogger.Debug("Healing Player", true);

            _timeCurrDeltaTime += DayNightCycle.main.deltaTime;

            QuickLogger.Debug($"Delta Time: {_timeCurrDeltaTime}");
            if (!(_timeCurrDeltaTime >= 1)) return;

            QuickLogger.Debug("Delta Passed", true);

            _timeCurrDeltaTime = 0.0f;

            var playerHealth = Player.main.liveMixin.health;
            var playerMaxHealth = Player.main.liveMixin.maxHealth;

            _nitrogenLevel.safeNitrogenDepth =
                Mathf.Clamp(_nitrogenLevel.safeNitrogenDepth -= _nitrogenPartial, 0, float.MaxValue);


            if (!Player.main.liveMixin.IsFullHealth())
            {
                QuickLogger.Debug("Added Health", true);
                PowerManager.ConsumePower(PowerManager.EnergyConsumptionPerSecond);
                Player.main.liveMixin.health = Mathf.Clamp(playerHealth + _healthPartial, 0, playerMaxHealth);
                QuickLogger.Debug($"Player Health = {playerHealth}", true);
            }
            else
            {
                ResetMachine();
            }
        }

        private void ResetMachine()
        {
            Player.main.playerController.SetEnabled(true);
            QuickLogger.Debug("Resetting", true);
            _healingStatus = HealingStatus.Idle;
            UpdateIsHealing(false);
            PowerManager.SetHasBreakerTripped(true);
        }

        public override void Initialize()
        {
            _prefabId = GetComponentInParent<PrefabIdentifier>();

            if (_prefabId == null)
            {
                QuickLogger.Error("Prefab Identifier Component was not found");
            }

            if (_buildable == null)
            {
                _buildable = GetComponentInParent<Constructable>() ?? GetComponent<Constructable>();
            }

            if (!FindAllComponents())
            {
                IsInitialized = false;
                throw new MissingComponentException("Failed to find all components");
            }

            if (PowerManager == null)
            {
                PowerManager = gameObject.EnsureComponent<AMMiniMedBayPowerManager>();
            }

            if (PlayerTrigger == null)
            {
                PlayerTrigger = gameObject.FindChild("model").FindChild("Trigger").EnsureComponent<AMMiniMedBayTrigger>();
            }
            

            if (PlayerTrigger != null)
            {
                PlayerTrigger.OnPlayerStay += OnPlayerStay;
                PlayerTrigger.OnPlayerExit += OnPlayerExit;
            }
            else
            {
                QuickLogger.Error("Player Trigger Component was not found");
            }

            if (AnimationManager == null)
            {
                AnimationManager = gameObject.EnsureComponent<AMMiniMedBayAnimationManager>();
            }
            
            if (PowerManager != null)
            {
                PowerManager.Initialize(this);
                PowerManager.OnPowerOutage += OnPowerOutage;
                PowerManager.OnPowerResume += OnPowerResume;

                //Setting to true to prevent power consumption when not in use on load
                PowerManager.SetHasBreakerTripped(true);
            }
            else
            {
                QuickLogger.Error("Power Manager Component was not found");
            }

            PageHash = UnityEngine.Animator.StringToHash("state");

            if (AudioHandler == null)
            {
                AudioHandler = new AMMiniMedBayAudioManager(gameObject.GetComponent<FMOD_CustomLoopingEmitter>());
            }

            if (Container == null)
            {
                Container = new AMMiniMedBayContainer(this);
            }

            if (_display == null)
            {
                _display = gameObject.AddComponent<AMMiniMedBayDisplay>();
            }
            
            if (Player.main.gameObject.GetComponent<NitrogenLevel>() != null)
            {
                _nitrogenLevel = Player.main.gameObject.GetComponent<NitrogenLevel>();

                InvokeRepeating(nameof(UpdateNitrogenDisplay), 1, 0.5f);
            }

            IsInitialized = true;
        }

        private void UpdateNitrogenDisplay()
        {
            if (_nitrogenLevel == null || _display == null) return;

            if (_nitrogenLevel.GetNitrogenEnabled())
            {
                if (IsPlayerInTrigger)
                {
                    _display.UpdatePlayerNitrogen(_nitrogenLevel.safeNitrogenDepth);
                }
                else
                {
                    _display.HidePlayerNitrogen();
                }
            }
        }

        private void OnPlayerExit()
        {
            IsPlayerInTrigger = false;
        }

        private void OnPlayerStay()
        {
            IsPlayerInTrigger = true;
        }

        internal bool IsPlayerInTrigger { get; set; }

        internal AMMiniMedBayTrigger PlayerTrigger { get; set; }

        public override void OnAddItemEvent(InventoryItem item)
        {
            base.OnAddItemEvent(item);
        }

        public override void OnRemoveItemEvent(InventoryItem item)
        {
            base.OnRemoveItemEvent(item);
        }

        private void OnPowerResume()
        {
            QuickLogger.Debug($"In OnPowerResume {_healingStatus}", true);
            //UpdateIsHealing(_healingStatus == HealingStatus.Healing);
        }

        private void OnPowerOutage()
        {
            ResetMachine();
        }

        private bool FindAllComponents()
        {
            QuickLogger.Debug("********************************************** In FindComponents **********************************************");

            _scanner = transform.Find("model")?.gameObject;

            if (_scanner == null)
            {
                QuickLogger.Error($"Scanner not found");
                IsInitialized = false;
                return false;
            }

            return true;
        }

        public override bool CanDeconstruct(out string reason)
        {
            if (Container == null || !IsInitialized)
            {
                reason = string.Empty;
                return true;
            }

            reason = !Container.GetIsEmpty() ? LanguageHelpers.GetLanguage(AMMiniMedBayBuildable.ContainerNotEmptyMessageKey) : string.Empty;
            return Container.GetIsEmpty();

        }

        public override void OnConstructedChanged(bool constructed)
        {
            QuickLogger.Debug("OnConstructedChanged Activated");

            if (constructed)
            {
                QuickLogger.Debug("Constructed", true);

                if (isActiveAndEnabled)
                {
                    if (!IsInitialized)
                    {
                        Initialize();
                    }

                    if (_display != null)
                    {
                        _display.Setup(this);
                        _runStartUpOnEnable = false;
                    }
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        internal void UpdateKitAmount(int containerSlotsFilled)
        {
            _display.ChangeStorageAmount(containerSlotsFilled);
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("OnProtoDeserialize Activated");
            QuickLogger.Debug("// ****************************** Load Data *********************************** //");

            if (_data == null)
            {
                ReadySaveData();
            }

            _currentBodyColor = _data.BodyColor.Vector4ToColor();
            MaterialHelpers.ChangeMaterialColor("AMMiniMedBay_BaseColor", gameObject, _currentBodyColor);

            QuickLogger.Debug("// ****************************** Loaded Data *********************************** //");
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (Mod.IsSaving()) return;
            QuickLogger.Info("Saving MedBay");
            Mod.SaveMod();
        }

        internal void Save(SaveData saveDataList)
        {
            var prefabIdentifier = GetComponent<PrefabIdentifier>() ?? GetComponentInParent<PrefabIdentifier>();
            var id = prefabIdentifier.Id;
            var saveData = new SaveDataEntry();

            saveData.Id = id;
            saveData.SCA = Container.NumberOfFirstAids;
            saveData.TTS = Container.GetTimeToSpawn();
            saveData.BodyColor = _currentBodyColor.ColorToVector4();
            saveDataList.Entries.Add(saveData);
        }

        internal void SetCurrentBodyColor(Color color)
        {
            _currentBodyColor = color;
        }

        private void OnDestroy()
        {
            if (_display != null)
            {
                _display.Destroy();
            }

            if (Container != null)
            {
                Container.Destroy();
                Container.medBayContainer.onAddItem -= OnAddItemEvent;
                Container.medBayContainer.onRemoveItem -= OnRemoveItemEvent;
            }

            if (PlayerTrigger != null)
            {
                PlayerTrigger.OnPlayerStay -= OnPlayerStay;
                PlayerTrigger.OnPlayerExit -= OnPlayerExit;
            }

            if (PowerManager != null)
            {
                PowerManager.OnPowerOutage -= OnPowerOutage;
                PowerManager.OnPowerResume -= OnPowerResume;
            }
        }
    }
}
