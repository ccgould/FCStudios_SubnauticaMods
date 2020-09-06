using System;
using System.Collections.Generic;
using System.Linq;
using FCS_DeepDriller.Configuration;
using FCS_DeepDriller.Helpers;
using FCS_DeepDriller.StatePattern;
using FCSCommon.Enums;
using FCSCommon.Utilities;
using FCSTechFabricator.Enums;
using FCSTechFabricator.Interfaces;
using UnityEngine;

namespace FCS_DeepDriller.Mono.MK2
{
    internal class FCSDeepDrillerPowerHandler : StateMachine, IFCSStorage, IFCSPowerManager
    {
        #region Private Fields

        private const bool PullPowerFromRelay = true; //Setting this to true by default until I feel like making it a toggle option
        private FCSPowerStates _powerState;
        private AnimationCurve _depthCurve;
        private readonly DeepDrillerPowerData _powerBank = new DeepDrillerPowerData();
        private float _powerDraw;
        private PowerRelay _powerRelay;
        private FCSPowerStates _prevPowerState;
        private FCSDeepDrillerController _mono;
        private bool _initialized;
        private const float MaxDepth = 200f;
        private FCSPowerStates PowerState
        {
            get => _powerState;
            set
            {
                _powerState = value;
                OnPowerUpdate?.Invoke(value);
            }
        }
        private float _timePassed;
        #endregion

        #region Properties

        internal Action<FCSPowerStates> OnPowerUpdate;
        internal Action<PowercellData> OnBatteryUpdate { get; set; }

        #endregion

        #region IFCSStorage Interface Properties

        public int GetContainerFreeSpace { get; }
        public bool IsFull => _powerBank.Battery.IsFull();

        #endregion

        #region Private Methods

        private void Update()
        {
            if (_mono == null || _mono.DeepDrillerContainer == null ||
                _mono.DisplayHandler == null || !_mono.IsConstructed ||
                !_initialized) return;

            ChargeSolarPanel();
            UpdatePowerState();
             
            _timePassed += DayNightCycle.main.deltaTime;
            
            if (_timePassed >= 1)
            {
                ConsumePower(CalculatePowerUsage());
                AttemptToChargeBattery();
                _timePassed = 0.0f;
            }
        }

        private void ConsumePower(float amount)
        {

            if (!_mono.IsOperational()) return;

            if (_powerRelay != null && _powerRelay.GetPower() >= amount)
            {
                _powerRelay.ConsumeEnergy(amount, out float amountConsumed);
            }
            else if (_powerBank.SolarPanel >= amount)
            {
                _powerBank.SolarPanel -= amount;
            }
            else if(_powerBank.Battery.Charge >= amount)
            {
                _powerBank.Battery.TakePower(amount);
                OnBatteryUpdate?.Invoke(_powerBank.Battery);
            }
        }

        private void AttemptToChargeBattery()
        {
            if (GetTotalCharge() <= 0) return;

            var amount = QPatch.Configuration.ChargePullAmount;

            if (_powerRelay != null && _powerRelay.GetPower() >= amount)
            {
                _powerRelay.ConsumeEnergy(amount, out float amountConsumed);
                _powerBank.Battery.ChargeBattery(amountConsumed);
                OnBatteryUpdate?.Invoke(_powerBank.Battery);
            }
            
            if (_powerBank.SolarPanel >= amount)
            {
                _powerBank.SolarPanel -= amount;
                _powerBank.Battery.ChargeBattery(amount);
                OnBatteryUpdate?.Invoke(_powerBank.Battery);
            }
        }
        
        private void UpdatePowerState()
        {
            if (PowerState == FCSPowerStates.Tripped) return;

            if (IsPowerAvailable() && _prevPowerState != FCSPowerStates.Powered)
            {
                PowerState = FCSPowerStates.Powered;
                _prevPowerState = FCSPowerStates.Powered;
            }
            else if (!IsPowerAvailable() && _prevPowerState == FCSPowerStates.Powered)
            {
                PowerState = FCSPowerStates.Unpowered;
                _prevPowerState = FCSPowerStates.Unpowered;
            }
        }

        private void ChargeSolarPanel()
        {
            if (IsSolarExtended())
            {
                _powerBank.SolarPanel = Mathf.Clamp(_powerBank.SolarPanel + GetRechargeScalar() * DayNightCycle.main.deltaTime * 0.25f * 10f, 0f, QPatch.Configuration.SolarCapacity);
            }
        }

        private float GetRechargeScalar()
        {
            return this.GetDepthScalar() * this.GetSunScalar();
        }

        private float GetDepthScalar()
        {
#if SUBNAUTICA
            float time = Mathf.Clamp01((MaxDepth - Ocean.main.GetDepthOf(base.gameObject)) / MaxDepth);
#elif BELOWZERO
    float time = Mathf.Clamp01((_maxDepth - Ocean.GetDepthOf(base.gameObject)) / _maxDepth);
#endif
            return _depthCurve.Evaluate(time);
        }

        private float GetSunScalar()
        {
            return DayNightCycle.main.GetLocalLightScalar();
        }
        
        private float CalculatePowerUsage()
        {
            float sum = _mono.UpgradeManager.Upgrades.Where(upgrade => upgrade.IsEnabled).Sum(upgrade => upgrade.PowerUsage);
            return _powerDraw + sum;
        }

        #endregion

        #region Internal Methods

        internal void Initialize(FCSDeepDrillerController mono)
        {
            _mono = mono;
            _powerDraw = QPatch.Configuration.PowerDraw;
            _depthCurve = new AnimationCurve();
            _depthCurve.AddKey(0f, 0f);
            _depthCurve.AddKey(0.4245796f, 0.5001081f);
            _depthCurve.AddKey(1f, 1f);
            _powerBank.Battery = new PowercellData { Charge = 0, Capacity = QPatch.Configuration.InternalBatteryCapacity };
            _initialized = true;
        }

        internal void LoadData(DeepDrillerSaveDataEntry data)
        {
            _powerBank.SolarPanel = data.PowerData.SolarPanel;
            _powerBank.Battery.Charge = data.PowerData.Battery.Charge;
            //PullPowerFromRelay = data.PullFromRelay; Disabled to until I decide to make it a toggle option

            if (data.SolarExtended)
            {
                ToggleSolarState();
            }

            PowerState = data.PowerState;
        }

        internal float GetPowerUsage()
        {
            return CalculatePowerUsage();
        }

        internal string GetSolarPowerData()
        {
            return $"Solar panel (sun: {Mathf.RoundToInt(GetRechargeScalar() * 100f)}% charge {Mathf.RoundToInt(_powerBank.SolarPanel)}/{Mathf.RoundToInt(QPatch.Configuration.SolarCapacity)})";
        }

        internal void ToggleSolarState()
        {
            _mono.AnimationHandler.SetBoolHash(_mono.SolarStateHash, !IsSolarExtended());
        }

        /// <summary>
        /// Add power to the battery from another power source
        /// </summary>
        /// <param name="powercell">The powercell  to pull power from</param>
        internal void ChargeBatteryFromPowercell(Battery powercell)
        {
            if (powercell.charge <= 0 || _powerBank.Battery.IsFull()) return;

            //Get the amount the battery needs
            var remainder = MathHelpers.GetRemainder(_powerBank.Battery.Charge, _powerBank.Battery.Capacity);

            //Get the minium amount of power from the battery and the power requirements
            var amount = Mathf.Min(powercell.charge, remainder);

            //Set the new battery value
            powercell.charge = Mathf.Max(0f, powercell.charge - amount);

            //Add the new battery amount
            _powerBank.Battery.ChargeBattery(amount);

            Inventory.main.Pickup(powercell.gameObject.GetComponent<Pickupable>());

            //Notify the drill of the change
            OnBatteryUpdate?.Invoke(_powerBank.Battery);
        }

        /// <summary>
        /// Gives the states of the solar panel
        /// </summary>
        /// <returns>Returns true if the solar panel is extended.</returns>
        internal bool IsSolarExtended()
        {
            return _mono.AnimationHandler.GetBoolHash(_mono.SolarStateHash);
        }

        /// <summary>
        /// Checks to see if all the conditions are met for being powered
        /// </summary>
        /// <returns>Returns true if power is available</returns>
        internal bool IsPowerAvailable()
        {
            if (GetTotalCharge() <= 0)
            {
                return false;
            }

            return true;
        }

        internal float GetTotalCharge()
        {
            return (_powerRelay?.GetPower() ?? 0) + _powerBank.Battery.Charge + _powerBank.SolarPanel;
        }
        
        internal void SetPowerRelay(PowerRelay powerRelay)
        {
            _powerRelay = powerRelay;
        }

        public float GetPowerUsagePerSecond()
        {
            return GetPowerUsage();
        }

        public float GetDevicePowerCharge()
        {
            return GetTotalCharge();
        }

        public float GetDevicePowerCapacity()
        {
            return QPatch.Configuration.InternalBatteryCapacity;
        }

        FCSPowerStates IFCSPowerManager.GetPowerState()
        {
            return GetPowerState();
        }

        public void TogglePowerState()
        {
            if (GetPowerState() != FCSPowerStates.Powered)
            {
                _mono.DisplayHandler.PowerOnDisplay();
                _mono.PowerManager.SetPowerState(FCSPowerStates.Powered);
            }
            else
            {
                _mono.DisplayHandler.PowerOffDisplay();
                _mono.PowerManager.SetPowerState(FCSPowerStates.Tripped);
            }
        }

        void IFCSPowerManager.SetPowerState(FCSPowerStates state)
        {
            SetPowerState(state);
        }

        public bool IsDevicePowerFull()
        {
            return IsFull;
        }

        public bool ModifyPower(float amount, out float consumed)
        {
            consumed = 0f;
            return false;
        }

        internal FCSPowerStates GetPowerState()
        {
            return PowerState;
        }

        internal void SetPowerState(FCSPowerStates state)
        {
            PowerState = state;
        }

        internal bool HasEnoughPowerToOperate()
        {
            if (!IsPowerAvailable()) return false;
            var amount = CalculatePowerUsage();

            return _powerRelay?.GetPower() >= amount || _powerBank.SolarPanel >= amount ||
                   _powerBank.Battery.Charge >= amount;
        }

        internal bool GetPullFromPowerRelay()
        {
            return PullPowerFromRelay;
        }

        /// <summary>
        /// Gets the data for saving the power data.
        /// </summary>
        /// <returns></returns>
        internal DeepDrillerPowerData SaveData()
        {
            return _powerBank;
        }

        internal bool IsTripped()
        {
            return PowerState == FCSPowerStates.Tripped;
        }

        #endregion

        #region IFCSStorage Interface

        public bool CanBeStored(int amount, TechType techType)
        {
#if SUBNAUTICA
            var equipType = CraftData.GetEquipmentType(techType);
#elif BELOWZERO
    var equipType = TechData.GetEquipmentType(techType);
#endif

            return equipType == EquipmentType.PowerCellCharger || techType == TechType.PowerCell ||
                   techType == TechType.PrecursorIonPowerCell;
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            var battery = item.item.gameObject.GetComponent<Battery>();
            ChargeBatteryFromPowercell(battery);
            return true;
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            if (!CanBeStored(1, pickupable.GetTechType()))
            {
                return false;
            }

            return !IsFull;
        }

        public bool IsAllowedToRemoveItems()
        {
            return false;
        }

        public Pickupable RemoveItemFromContainer(TechType techType, int amount)
        {
            throw new NotImplementedException();
        }

        public Dictionary<TechType, int> GetItemsWithin()
        {
            throw new NotImplementedException();
        }

        public Action<int, int> OnContainerUpdate { get; set; }
        public bool ContainsItem(TechType techType)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}