using System;
using FCS_DeepDriller.Configuration;
using FCS_DeepDriller.Helpers;
using FCS_DeepDriller.Mono.MK2;
using FCSCommon.Enums;
using RadicalLibrary;
using UnityEngine;

namespace FCS_DeepDriller.Mono.MK1
{
    internal class FCSDeepDrillerPowerHandler : MonoBehaviour
    {
        private FCSPowerStates PowerState
        {
            get => _powerState;
            set
            {
                _powerState = value;
                OnPowerUpdate?.Invoke(value);
            }
        }
        private FCSPowerStates _powerState;
        private FCSDeepDrillerController _mono;
        private float _maxDepth = 200f;
        private AnimationCurve _depthCurve;
        private readonly DeepDrillerPowerData _powerBank = new DeepDrillerPowerData();
        private bool _prevPowerState;
        internal Action<FCSPowerStates> OnPowerUpdate;
        private bool _initialized;
        public Action OnBatteryUpdate { get; set; }

        private void Update()
        {
            if (_mono == null || _mono.DeepDrillerContainer == null || 
                _mono.DisplayHandler == null || !_mono.IsConstructed || 
                !_initialized) return;
            ProduceSolarPower();

            UpdatePowerState();
        }

        private void UpdatePowerState()
        {
            if (PowerState == FCSPowerStates.Tripped) return;

            if (IsPowerAvailable() && _prevPowerState != true)
            {
                PowerState = FCSPowerStates.Powered;
                _prevPowerState = true;
            }
            else if (!IsPowerAvailable() && _prevPowerState)
            {
                PowerState = FCSPowerStates.Unpowered;
                _prevPowerState = false;
            }
        }

        private void ProduceSolarPower()
        {
            if (_powerBank == null) return;
            _powerBank.SolarPanel = Mathf.Clamp(GetRechargeScalar() * DayNightCycle.main.deltaTime * 0.50f * 5f, 0f, QPatch.Configuration.SolarCapacity);
            //QuickLogger.Debug($"Current Solar Charge: {_powerBank.Solar.Battery.charge} || Current Solar Capacity: {_powerBank.Solar.Battery.capacity}");
        }

        private float GetRechargeScalar()
        {
            return this.GetDepthScalar() * this.GetSunScalar();
        }

        private float GetDepthScalar()
        {
#if SUBNAUTICA
            float time = Mathf.Clamp01((_maxDepth - Ocean.main.GetDepthOf(base.gameObject)) / _maxDepth);
#elif BELOWZERO
            float time = Mathf.Clamp01((_maxDepth - Ocean.GetDepthOf(base.gameObject)) / _maxDepth);
#endif
            return _depthCurve.Evaluate(time);
        }

        private float GetSunScalar()
        {
            return DayNightCycle.main.GetLocalLightScalar();
        }

        internal void Initialize(FCSDeepDrillerController mono)
        {
            _mono = mono;

            _depthCurve = new AnimationCurve();
            _depthCurve.AddKey(0f, 0f);
            _depthCurve.AddKey(0.4245796f, 0.5001081f);
            _depthCurve.AddKey(1f, 1f);
            PowerState = FCSPowerStates.Tripped;

            _initialized = true;
        }

        internal FCSPowerStates GetPowerState()
        {
            return PowerState;
        }

        internal void SetPowerState(FCSPowerStates state)
        {
            PowerState = state;
        }
        
        internal void LoadData(DeepDrillerSaveDataEntry data)
        {
            _powerBank.SolarPanel = data.PowerData.SolarPanel;
            _powerBank.Battery = data.PowerData.Battery;
            PowerState = data.PowerState;
        }

        internal float GetCharge()
        {
            return _powerBank.Battery.Charge;
        }

        internal string GetSolarPowerData()
        {
            return $"Solar panel (sun: {Mathf.RoundToInt(GetRechargeScalar() * 100f)}% charge {Mathf.RoundToInt(_powerBank.SolarPanel)}/{Mathf.RoundToInt(QPatch.Configuration.SolarCapacity)})";
        }

        /// <summary>
        /// Gets the data for saving the power data.
        /// </summary>
        /// <returns></returns>
        internal DeepDrillerPowerData SaveData()
        {
            return _powerBank;
        }
        
        /// <summary>
        /// Checks to see if all the conditions are met for being powered
        /// </summary>
        /// <returns>Returns true if power is available</returns>
        internal bool IsPowerAvailable()
        {
            if (GetCharge() <= 0)
            {
                return false;
            }

            return !QPatch.Configuration.AllowDamage || !_mono.HealthManager.IsDamagedFlag();
        }

        /// <summary>
        /// Gives the states of the solar panel
        /// </summary>
        /// <returns>Returns true if the solar panel is extended.</returns>
        internal bool IsSolarExtended()
        {
           return _mono.AnimationHandler.GetBoolHash(_mono.IsSolarExtended);
        }

        /// <summary>
        /// Add power to the battery from another power source
        /// </summary>
        /// <param name="powercell">The powercell  to pull power from</param>
        internal void ChargeBatteryFromPowercell(IBattery powercell)
        {
            if (powercell.charge <= 0 || _powerBank.Battery.IsFull()) return;

            //Get the amount the battery needs
            var remainder = MathHelpers.GetRemainder(_powerBank.Battery.Charge, _powerBank.Battery.Capacity);
            
            //Get the minium amount of power from the battery and the power requirements
            var amount = Mathf.Min(powercell.charge, remainder);
            
            //Set the new battery value
            powercell.charge = Mathf.Max(0f, powercell.charge - amount);
            
            //Add the new battery amount
            _powerBank.Battery.Charge = Mathf.Clamp(_powerBank.Battery.Charge + amount, 0, _powerBank.Battery.Capacity);
            
            //Notify the drill of the change
            OnBatteryUpdate?.Invoke();
        }

        //TODO Connect to base
    }
}