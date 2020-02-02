using FCSCommon.Objects;
using FCSCommon.Utilities;
using FCSCommon.Enums;
using System;
using UnityEngine;


namespace FCS_DeepDriller.Mono.Handlers
{
    internal class FCSDeepDrillerHealthHandler : MonoBehaviour
    {

        private LiveMixin _liveMixin = new LiveMixin();
        public int HealthMultiplyer { get; set; } = 1;
        private const float DayNight = 1200f;
        private int _damagePerDay = 10;
        private float _damagePerSecond;
        private float _passedTime;
        private FCSDeepDrillerController _mono;
        private float _prevHealth;
        public Action OnDamaged { get; set; }
        public Action OnRepaired { get; set; }


        #region Unity Methods

        private void Update()
        {
            UpdateHealthSystem();
        }

        #endregion

        internal bool IsDamagedFlag()
        {
            if (_liveMixin == null) return true;

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
