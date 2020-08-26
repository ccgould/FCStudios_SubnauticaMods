using MAC.OxStation.Mono;
using System;
using FCSCommon.Enums;
using FCSCommon.Utilities;
using FCSTechFabricator.Enums;
using UnityEngine;

namespace MAC.OxStation.Managers
{
    internal class PowerManager : MonoBehaviour
    {
        #region Private Members

        private FCSPowerStates _powerState;
        private OxStationController _mono;
        private readonly PowerRelay _connectedRelay = null;
        private float EnergyConsumptionPerSecond => QPatch.Configuration.EnergyPerSec;
        private float EnergyToConsume => EnergyConsumptionPerSecond * DayNightCycle.main.deltaTime;
        private FCSPowerStates PowerState
        {
            get => _powerState;
            set
            {
                _powerState = value;
                OnPowerUpdate?.Invoke(value);
            }
        }
        private Coroutine _relayCoroutine;
        private bool _useInbound;

        #endregion

        #region  Unity Methods

        private void OnDestroy()
        {
            if (_relayCoroutine != null)
                StopCoroutine(_relayCoroutine);
        }

        #endregion

        #region Internal Methods

        internal Action<FCSPowerStates> OnPowerUpdate;

        internal PowerRelay PowerRelay { get; private set; }
        
        internal void Initialize(OxStationController mono)
        {
            _mono = mono;
            var habitat = mono?.gameObject.transform?.parent?.gameObject.GetComponentInParent<SubRoot>();


            if (PowerRelay == null)
            {
                PowerRelay = gameObject.GetComponent<PowerRelay>();
            }
            
            if (habitat != null)
            {
                PowerRelay relay = PowerSource.FindRelay(_mono.transform);
                if (relay != null && relay != _connectedRelay)
                {
                    PowerRelay.AddInboundPower(relay);
                    QuickLogger.Debug("PowerRelay found at last!", true);
                }
                PowerRelay.dontConnectToRelays = true;
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

            var power = _useInbound ? PowerRelay.GetPowerFromInbound() : PowerRelay.GetPower();

            if (power >= EnergyToConsume)
            {
                SetPowerStates(FCSPowerStates.Powered);
                return;
            }

            SetPowerStates(FCSPowerStates.Unpowered);
        }

        internal void ConsumePower()
        {
            if (_mono.HealthManager.IsDamageApplied() || PowerRelay == null) return;
            float amountConsumed;
            
            bool requiresEnergy = GameModeUtils.RequiresPower();

            if (!requiresEnergy) return;

            PowerRelay.ConsumeEnergy(EnergyToConsume, out amountConsumed);
            
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

        #endregion
    }
}