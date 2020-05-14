using FCS_AIMarineTurbine.Buildable;
using FCS_AIMarineTurbine.Mono;
using FCSCommon.Utilities;
using System;
using System.Collections;
using UnityEngine;

namespace FCS_AIJetStreamT242.Mono
{
    /// <summary>
    // This class handles all power management  of the turbine
    /// </summary>
    internal class AIJetStreamT242PowerManager : PowerSource
    {
        private bool _hasBreakerTripped;
        internal bool IsSafeToContinue { get; set; }
        internal Action OnKillBattery { get; set; }
        internal Action OnBreakerTripped { get; set; }
        internal Action OnBreakerReset { get; set; }

        public float MaxPowerPerMin { get; set; } = 100;
        private float _capacity;
        private PowerRelay PowerRelay => _mono.PowerRelay;
        private bool _isBatteryDestroyed;
        private AIJetStreamT242Controller _mono;
        private float _storedPower;
        private float _energyPerSec;
        private bool _initialize;
        private float _secondsInAMinute = 60f;


        #region Unity Methods

        private void Update()
        {
            if (_initialize)
            {
                ProducePower();
            }
        }
        #endregion

        private void ProducePower()
        {
            if (_mono.HealthManager == null) return;
            
            if (_mono.MaxSpeed > 0)
            {
                var decPercentage = (MaxPowerPerMin / _mono.MaxSpeed) / _secondsInAMinute;
                _energyPerSec = (_mono.GetCurrentSpeed() * decPercentage) * DayNightCycle.main.deltaTime;
            }
            else
            {
                _energyPerSec = 0;
            }

            if (_hasBreakerTripped || _mono.HealthManager.IsDamageApplied()) return;
            
            float num2 = maxPower - power;
            
            if (num2 > 0f)
            {
                if (num2 < _energyPerSec)
                {
                    _energyPerSec = num2;
                }

                this.AddEnergy(_energyPerSec, out var amountStored);
            }
        }

        internal void KillBattery()
        {
            QuickLogger.Debug($"KillBattery");
            SetCharge(0);
            //_capacity = _charge = 0;
            OnKillBattery?.Invoke();
        }

        private void OnDestroy()
        {
            try
            {
                KillBattery();

                if (PowerRelay != null)
                {
                    PowerRelay.RemoveInboundPower(this);
                }
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }
        }
        
        internal float GetEnergyPerMinute()
        {
            return _energyPerSec * 60;
        }

        internal float GetCharge()
        {
            return GetPower();
        }

        internal void SetCharge(float savedDataCharge)
        {
           SetCharge(savedDataCharge);
        }

        private void TriggerPowerOff()
        {
            _hasBreakerTripped = true;
            //_storedPower = _charge;
            //_charge = 0.0f;
            OnBreakerTripped?.Invoke();
        }

        private void TriggerPowerOn()
        {
            //_charge = _storedPower;
            //_storedPower = 0.0f;
            _hasBreakerTripped = false;
            OnBreakerReset?.Invoke();
        }

        internal void Initialize(AIJetStreamT242Controller mono)
        {
            _capacity = AIJetStreamT242Buildable.JetStreamT242Config.MaxCapacity;
            _mono = mono;
            _initialize = true;
        }

        internal bool GetHasBreakerTripped()
        {
            return _hasBreakerTripped;
        }

        internal void SetHasBreakerTripped(bool value)
        {
            _hasBreakerTripped = value;
        }
        
        internal void TogglePower()
        {
            if (GetHasBreakerTripped())
            {
                TriggerPowerOn();
            }
            else if (GetHasBreakerTripped() == false)
            {
                TriggerPowerOff();
            }
        }

        internal float GetStoredPower()
        {
            return _storedPower;
        }

        internal void SetStoredPower(float value)
        {
            _storedPower = value;
        }

        public GameObject GetGameObject()
        {
            return base.gameObject;
        }
    }
}
