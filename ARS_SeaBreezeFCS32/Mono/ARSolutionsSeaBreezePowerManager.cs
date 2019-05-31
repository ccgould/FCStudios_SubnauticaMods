using FCSCommon.Utilities;
using System;
using UnityEngine;

namespace ARS_SeaBreezeFCS32.Mono
{
    internal class ARSolutionsSeaBreezePowerManager : MonoBehaviour
    {
        private bool _hasBreakerTripped;
        public Action OnBreakerTripped { get; set; }
        public Action OnBreakerReset { get; set; }
        private float EnergyConsumptionPerSecond = 0f;
        private float AvailablePower => this.ConnectedRelay.GetPower();

        private PowerRelay _powerRelay;

        private ARSolutionsSeaBreezeController _mono;

        private PowerRelay _connectedRelay;

        private PowerRelay ConnectedRelay
        {
            get
            {
                while (_connectedRelay == null)
                    UpdatePowerRelay();

                return _connectedRelay;
            }
        }


        #region Unity Methods

        private void Awake()
        {
            UpdatePowerRelay();
        }

        private void Start()
        {
            UpdatePowerRelay();
        }

        private void Update()
        {
            float energyToConsume = EnergyConsumptionPerSecond * DayNightCycle.main.deltaTime;
            bool requiresEnergy = GameModeUtils.RequiresPower();
            bool hasPowerToConsume = !requiresEnergy || (this.AvailablePower >= energyToConsume);
            if (requiresEnergy)
                this.ConnectedRelay.ConsumeEnergy(energyToConsume, out float amountConsumed);
        }
        #endregion

        private void UpdatePowerRelay()
        {
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

        public void Initialize(ARSolutionsSeaBreezeController mono)
        {
            _mono = mono;
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
        }
    }
}
