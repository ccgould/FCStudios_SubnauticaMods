using AE.SeaCooker.Mono;
using FCSCommon.Utilities;
using System;
using FCSCommon.Enums;
using UnityEngine;

namespace AE.SeaCooker.Managers
{
    internal class PowerManager
    {
        private FCSPowerStates _powerState;
        private SeaCookerController _mono;
        private SubRoot _habitat;
        private PowerRelay _connectedRelay = null;
        private float EnergyConsumptionPerSecond { get; set; } = QPatch.Configuration.Config.EnergyPerSec;
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
                //QuickLogger.Debug($"Current PowerState: {value}");
                OnPowerUpdate?.Invoke(value);
            }
        }

        internal Action<FCSPowerStates> OnPowerUpdate;
        private bool _hasPowerToConsume;
        private float _energyToConsume;

        internal void Initialize(SeaCookerController mono)
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

            if (_habitat.powerRelay.GetPowerStatus() == PowerSystem.Status.Offline || !_hasPowerToConsume && GetPowerState() != FCSPowerStates.Unpowered)
            {
                SetPowerStates(FCSPowerStates.Unpowered);
                return;
            }

            if (_habitat.powerRelay.GetPowerStatus() != PowerSystem.Status.Offline || _hasPowerToConsume && GetPowerState() != FCSPowerStates.Powered)
            {
                SetPowerStates(FCSPowerStates.Powered);
            }
        }

        internal void ConsumePower()
        {
            if (PowerState == FCSPowerStates.Unpowered || !_mono.FoodManager.IsCooking()) return;

            _energyToConsume = EnergyConsumptionPerSecond * DayNightCycle.main.deltaTime;
            bool requiresEnergy = GameModeUtils.RequiresPower();
            _hasPowerToConsume = !requiresEnergy || AvailablePower >= _energyToConsume;

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
            return Mathf.Round(EnergyConsumptionPerSecond * 60) / 10f;
        }

        internal PowerRelay CurrentRelay()
        {
            return _connectedRelay;
        }
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
    }
}
