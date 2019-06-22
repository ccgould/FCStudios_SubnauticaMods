using FCSCommon.Utilities;
using System;
using System.Collections;
using UnityEngine;

namespace ARS_SeaBreezeFCS32.Mono
{
    internal class ARSolutionsSeaBreezePowerManager : MonoBehaviour
    {
        private bool _hasBreakerTripped;
        public Action OnBreakerTripped { get; set; }
        public Action OnBreakerReset { get; set; }

        private float EnergyConsumptionPerSecond = 0.2f;
        private float AvailablePower => _connectedRelay.GetPower();

        public Action OnPowerOutage { get; set; }
        public Action OnPowerResume { get; set; }

        private ARSolutionsSeaBreezeController _mono;

        public bool NotAllowToOperate => !_mono.IsConstructed || _connectedRelay == null;

        private PowerRelay _connectedRelay;
        private float _energyToConsume;
        private bool _prevPowerState;

        private bool IsPowerAvailable => AvailablePower > _energyToConsume;

        #region Unity Methods
        private void Update()
        {
            if (this.NotAllowToOperate)
                return;

            _energyToConsume = EnergyConsumptionPerSecond * DayNightCycle.main.deltaTime;
            bool requiresEnergy = GameModeUtils.RequiresPower();
            bool hasPowerToConsume = !requiresEnergy || (this.AvailablePower >= _energyToConsume);

            //QuickLogger.Debug($"HasPowerToConsume {hasPowerToConsume} || AVP {AvailablePower}|| PrevPowerState {_prevPowerState}");

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

            if (requiresEnergy && !GetHasBreakerTripped())
                _connectedRelay.ConsumeEnergy(_energyToConsume, out float amountConsumed);
        }
        #endregion

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

        private void TriggerPowerOff()
        {
            _hasBreakerTripped = true;
            OnBreakerTripped?.Invoke();
        }

        private void TriggerPowerOn()
        {
            _hasBreakerTripped = false;
            OnBreakerReset?.Invoke();
        }

        internal void Initialize(ARSolutionsSeaBreezeController mono)
        {
            _mono = mono;
            StartCoroutine(UpdatePowerRelay());
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

            QuickLogger.Debug($"HasBreakerTripped: {_hasBreakerTripped}", true);
        }

        public bool GetIsPowerAvailable()
        {
            return _connectedRelay != null && IsPowerAvailable;
        }

        private void OnDestroy()
        {

        }
    }
}
