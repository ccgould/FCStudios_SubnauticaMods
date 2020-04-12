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
        private const int HealthMultiplier = 1;
        private const float DayNight = 1200f;
        private const int DamagePerDay = 10;
        private float _damagePerSecond;
        private float _passedTime;
        private OxStationController _mono;
        private bool _wasDead;
        internal Action OnDamaged { get; set; }
        internal Action OnRepaired { get; set; }


        private void ResetPassedTime()
        {
            _passedTime = 0;
        }

        /// <summary>
        /// Gets if the oxstation is currently damaged.
        /// </summary>
        /// <returns>Returns true if the oxstation is damaged.</returns>
        internal bool IsDamagedFlag()
        {
            return _liveMixin.health <= 0;
        }

        internal void UpdateHealthSystem()
        {
            // Keep track of how much time has passed
            _passedTime += DayNightCycle.main.deltaTime;

            if (_mono == null) return;

            // If power is off reset the time since we cant recieve damage if the unit isnt running
            if (_mono.PowerManager?.GetPowerState() != FCSPowerStates.Powered)
            {
                ResetPassedTime();
                return;
            }

            //Apply damage to the device
            if (_passedTime >= _damagePerSecond)
            {
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
            _damagePerSecond = DayNight / DamagePerDay;

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
                _liveMixin.health = Mathf.Clamp(_liveMixin.health - HealthMultiplier, 0f, 100f);
            }
        }

        internal void HealthChecks()
        {
            try
            {
                if (GetHealth() >= 1f && !IsDamagedFlag() && _wasDead)
                {
                    QuickLogger.Debug($"{Mod.FriendlyName} Repaired", true);
                    OnRepaired?.Invoke();
                    _wasDead = false;
                }

                if (GetHealth() <= 0f && IsDamagedFlag() && !_wasDead)
                {
                    QuickLogger.Debug($"{Mod.FriendlyName} Damaged", true);
                    OnDamaged?.Invoke();
                    _wasDead = true;
                }
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }
        }

        internal void Kill()
        {
            _liveMixin.health = 0;
        }
    }
}
