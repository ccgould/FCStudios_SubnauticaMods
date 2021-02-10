using System;
using System.Collections;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Managers;
using FCS_AlterraHub.Model;
using FCS_HomeSolutions.SeaBreeze.Enumerators;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.SeaBreeze.Mono
{
    internal class SeaBreezePowerManager : FCSPowerManager
    {
        private bool _hasBreakerTripped;
        public Action OnBreakerTripped { get; set; }
        public Action OnBreakerReset { get; set; }

        private readonly float _energyConsumptionPerSecond =QPatch.Configuration.SeaBreezePowerUsage;
        private float AvailablePower => _connectedRelay.GetPower() + _powercellData.GetCharge();
        public Action OnPowerOutage { get; set; }
        public Action OnPowerResume { get; set; }
        private PowercellData _powercellData;

        private const float ChargeSpeed = 0.005f;

        private SeaBreezeController _mono;

        public bool NotAllowToOperate => !_mono.IsConstructed || _connectedRelay == null || !QPatch.Configuration.SeaBreezeUseBasePower || _powercellData == null;

        private PowerRelay _connectedRelay;
        private float _energyToConsume;
        private bool _prevPowerState;
        private float _chargeTimer = 5f;

        private bool IsPowerAvailable => AvailablePower > _energyToConsume;

        #region Unity Methods
        private void Update()
        {
            
            if (this.NotAllowToOperate || _hasBreakerTripped)
                return;
            
            _energyToConsume = _energyConsumptionPerSecond * DayNightCycle.main.deltaTime;
            var batteryChargePull = DayNightCycle.main.deltaTime * ChargeSpeed * _powercellData.GetCapacity();
            bool requiresEnergy = GameModeUtils.RequiresPower();
            bool hasPowerToConsume = !requiresEnergy || (this.AvailablePower >= _energyToConsume) || !_powercellData.IsEmpty();
            
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

            _mono.DisplayManager.UpdateVisuals(_powercellData);

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
                        _chargeTimer -= DayNightCycle.main.deltaTime;

                        if (_chargeTimer < 0)
                        {
                            _connectedRelay.ConsumeEnergy(batteryChargePull, out float amountPConsumed);
                            _powercellData.AddCharge(amountPConsumed);
                            _chargeTimer = 5f;
                        }
                    }
                }
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
            _mono?.DisplayManager?.GotoPage(SeaBreezePages.PowerOffPage);
            OnBreakerTripped?.Invoke();
        }

        private void TriggerPowerOn()
        {
            _hasBreakerTripped = false;
            _mono?.DisplayManager?.GotoPage(SeaBreezePages.BootPage);
            OnBreakerReset?.Invoke();
        }

        internal void Initialize(SeaBreezeController mono)
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

        internal void LoadSave(PowercellData powercellData, bool hasBreakerTripped)
        {
            if (powercellData != null)
            {
                _powercellData = powercellData;
            }

            if (hasBreakerTripped)
            {
                TriggerPowerOff();
            }
        }

        internal PowercellData Save()
        {
            return _powercellData; 
        }

        #region FCSPowerManager

        public override float GetPowerUsagePerSecond()
        {
            return _energyConsumptionPerSecond;
        }

        public override float GetDevicePowerCharge()
        {
            return _powercellData.GetCharge();
        }

        public override float GetDevicePowerCapacity()
        {
            return _powercellData.GetCapacity();
        }

        public override FCSPowerStates GetPowerState()
        {
            return _hasBreakerTripped ? FCSPowerStates.Tripped : FCSPowerStates.Powered;
        }

        public override void TogglePowerState()
        {
            if (GetHasBreakerTripped())
            {
                TriggerPowerOn();
            }
            else
            {
                TriggerPowerOff();
            }

            QuickLogger.Debug($"HasBreakerTripped: {_hasBreakerTripped}", true);
        }

        public override void SetPowerState(FCSPowerStates state)
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

        public override bool IsDevicePowerFull()
        {
            return _powercellData.IsFull();
        }

        #endregion
    }
}
