using System;
using DataStorageSolutions.Abstract;
using DataStorageSolutions.Model;
using FCSCommon.Enums;
using FCSCommon.Utilities;
using FCSTechFabricator.Enums;
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
                    OnPowerUpdate?.Invoke(value,_mono.Manager);
                    _prevPowerState = value;
                }
            }
        }
        private bool _hasPowerToConsume;
        private float _energyToConsume;
        #endregion

        #region Internal Properties

        internal Action<FCSPowerStates,BaseManager> OnPowerUpdate;
        private bool _isInitailized;

        #endregion
        
        #region Internal Methods
        internal void Initialize(DataStorageSolutionsController mono, float powerConsumption)
        {
            if (_isInitailized) return;

                _mono = mono;
            if (_mono.Manager != null)
            {
                _mono.Manager.OnBreakerToggled += OnBreakerToggled;
            }

            _energyConsumptionPerSecond = powerConsumption;

            _isInitailized = true;
        }

        private void OnBreakerToggled(bool obj)
        {
            SetPowerStates(obj ? FCSPowerStates.Tripped : FCSPowerStates.None);
        }

        internal void UpdatePowerState()
        {
            var habitat = _mono?.SubRoot;

            if (habitat == null || habitat.powerRelay == null || _powerState == FCSPowerStates.Tripped) return;

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
            if(PowerState == FCSPowerStates.Tripped)return;
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
