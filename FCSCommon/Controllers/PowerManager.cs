using FCSCommon.Models.Abstract;
using FCSCommon.Utilities;
using System;
using System.Collections;
using UnityEngine;

namespace FCSCommon.Controllers
{
    public abstract class PowerManager : MonoBehaviour
    {
        public Action OnPowerOutage { get; set; }

        public Action OnPowerResume { get; set; }

        public abstract float EnergyConsumptionPerSecond { get; set; }

        private float AvailablePower => _connectedRelay.GetPower();

        private PowerRelay _connectedRelay;

        private FCSController _mono;

        private bool _prevPowerState;

        private bool _hasBreakerTripped;

        private float _energyToConsume;

        public Action OnBreakerReset { get; set; }

        public bool NotAllowToOperate => !_mono.IsConstructed || GetHasBreakerTripped() || _connectedRelay == null;

        public Action OnBreakerTripped { get; set; }

        public virtual void Update()
        {
            if (this.NotAllowToOperate)
                return;

            _energyToConsume = EnergyConsumptionPerSecond * DayNightCycle.main.deltaTime;
            bool requiresEnergy = GameModeUtils.RequiresPower();
            bool hasPowerToConsume = !requiresEnergy || (this.AvailablePower >= _energyToConsume);

            if (hasPowerToConsume && !_prevPowerState)
            {
                OnPowerResume?.Invoke();
                _prevPowerState = true;
            }
            else if (!hasPowerToConsume && _prevPowerState)
            {
                OnPowerOutage?.Invoke();
                _prevPowerState = false;
            }

            if (!hasPowerToConsume)
                return;

            if (requiresEnergy)
                _connectedRelay.ConsumeEnergy(_energyToConsume, out float amountConsumed);
        }

        private IEnumerator UpdatePowerRelay()
        {
            QuickLogger.Debug("In UpdatePowerRelay found at last!");

            var i = 1;

            while (_connectedRelay == null)
            {
                QuickLogger.Debug($"Checking For Relay... Attempt {i}");

                PowerRelay relay = PowerSource.FindRelay(this.transform);
                if (relay != null && relay != _connectedRelay)
                {
                    _connectedRelay = relay;
                    QuickLogger.Debug("PowerRelay found at last!");
                }
                else
                {
                    _connectedRelay = null;
                }

                i++;
                yield return new WaitForSeconds(0.5f);
            }
        }

        public virtual void Initialize(FCSController mono)
        {
            _mono = mono;
            StartCoroutine(UpdatePowerRelay());
        }

        public virtual void TriggerPowerOff()
        {
            _hasBreakerTripped = true;
            OnBreakerTripped?.Invoke();
        }
        public virtual void TriggerPowerOn()
        {
            _hasBreakerTripped = false;
            OnBreakerReset?.Invoke();
        }

        private bool IsPowerAvailable => AvailablePower > _energyToConsume || !_hasBreakerTripped;

        public virtual bool GetHasBreakerTripped()
        {
            return _hasBreakerTripped;
        }

        public virtual void SetHasBreakerTripped(bool value)
        {
            _hasBreakerTripped = value;
        }

        public virtual void TogglePower()
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

        public virtual bool GetIsPowerAvailable()
        {
            return _connectedRelay != null && IsPowerAvailable;
        }

    }
}
