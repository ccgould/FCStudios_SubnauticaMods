using AMMiniMedBay.Display;
using AMMiniMedBay.Enumerators;
using AMMiniMedBay.Models;
using FCSCommon.Extensions;
using FCSCommon.Models.Abstract;
using FCSCommon.Utilities;
using System;
using UnityEngine;

namespace AMMiniMedBay.Mono
{
    internal class AMMiniMedBayController : FCSController, IConstructable
    {
        private GameObject _scanner;
        private bool _initialized;
        private float _processTime = 3.0f;
        private Constructable _buildable;
        private float _timeCurrDeltaTime;

        private float _healthPartial;
        private HealingStatus _healingStatus;
        private bool _isHealing;
        private AMMiniMedBayDisplay _display;
        public override Action OnMonoUpdate { get; set; }

        internal AMMiniMedBayAudioManager AudioHandler { get; set; }

        public AMMiniMedBayContainer Container { get; private set; }

        public override bool IsConstructed => _buildable != null && _buildable.constructed;

        public AMMiniMedBayPowerManager PowerManager { get; set; }
        public int PageHash { get; set; }
        public AMMiniMedBayAnimationManager AnimationManager { get; set; }

        internal void HealPlayer()
        {
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

        private void Update()
        {
            OnMonoUpdate?.Invoke();

            QuickLogger.Debug($"isHealing {_isHealing}");

            if (!_isHealing) return;

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
                _healingStatus = HealingStatus.Healing;
                Player.main.liveMixin.health = Mathf.Clamp(playerHealth + _healthPartial, 0, playerMaxHealth);
                QuickLogger.Debug($"Player Health = {playerHealth}", true);
            }
            else
            {
                QuickLogger.Debug("Resetting", true);
                _healingStatus = HealingStatus.Idle;
                UpdateIsHealing(false);
                PowerManager.SetHasBreakerTripped(true);
            }

        }

        private void Awake()
        {
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

            AudioHandler = new AMMiniMedBayAudioManager(gameObject.GetComponent<FMOD_CustomLoopingEmitter>());

            Container = new AMMiniMedBayContainer(this);
        }

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
            QuickLogger.Debug("In OnPowerResume", true);
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
    }
}
