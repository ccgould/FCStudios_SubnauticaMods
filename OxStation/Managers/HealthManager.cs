using MAC.OxStation.Config;
using MAC.OxStation.Mono;
using System;
using FCSCommon.Enums;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using UnityEngine;

namespace MAC.OxStation.Managers
{
    internal class HealthManager
    {
        private LiveMixin _liveMixin = new LiveMixin();
        public int HealthMultiplyer { get; set; } = 1;
        private const float DayNight = 1200f;
        private int _damagePerDay = 10;
        private float _damagePerSecond;
        private float _passedTime;
        private OxStationController _mono;
        private float _prevHealth;
        public Action OnDamaged { get; set; }
        public Action OnRepaired { get; set; }

        internal bool IsDamagedFlag()
        {
            return _liveMixin.health <= 0;
        }

        internal void UpdateHealthSystem()
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

        internal bool IsDamageApplied()
        {
            if (_liveMixin == null) return true;

            return _liveMixin.health <= 0;
        }

        internal float GetHealth()
        {
            return _liveMixin.health;
        }

        internal int GetHealthPercentageFull()
        {
            return Mathf.RoundToInt((100 * GetHealth()) / _liveMixin.maxHealth);
        }

        internal float GetHealthPercentage()
        {
            return GetHealth() / _liveMixin.maxHealth;
        }

        internal void SetHealth(float health)
        {
            if (!QPatch.Configuration.DamageOverTime) return;

            if (health <= 0)
            {
                _liveMixin.Kill();
                return;
            }
            _liveMixin.health = health;
        }

        internal void Initialize(OxStationController mono)
        {
            QuickLogger.Debug("Health Initialize");
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
            }
            else
            {
                QuickLogger.Error($"LiveMixing not found!");
            }
        }

        internal void ApplyDamage()
        {
            if (!QPatch.Configuration.DamageOverTime) return;

            if (_liveMixin.health > 0)
            {
                _liveMixin.health = Mathf.Clamp(_liveMixin.health - HealthMultiplyer, 0f, 100f);
            }
        }

        internal void HealthChecks()
        {
            try
            {
                if (GetHealth() >= 1f && !IsDamagedFlag() && !Mathf.Approximately(_prevHealth, GetHealth()))
                {
                    QuickLogger.Debug($"{Mod.FriendlyName} Repaired", true);
                    OnRepaired?.Invoke();
                    _prevHealth = GetHealth();
                }

                if (GetHealth() <= 0f && IsDamagedFlag() && !Mathf.Approximately(_prevHealth, GetHealth()))
                {
                    QuickLogger.Debug($"{Mod.FriendlyName} Damaged", true);
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
