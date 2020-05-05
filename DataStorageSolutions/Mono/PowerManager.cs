using System;
using DataStorageSolutions.Abstract;
using FCSCommon.Enums;
using FCSCommon.Utilities;
using UnityEngine;

namespace DataStorageSolutions.Mono
{
    internal class PowerManager
    {
        #region Private Members
        private FCSPowerStates _powerState;
        private PowerRelay _connectedRelay;
        private DataStorageSolutionsController _mono;
        private float _energyConsumptionPerSecond;
        private FCSPowerStates _prevPowerState;
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

                if (_prevPowerState != value)
                {
                    OnPowerUpdate?.Invoke(value);
                    _prevPowerState = value;
                }

            }
        }
        private bool _hasPowerToConsume;
        private float _energyToConsume;
        #endregion

        #region Internal Properties

        internal Action<FCSPowerStates> OnPowerUpdate;


        #endregion
        
        #region Internal Methods
        internal void Initialize(DataStorageSolutionsController mono, float powerConsumption)
        {
            _mono = mono;

            _energyConsumptionPerSecond = powerConsumption;
        }
        
        internal void UpdatePowerState()
        {
            var habitat = _mono.SubRoot;
            if (habitat == null || habitat.powerRelay == null) return;

            if (habitat.powerRelay.GetPowerStatus() == PowerSystem.Status.Offline)
            {
                SetPowerStates(FCSPowerStates.Unpowered);
                return;
            }

            if (habitat.powerRelay.GetPowerStatus() == PowerSystem.Status.Normal || habitat.powerRelay.GetPowerStatus() == PowerSystem.Status.Emergency)
            {
                SetPowerStates(FCSPowerStates.Powered);
            }
        }

        internal void ConsumePower()
        {
            _energyToConsume = _energyConsumptionPerSecond * DayNightCycle.main.deltaTime;
            bool requiresEnergy = GameModeUtils.RequiresPower();
            _hasPowerToConsume = !requiresEnergy || AvailablePower >= _energyToConsume;

            if (PowerState == FCSPowerStates.Unpowered) return;

            if (!requiresEnergy) return;
            ConnectedRelay.ConsumeEnergy(_energyToConsume, out float amountConsumed);
            //QuickLogger.Debug($"Energy Consumed: {amountConsumed}");
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
            return Mathf.Round(_energyConsumptionPerSecond * 60) / 10f;
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

        public void UpdatePowerUsage(float configServerPowerUsage)
        {
            _energyConsumptionPerSecond = configServerPowerUsage;
        }
    }
}
