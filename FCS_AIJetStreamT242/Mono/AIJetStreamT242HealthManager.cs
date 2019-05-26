using FCS_AIJetStreamT242.Buildable;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using System;
using UnityEngine;

namespace FCS_AIJetStreamT242.Mono
{
    internal class AIJetStreamT242HealthManager : MonoBehaviour
    {
        public LiveMixin LiveMixin { get; set; } = new LiveMixin();
        public bool IsDamagedFlag { get; set; } = true;
        public bool IsSafeToContinue { get; set; }
        public int HealthMultiplyer { get; set; } = 10;

        private float _passedTime = 0f;
        private AIJetStreamT242Controller _mono;
        private double _damageTimeInSeconds = 2520;
        public Action OnDamaged { get; set; }
        public Action OnRepaired { get; set; }


        #region Unity Methods

        private void Awake()
        {
            LiveMixin = GetComponentInParent<LiveMixin>();

            if (LiveMixin != null)
            {
                if (LiveMixin.data == null)
                {
                    QuickLogger.Error($"LiveMixing Data  is null!");
                    QuickLogger.Debug($"Creating Data");
                    LiveMixin.data = CustomLiveMixinData.Get();
                    QuickLogger.Debug($"Created Data");
                }
                else
                {
                    LiveMixin.data.weldable = true;
                }

                InvokeRepeating("HealthChecks", 0, 1);
            }
            else
            {
                QuickLogger.Error($"LiveMixing not found!");
            }
        }

        private void Update()
        {
            //TODO Check if will cause problem
            if (!IsSafeToContinue) return;

            _passedTime += DayNightCycle.main.deltaTime;

            if (_passedTime >= _damageTimeInSeconds)
            {
                ApplyDamage();
            }

            if (LiveMixin.health <= 0 && !_mono.PowerManager.IsBatteryDestroyed())
            {
                _mono.PowerManager.DestroyBattery();
            }
            else if (LiveMixin.health > 0 && _mono.PowerManager.IsBatteryDestroyed())
            {
                _mono.PowerManager.RepairBattery();
            }

        }
        #endregion

        public bool IsDamageApplied()
        {
            if (LiveMixin == null) return false;

            return LiveMixin.health <= 0;
        }

        internal float GetHealth()
        {
            return LiveMixin.health;
        }

        internal void SetHealth(float health)
        {
            LiveMixin.health = health;
        }

        internal void Initialize(AIJetStreamT242Controller mono)
        {
            _mono = mono;
        }

        public void ApplyDamage()
        {
            if (LiveMixin.health > 0 && AIJetStreamT242Buildable.JetStreamT242Config.EnableWear)
            {
                LiveMixin.health = Mathf.Clamp(LiveMixin.maxHealth - HealthMultiplyer, 0f, 100f);
            }
            ResetTimer();
        }

        private void HealthChecks()
        {
            try
            {
                if (GetHealth() >= 100f && !IsDamagedFlag)
                {
                    QuickLogger.Debug("Turbine Repaired");
                    OnRepaired?.Invoke();
                    //TODO Update Damage
                    //StartCoroutine(UpdateDamageMaterial());
                    IsDamagedFlag = true;
                }

                if (GetHealth() <= 0f && IsDamagedFlag)
                {
                    QuickLogger.Debug("Turbine Damaged");
                    OnDamaged?.Invoke();
                    //TODO Update Damage
                    //StartCoroutine(UpdateDamageMaterial());
                    IsDamagedFlag = false;
                }
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }
        }

        private void ResetTimer()
        {
            _passedTime = 0;
        }

        internal float GetPassedTime()
        {
            return _passedTime;
        }

        public void SetPassedTime(float savedDataPassedTime)
        {
            _passedTime = savedDataPassedTime;
        }
    }
}
