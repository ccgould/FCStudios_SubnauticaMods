using AE.MiniFountainFilter.Mono;
using FCSCommon.Utilities;
using System;
using FCSCommon.Enums;
using UnityEngine;

namespace AE.MiniFountainFilter.Managers
{
    internal class PowerManager
    {
        #region Private Members
        private FCSPowerStates _powerState;
        private MiniFountainFilterController _mono;
        private SubRoot _habitat;
        private PowerRelay _connectedRelay;
        private float EnergyConsumptionPerSecond { get; } = QPatch.Configuration.EnergyPerSec; //0.425
        private float AvailablePower => this.ConnectedRelay.GetPower();
        private PowerRelay ConnectedRelay
        {
            get
            {
                while (_connectedRelay == null)
                    UpdatePowerRelay();

                return _connectedRelay;
            }
        }
        private FCSPowerStates PowerState
        {
            get => _powerState;
            set
            {
                _powerState = value;
                OnPowerUpdate?.Invoke(value);
            }
        }
        private bool _hasPowerToConsume;
        private float _energyToConsume;
        #endregion

        #region Internal Properties
        internal Action<FCSPowerStates> OnPowerUpdate;
        #endregion
        
        #region Internal Methods
        internal void Initialize(MiniFountainFilterController mono)
        {
            _mono = mono;
            var habitat = mono?.gameObject.transform?.parent?.gameObject.GetComponentInParent<SubRoot>();

            if (habitat != null)
            {
                _habitat = habitat;
            }
        }

        internal void UpdatePowerState()
        {
            if (_habitat.powerRelay.GetPowerStatus() == PowerSystem.Status.Offline)
            {
                SetPowerStates(FCSPowerStates.Unpowered);
                return;
            }

            if (!_hasPowerToConsume && GetPowerState() != FCSPowerStates.Unpowered)
            {
                SetPowerStates(FCSPowerStates.Unpowered);
                return;
            }

            if (_hasPowerToConsume && GetPowerState() != FCSPowerStates.Powered)
            {
                SetPowerStates(FCSPowerStates.Powered);
            }
        }

        internal void ConsumePower()
        {
            _energyToConsume = EnergyConsumptionPerSecond * DayNightCycle.main.deltaTime;
            bool requiresEnergy = GameModeUtils.RequiresPower();
            _hasPowerToConsume = !requiresEnergy || AvailablePower >= _energyToConsume;

            if (PowerState == FCSPowerStates.Unpowered || _mono.TankManager.IsFull()) return;

            if (!requiresEnergy) return;
            ConnectedRelay.ConsumeEnergy(_energyToConsume, out float amountConsumed);
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
            if (PowerState != FCSPowerStates.Powered)
            {
                return 0f;
            }
            return Mathf.Round(EnergyConsumptionPerSecond * 60) / 10f;
        }

        internal PowerRelay CurrentRelay()
        {
            return _connectedRelay;
        }
        #endregion

        #region Private Methods
        private void UpdatePowerRelay()
        {
            PowerRelay relay = PowerSource.FindRelay(_mono.transform);
            if (relay != null && relay != _connectedRelay)
            {
                _connectedRelay = relay;

                QuickLogger.Debug("PowerRelay found at last!");
            }
            else
            {
                _connectedRelay = null;
            }
        } 
        #endregion
    }
}
