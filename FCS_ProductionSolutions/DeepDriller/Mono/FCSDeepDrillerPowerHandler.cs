using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Managers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.DeepDriller.Configuration;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_ProductionSolutions.DeepDriller.Mono
{
    internal class FCSDeepDrillerPowerHandler : FCSPowerManager, IFCSStorage
    {
        #region Private Fields

        private const bool PullPowerFromRelay = true; //Setting this to true by default until I feel like making it a toggle option
        private FCSPowerStates _powerState;
        private AnimationCurve _depthCurve;
        private DeepDrillerPowerData _powerBank = new DeepDrillerPowerData();
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
                !_initialized || _powerBank?.Battery == null) return;

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

            if (!_mono.IsOperational) return;

            if (_powerRelay != null && _powerRelay.GetPower() >= amount)
            {
                _powerRelay.ConsumeEnergy(amount, out float amountConsumed);
            }
            else if (_powerBank.SolarPanel >= amount)
            {
                _powerBank.SolarPanel -= amount;
            }
            else if(_powerBank.Battery.GetCharge() >= amount)
            {
                _powerBank.Battery.RemoveCharge(amount);
                OnBatteryUpdate?.Invoke(_powerBank.Battery);
            }
        }

        private void AttemptToChargeBattery()
        {
            if (GetTotalCharge() <= 0) return;

            var amount = QPatch.DeepDrillerMk2Configuration.ChargePullAmount;

            if (_powerRelay != null && _powerRelay.GetPower() >= amount)
            {
                _powerRelay.ConsumeEnergy(amount, out float amountConsumed);
                _powerBank.Battery.AddCharge(amountConsumed);
                OnBatteryUpdate?.Invoke(_powerBank.Battery);
            }
            
            if (_powerBank.SolarPanel >= amount)
            {
                _powerBank.SolarPanel -= amount;
                _powerBank.Battery.AddCharge(amount);
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
                PowerState = FCSPowerStates.UnPowered;
                _prevPowerState = FCSPowerStates.UnPowered;
            }
        }

        private void ChargeSolarPanel()
        {
            if (IsSolarExtended())
            {
                _powerBank.SolarPanel = Mathf.Clamp(_powerBank.SolarPanel + GetRechargeScalar() * DayNightCycle.main.deltaTime * 0.25f * 10f, 0f, QPatch.DeepDrillerMk2Configuration.SolarCapacity);
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
            _powerDraw = QPatch.DeepDrillerMk2Configuration.PowerDraw;
            _depthCurve = new AnimationCurve();
            _depthCurve.AddKey(0f, 0f);
            _depthCurve.AddKey(0.4245796f, 0.5001081f);
            _depthCurve.AddKey(1f, 1f);
            _initialized = true;
        }

        internal void LoadData(DeepDrillerMk2SaveDataEntry data)
        {
            if (data?.PowerData?.Battery == null) return;

            QuickLogger.Message($"Solar Panel: {data.PowerData.SolarPanel} || Battery: {data.PowerData.Battery.GetCharge()}",true);


            if (_powerBank?.Battery == null)
            {
                _powerBank = new DeepDrillerPowerData();
            }

            _powerBank.SolarPanel = data.PowerData.SolarPanel;
            _powerBank.Battery.AddCharge(data.PowerData.Battery.GetCharge());
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
            return $"Solar panel (sun: {Mathf.RoundToInt(GetRechargeScalar() * 100f)}% charge {Mathf.RoundToInt(_powerBank.SolarPanel)}/{Mathf.RoundToInt(QPatch.DeepDrillerMk2Configuration.SolarCapacity)})";
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
            var remainder = MathHelpers.GetRemainder(_powerBank.Battery.GetCharge(), _powerBank.Battery.GetCapacity());

            //Get the minium amount of power from the battery and the power requirements
            var amount = Mathf.Min(powercell.charge, remainder);

            //Set the new battery value
            powercell.charge = Mathf.Max(0f, powercell.charge - amount);

            //Add the new battery amount
            _powerBank.Battery.AddCharge(amount);

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
            return !(GetTotalCharge() <= 0);
        }

        internal float GetTotalCharge()
        {
            if (_powerBank?.Battery == null) return 0f;
            return (_powerRelay?.GetPower() ?? 0) + _powerBank.Battery.GetCharge() + _powerBank.SolarPanel;
        }
        
        internal void SetPowerRelay(PowerRelay powerRelay)
        {
            _powerRelay = powerRelay;
        }

        public override float GetPowerUsagePerSecond()
        {
            return GetPowerUsage();
        }

        public override float GetDevicePowerCharge()
        {
            return GetTotalCharge();
        }

        public override float GetDevicePowerCapacity()
        {
            return QPatch.DeepDrillerMk2Configuration.InternalBatteryCapacity;
        }
        
        public override void TogglePowerState()
        {
            if (GetPowerState() == FCSPowerStates.Tripped)
            {
                _mono.DisplayHandler.PowerOnDisplay();
                _mono.DeepDrillerPowerManager.SetPowerState(FCSPowerStates.Powered);
            }
            else
            {
                _mono.DisplayHandler.PowerOffDisplay();
                _mono.DeepDrillerPowerManager.SetPowerState(FCSPowerStates.Tripped);
            }
        }

        public override bool IsDevicePowerFull()
        {
            return IsFull;
        }

        public override bool ModifyPower(float amount, out float consumed)
        {
            consumed = 0f;
            return false;
        }

        public override FCSPowerStates GetPowerState()
        {
            return PowerState;
        }

        public override void SetPowerState(FCSPowerStates state)
        {
            PowerState = state;
        }

        internal bool HasEnoughPowerToOperate()
        {
            if (!IsPowerAvailable()) return false;
            var amount = CalculatePowerUsage();

            return _powerRelay?.GetPower() >= amount || _powerBank.SolarPanel >= amount ||
                   _powerBank.Battery.GetCharge() >= amount;
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
            return null;
        }

        public Dictionary<TechType, int> GetItemsWithin()
        {
            return null;
        }

        public Action<int, int> OnContainerUpdate { get; set; }
        public Action<FcsDevice, TechType> OnContainerAddItem { get; set; }
        public Action<FcsDevice, TechType> OnContainerRemoveItem { get; set; }

        public bool ContainsItem(TechType techType)
        {
            return false;
        }

        #endregion
    }
}