using System;
using System.Linq;
using FCSCommon.Enums;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using FCSTechFabricator.Enums;
using UnityEngine;

namespace FCS_DeepDriller.Mono.MK2
{
    internal class FCSDeepDrillerHealthHandler : MonoBehaviour
    {
        private LiveMixin _liveMixin = new LiveMixin();
        public float HealthMultiplyer { get; set; } = 1;
        private const float DayNight = 1200f;
        private int _damagePerDay = 10;
        private float _damagePerSecond;
        private float _passedTime;
        private FCSDeepDrillerController _mono;
        private float _prevHealth;
        private const float HealthMultiplierBase = 1;
        
        public Action OnDamaged { get; set; }
        public Action OnRepaired { get; set; }


        #region Unity Methods

        private void Update()
        {
            if (QPatch.Configuration.HardCoreMode)
            {
                _liveMixin.invincible = false;
                UpdateHealthSystem();
                UpdateHealthMultiplier();
            }
            else
            {
                _liveMixin.invincible = true;
            }
        }

        private void UpdateHealthMultiplier()
        {
            var amount = _mono.UpgradeManager.Upgrades.Sum(x => x.Damage);
            HealthMultiplyer = HealthMultiplierBase + amount;
        }

        #endregion

        internal bool IsDamagedFlag()
        {
            if (_liveMixin == null) return true;

            if (!QPatch.Configuration.HardCoreMode) return false;

            return _liveMixin.health <= 0;
        }

        private void UpdateHealthSystem()
        {
            _passedTime += DayNightCycle.main.deltaTime;

            //QuickLogger.Debug($"Passed Time: {_passedTime} || Health {GetHealth()}");

            if (_mono == null) return;

            if (_mono.PowerManager?.GetPowerState() != FCSPowerStates.Powered)
            {
                //QuickLogger.Debug("Not Damaging Unit");
                ResetPassedTime();
                return;
            }

            if (_passedTime >= _damagePerSecond)
            {
                //QuickLogger.Debug("Damaging Unit");
                ApplyDamage();
                ResetPassedTime();
            }
        }
        
        internal float GetHealth()
        {
            return _liveMixin.health;
        }

        internal void SetHealth(float health)
        {
            _liveMixin.health = health;
        }

        internal void Initialize(FCSDeepDrillerController mono)
        {
            _mono = mono;
            _liveMixin = mono.gameObject.AddComponent<LiveMixin>();
            
            _damagePerSecond = DayNight / _damagePerDay;

            if (_liveMixin != null)
            {
                if (_liveMixin.data == null)
                {
                    QuickLogger.Debug($"Creating Data");
                    _liveMixin.data = CustomLiveMixinData.Get();
                    QuickLogger.Debug($"Created Data");
                }


                InvokeRepeating("HealthChecks", 0, 1);
            }
            else
            {
                QuickLogger.Error($"LiveMixing not found!");
            }
        }

        internal void ApplyDamage()
        {
            if (_liveMixin.health > 0)
            {
                _liveMixin.health = Mathf.Clamp(_liveMixin.health - HealthMultiplyer, 0f, 100f);
            }
        }

        private void HealthChecks() // In and InvokeRepeating
        {
            try
            {
                if (!QPatch.Configuration.HardCoreMode && GetHealth() <= 0f)
                {
                    OnRepaired?.Invoke();
                    _prevHealth = 100;
                    return;
                }

                if (GetHealth() >= 1f && !IsDamagedFlag() && !Mathf.Approximately(_prevHealth, GetHealth()))
                {
                    QuickLogger.Debug("Drill Repaired", true);
                    OnRepaired?.Invoke();
                    _prevHealth = GetHealth();
                }

                if (GetHealth() <= 0f && IsDamagedFlag() && !Mathf.Approximately(_prevHealth, GetHealth()))
                {
                    QuickLogger.Debug("Drill Damaged", true);
                    OnDamaged?.Invoke();
                    _prevHealth = GetHealth();
                }
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }
        }

        private void ResetPassedTime()
        {
            _passedTime = 0;
        }

        internal float GetPassedTime()
        {
            return _passedTime;
        }

        internal void SetPassedTime(float savedDataPassedTime)
        {
            _passedTime = savedDataPassedTime;
        }
    }
}
