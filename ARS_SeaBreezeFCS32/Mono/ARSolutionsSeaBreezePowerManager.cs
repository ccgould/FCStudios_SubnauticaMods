using FCSCommon.Utilities;
using System;
using System.Collections;
using ARS_SeaBreezeFCS32.Model;
using FCSTechFabricator.Enums;
using FCSTechFabricator.Interfaces;
using UnityEngine;

namespace ARS_SeaBreezeFCS32.Mono
{
    internal class ARSolutionsSeaBreezePowerManager : MonoBehaviour, IFCSPowerManager
    {
        private bool _hasBreakerTripped;
        public Action OnBreakerTripped { get; set; }
        public Action OnBreakerReset { get; set; }

        private float EnergyConsumptionPerSecond = QPatch.Configuration.PowerUsage;
        private float AvailablePower => _connectedRelay.GetPower() + _powercellData.GetCharge();
        public Action OnPowerOutage { get; set; }
        public Action OnPowerResume { get; set; }
        private PowercellData _powercellData;

        private const float chargeSpeed = 0.005f;

        private ARSolutionsSeaBreezeController _mono;

        public bool NotAllowToOperate => !_mono.IsConstructed || _connectedRelay == null || !QPatch.Configuration.UseBasePower || _powercellData == null;

        private PowerRelay _connectedRelay;
        private float _energyToConsume;
        private bool _prevPowerState;
        private float _chargeTimer = 5f;

        private bool IsPowerAvailable => AvailablePower > _energyToConsume;

        #region Unity Methods
        private void Update()
        {
            if (this.NotAllowToOperate)
                return;
            
            _energyToConsume = EnergyConsumptionPerSecond * DayNightCycle.main.deltaTime;
            var batteryChargePull = DayNightCycle.main.deltaTime * chargeSpeed * _powercellData.GetCapacity();
            bool requiresEnergy = GameModeUtils.RequiresPower();
            bool hasPowerToConsume = !requiresEnergy || (this.AvailablePower >= _energyToConsume);
            
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

            if (!hasPowerToConsume) return;


            if (requiresEnergy)
            {
                if (_connectedRelay.GetPower() <= _energyToConsume)
                {
                    _powercellData.RemoveCharge(_energyToConsume);
                    return;
                }

                if (!GetHasBreakerTripped())
                {
                    _connectedRelay.ConsumeEnergy(_energyToConsume, out float amountConsumed);
                }


                if (!_powercellData.IsFull())
                {
                    if (_connectedRelay.GetPower() >= batteryChargePull)
                    {
                        _chargeTimer -= Time.deltaTime;

                        if (_chargeTimer < 0)
                        {
                            _connectedRelay.ConsumeEnergy(batteryChargePull, out float amountPConsumed);
                            _powercellData.AddCharge(amountPConsumed);
                            QuickLogger.Debug($"Charging Battery: {amountPConsumed} units", true);
                            _chargeTimer = 5f;
                        }
                    }
                }
                //QuickLogger.Debug($"Power Consumed: {amountConsumed}");

                _mono.DisplayManager.UpdateVisuals(_powercellData);
            }
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
            _powercellData = new PowercellData();
            _powercellData.Initialize(200,200);
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

        internal bool GetIsPowerAvailable()
        {
            return _connectedRelay != null && IsPowerAvailable;
        }

        private void OnDestroy()
        {

        }

        internal void LoadSave(PowercellData powercellData)
        {
            if (powercellData != null)
            {
                _powercellData = powercellData;
            }
        }

        internal PowercellData Save()
        {
            return _powercellData; 
        }

        #region IFCSPowerManager

        public float GetPowerUsagePerSecond()
        {
            return EnergyConsumptionPerSecond;
        }

        public float GetDevicePowerCharge()
        {
            return _powercellData.GetCharge();
        }

        public float GetDevicePowerCapacity()
        {
            return _powercellData.GetCapacity();
        }

        public FCSPowerStates GetPowerState()
        {
            return _hasBreakerTripped ? FCSPowerStates.Tripped : FCSPowerStates.Powered;
        }

        public void TogglePowerState()
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

        public void SetPowerState(FCSPowerStates state)
        {
            switch (state)
            {
                case FCSPowerStates.Powered:
                    TriggerPowerOn();
                    break;
                case FCSPowerStates.Tripped:
                    TriggerPowerOff();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        public bool IsDevicePowerFull()
        {
            return _powercellData.IsFull();
        }

        public bool ModifyPower(float amount, out float consumed)
        {
            consumed = 0f;
            return false;
        }

        #endregion
    }
}
