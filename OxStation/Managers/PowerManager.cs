using MAC.OxStation.Mono;
using System;
using FCSCommon.Enums;
using FCSCommon.Utilities;
using UnityEngine;

namespace MAC.OxStation.Managers
{
    internal class PowerManager : MonoBehaviour
    {
        private FCSPowerStates _powerState;
        private OxStationController _mono;

        private PowerRelay _connectedRelay = null;
        private float EnergyConsumptionPerSecond { get; set; } = QPatch.Configuration.EnergyPerSec;
        private float AvailablePower => GetPower();

        private float _energyToConsume => EnergyConsumptionPerSecond * DayNightCycle.main.deltaTime;

        private float GetPower()
        {
            float power = 0;

            if (_mono.PowerRelay != null)
            {
                return _useInbound ? _mono.PowerRelay.GetPowerFromInbound() : _mono.PowerRelay.GetPower();
            }

            return power;
        }

        private FCSPowerStates PowerState
        {
            get => _powerState;
            set
            {
                _powerState = value; 
                QuickLogger.Debug($"Current PowerState: {value}");
                OnPowerUpdate?.Invoke(value);
            }
        }

        internal Action<FCSPowerStates> OnPowerUpdate;
        private bool _hasPowerToConsume;
        private Coroutine _relayCoroutine;
        private bool _checkingForRelay;
        private bool _useInbound;

        internal void Initialize(OxStationController mono)
        {
            _mono = mono;
            var habitat = mono?.gameObject.transform?.parent?.gameObject.GetComponentInParent<SubRoot>();

            if (habitat != null)
            {
                PowerRelay relay = PowerSource.FindRelay(_mono.transform);
                if (relay != null && relay != _connectedRelay)
                {
                    _mono.PowerRelay.AddInboundPower(relay);
                    _checkingForRelay = false;
                    QuickLogger.Debug("PowerRelay found at last!", true);
                }
                _mono.PowerRelay.dontConnectToRelays = true;
                _useInbound = true;
            }
        }

        internal void UpdatePowerState()
        {
            if (_mono.HealthManager.IsDamageApplied())
            {
                SetPowerStates(FCSPowerStates.Unpowered);
                return;
            }

            var power = _useInbound ? _mono.PowerRelay.GetPowerFromInbound() : _mono.PowerRelay.GetPower();

            if (power >= _energyToConsume)
            {
                SetPowerStates(FCSPowerStates.Powered);
                return;
            }

            SetPowerStates(FCSPowerStates.Unpowered);
        }

        internal void ConsumePower()
        {
            if (_mono.HealthManager.IsDamageApplied() || _mono.PowerRelay == null) return;
            float amountConsumed;
            
            bool requiresEnergy = GameModeUtils.RequiresPower();
            _hasPowerToConsume = !requiresEnergy || AvailablePower >= _energyToConsume;

            if (!requiresEnergy) return;

            _mono.PowerRelay.ConsumeEnergy(_energyToConsume, out amountConsumed);
            
            QuickLogger.Debug($"Energy Consumed: {amountConsumed}");
        }

        /// <summary>
        /// Gets the powerState of the unit.
        /// </summary>
        /// <returns></returns>
        internal FCSPowerStates GetPowerState()
        {
            return PowerState;
        }

        /// <summary>
        /// Sets the powerstate
        /// </summary>
        /// <param name="powerState">The power state to set this unit</param>
        internal void SetPowerStates(FCSPowerStates powerState)
        {
            PowerState = powerState;
        }

        internal float GetPowerUsage()
        {
            if (_mono.HealthManager.IsDamageApplied() || PowerState != FCSPowerStates.Powered)
            {
                return 0f;
            }
            return Mathf.Round(EnergyConsumptionPerSecond * 60);
        }
        
        private void OnDestroy()
        {
            if(_relayCoroutine != null)
                StopCoroutine(_relayCoroutine);
        }
    }
}
