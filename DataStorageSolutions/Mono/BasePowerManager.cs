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
        private float AvailablePower => _connectedRelay?.GetPower() ?? 0f;

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
                _connectedRelay = gameObject.GetComponent<PowerRelay>();
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
            if(PowerState == FCSPowerStates.Tripped || _connectedRelay == null)return;

            _energyToConsume = _energyConsumptionPerSecond * DayNightCycle.main.deltaTime;
            bool requiresEnergy = GameModeUtils.RequiresPower();

            if (PowerState == FCSPowerStates.Unpowered) return;

            if (!requiresEnergy) return;
            _connectedRelay.ConsumeEnergy(_energyToConsume, out float amountConsumed);
        }
        
        #endregion

        #region Private Methods
        
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

        internal bool HasPower()
        {

            if (_connectedRelay == null)
            {
                return false;
            }

            if (_connectedRelay == null) return false;

            if (_connectedRelay.GetPowerStatus() == PowerSystem.Status.Offline)
            {
                return false;
            }

            return _connectedRelay.GetPowerStatus() == PowerSystem.Status.Normal || _connectedRelay.GetPowerStatus() == PowerSystem.Status.Emergency;
        }
    }
}
