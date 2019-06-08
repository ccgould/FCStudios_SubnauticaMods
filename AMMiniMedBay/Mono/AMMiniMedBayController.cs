using AMMiniMedBay.Display;
using AMMiniMedBay.Enumerators;
using AMMiniMedBay.Models;
using FCSCommon.Extensions;
using FCSCommon.Models.Abstract;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Utility;
using System;
using System.IO;
using UnityEngine;

namespace AMMiniMedBay.Mono
{
    internal class AMMiniMedBayController : FCSController, IConstructable, IProtoTreeEventListener
    {
        private GameObject _scanner;
        private bool _initialized;
        private float _processTime = 3.0f;
        private Constructable _buildable;
        private float _timeCurrDeltaTime;
        private PrefabIdentifier _prefabId;
        private readonly string _saveDirectory = Path.Combine(SaveUtils.GetCurrentSaveDataDir(), "AMMiniMedBay");
        private string SaveFile => Path.Combine(_saveDirectory, _prefabId.Id + ".json");
        private float _healthPartial;
        private HealingStatus _healingStatus;
        private bool _isHealing;
        private AMMiniMedBayDisplay _display;
        private NitrogenLevel _nitrogenLevel;
        public override Action OnMonoUpdate { get; set; }

        internal AMMiniMedBayAudioManager AudioHandler { get; set; }

        public AMMiniMedBayContainer Container { get; private set; }

        public override bool IsConstructed => _buildable != null && _buildable.constructed;

        public AMMiniMedBayPowerManager PowerManager { get; set; }
        public int PageHash { get; set; }
        public AMMiniMedBayAnimationManager AnimationManager { get; set; }

        internal void HealPlayer()
        {
            if (!PowerManager.GetIsPowerAvailable()) return;
            Player.main.playerController.SetEnabled(false);
            RemoveNitrogen();
            var remainder = Player.main.liveMixin.maxHealth - Player.main.liveMixin.health;
            _healthPartial = remainder / _processTime;
            PowerManager.SetHasBreakerTripped(false);
            UpdateIsHealing(true);
        }

        private void RemoveNitrogen()
        {
            if (_nitrogenLevel.GetNitrogenEnabled())
            {
                if (_nitrogenLevel.safeNitrogenDepth <= 100f)
                    _nitrogenLevel.safeNitrogenDepth = 0f;
                else
                    _nitrogenLevel.safeNitrogenDepth -= 100f;
            }
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

        private void Update()
        {
            _display.UpdatePlayerHealthPercent(IsPlayerInTrigger ? Mathf.CeilToInt(Player.main.liveMixin.health) : 0);

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

            if (!Player.main.liveMixin.IsFullHealth())
            {
                QuickLogger.Debug("Added Health", true);

                Player.main.liveMixin.health = Mathf.Clamp(playerHealth + _healthPartial, 0, playerMaxHealth);
                QuickLogger.Debug($"Player Health = {playerHealth}", true);
            }
            else
            {
                Player.main.playerController.SetEnabled(true);
                QuickLogger.Debug("Resetting", true);
                _healingStatus = HealingStatus.Idle;
                UpdateIsHealing(false);
                PowerManager.SetHasBreakerTripped(true);
            }
        }

        private void Awake()
        {
            _prefabId = GetComponentInParent<PrefabIdentifier>();

            if (_prefabId == null)
            {
                QuickLogger.Error("Prefab Identifier Component was not found");
            }

            if (_buildable == null)
            {
                _buildable = GetComponentInParent<Constructable>();
            }

            if (!FindAllComponents())
            {
                _initialized = false;
                throw new MissingComponentException("Failed to find all components");
            }

            PowerManager = gameObject.GetOrAddComponent<AMMiniMedBayPowerManager>();

            PlayerTrigger = gameObject.FindChild("model").FindChild("Trigger").GetOrAddComponent<AMMiniMedBayTrigger>();

            if (PlayerTrigger != null)
            {
                PlayerTrigger.OnPlayerStay += OnPlayerStay;
                PlayerTrigger.OnPlayerExit += OnPlayerExit;
            }
            else
            {
                QuickLogger.Error("Player Trigger Component was not found");
            }

            AnimationManager = gameObject.GetOrAddComponent<AMMiniMedBayAnimationManager>();

            if (AnimationManager == null)
            {
                QuickLogger.Error("Animation Controller Not Found!");
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

            AudioHandler = new AMMiniMedBayAudioManager(gameObject.GetComponent<FMOD_CustomLoopingEmitter>());

            Container = new AMMiniMedBayContainer(this);

            if (Player.main.gameObject.GetComponent<NitrogenLevel>() != null)
                _nitrogenLevel = Player.main.gameObject.GetComponent<NitrogenLevel>();
        }

        private void OnPlayerExit()
        {
            IsPlayerInTrigger = false;
        }

        private void OnPlayerStay()
        {
            IsPlayerInTrigger = true;
        }

        public bool IsPlayerInTrigger { get; set; }

        public AMMiniMedBayTrigger PlayerTrigger { get; set; }

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
            UpdateIsHealing(_healingStatus == HealingStatus.Healing);
        }

        private void OnPowerOutage()
        {
            QuickLogger.Debug("In OnPowerOutage", true);
            UpdateIsHealing(false);
        }

        private bool FindAllComponents()
        {
            QuickLogger.Debug("********************************************** In FindComponents **********************************************");

            _scanner = transform.Find("model")?.gameObject;

            if (_scanner == null)
            {
                QuickLogger.Error($"Scanner not found");
                _initialized = false;
                return false;
            }

            _initialized = true;
            return true;
        }

        public bool CanDeconstruct(out string reason)
        {
            reason = !Container.GetIsEmpty() ? "Please empty MedKit Container" : string.Empty;
            return Container.GetIsEmpty();
        }

        public void OnConstructedChanged(bool constructed)
        {
            if (constructed)
            {
                QuickLogger.Debug("Constructed", true);

                if (_display == null)
                {
                    _display = gameObject.AddComponent<AMMiniMedBayDisplay>();
                    _display.Setup(this);
                }
            }
        }

        internal void UpdateKitAmount(int containerSlotsFilled)
        {
            _display.ChangeStorageAmount(containerSlotsFilled);
        }

        public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
        {
            QuickLogger.Debug($"Saving {_prefabId.Id} Data");

            if (!Directory.Exists(_saveDirectory))
                Directory.CreateDirectory(_saveDirectory);

            var saveData = new SaveData
            {
                SCA = Container.NumberOfCubes
            };

            var output = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            File.WriteAllText(SaveFile, output);

            QuickLogger.Debug($"Saved {_prefabId.Id} Data");
        }

        public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("// ****************************** Load Data *********************************** //");

            if (_prefabId != null)
            {
                QuickLogger.Info($"Loading AM Mini MedBay {_prefabId.Id}");

                if (File.Exists(SaveFile))
                {
                    string savedDataJson = File.ReadAllText(SaveFile).Trim();

                    //LoadData
                    var savedData = JsonConvert.DeserializeObject<SaveData>(savedDataJson);
                    Container.NumberOfCubes = savedData.SCA;
                }
            }
            else
            {
                QuickLogger.Error("PrefabIdentifier is null");
            }
            QuickLogger.Debug("// ****************************** Loaded Data *********************************** //");
        }
    }
}
