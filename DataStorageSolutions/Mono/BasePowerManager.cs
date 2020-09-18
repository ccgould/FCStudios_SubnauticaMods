using System;
using DataStorageSolutions.Abstract;
using FCSCommon.Utilities;
using FCSTechFabricator.Abstract;
using FCSTechFabricator.Enums;
using UnityEngine;

namespace DataStorageSolutions.Mono
{
    internal class BasePowerManager : FCSPowerManager
    {
        #region Private Members
        private FCSPowerStates _powerState;
        private PowerRelay _connectedRelay;
        private float _energyConsumptionPerSecond;
        private FCSPowerStates _prevPowerState;
        private bool _isInitialized;
        private BaseManager _manager;
        private float AvailablePower => ConnectedRelay?.GetPower() ?? 0f;
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
                    OnPowerUpdate?.Invoke(value,_manager);
                    _prevPowerState = value;
                }
            }
        }
        private float _energyToConsume;
        #endregion

        #region Internal Properties

        internal Action<FCSPowerStates,BaseManager> OnPowerUpdate;

        #endregion
        
        #region Internal Methods
        internal void Initialize(BaseManager manager)
        {
            if (_isInitialized) return;

                _manager = manager;

            _isInitialized = true;
        }
        
        internal void UpdatePowerState()
        {
            var habitat = _manager.Habitat;

            if (habitat == null || habitat.powerRelay == null || _manager == null) return;

            if (_manager.GetHasBreakerTripped())
            {
                SetPowerState(FCSPowerStates.Tripped);
            }
            else if (habitat.powerRelay.GetPowerStatus() == PowerSystem.Status.Offline)
            {
                SetPowerState(FCSPowerStates.Unpowered);
            }
            else if (habitat.powerRelay.GetPowerStatus() == PowerSystem.Status.Normal || habitat.powerRelay.GetPowerStatus() == PowerSystem.Status.Emergency)
            {
                SetPowerState(FCSPowerStates.Powered);
            }
        }

        internal void ConsumePower()
        {
            if(PowerState == FCSPowerStates.Tripped)return;

            _energyToConsume = _energyConsumptionPerSecond * DayNightCycle.main.deltaTime;
            bool requiresEnergy = GameModeUtils.RequiresPower();

            if (PowerState == FCSPowerStates.Unpowered) return;

            if (!requiresEnergy) return;
            ConnectedRelay.ConsumeEnergy(_energyToConsume, out float amountConsumed);
        }
        
        #endregion

        #region Private Methods
        private void UpdatePowerRelay()
        {
            if(_manager == null || _manager.Habitat == null) return;
            PowerRelay relay = PowerSource.FindRelay(_manager.Habitat.transform);
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

        private void Update()
        {
            if (_manager == null) return;
            UpdatePowerState();
            ConsumePower();
        }

        #endregion

        public void AddPowerUsage(float usage)
        {
            _energyConsumptionPerSecond += usage;
        }

        public void RemovePowerUsage(float usage)
        {
            _energyConsumptionPerSecond -= usage;
        }

        public override float GetPowerUsagePerSecond()
        {
            return _energyConsumptionPerSecond;
        }

        public override FCSPowerStates GetPowerState()
        {
            return _powerState;
        }

        public override void SetPowerState(FCSPowerStates state)
        {
            PowerState = state;
        }
    }
}
