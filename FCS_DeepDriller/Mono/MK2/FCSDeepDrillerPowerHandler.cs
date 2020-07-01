using System;
using System.Collections.Generic;
using FCS_DeepDriller.Configuration;
using FCS_DeepDriller.Helpers;
using FCS_DeepDriller.Mono.MK2;
using FCSCommon.Enums;
using FCSCommon.Utilities;
using FCSTechFabricator.Interfaces;
using RadicalLibrary;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FCS_DeepDriller.Mono.MK1
{
    internal class FCSDeepDrillerPowerHandler : MonoBehaviour, IFCSStorage
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
        private float _timePassed = 0.0f;
        private AnimationCurve _depthCurve;
        private readonly DeepDrillerPowerData _powerBank = new DeepDrillerPowerData();
        private bool _prevPowerState;
        internal Action<FCSPowerStates> OnPowerUpdate;
        private bool _initialized;
        private float _powerDraw;
        private PowerRelay _powerRelay;
        private PowerRelay _connectedRelay;
        private bool _pullPowerFromRelay;
        public Action<PowercellData> OnBatteryUpdate { get; set; }
        public Action OnUsageChange { get; set; }

        private void Update()
        {
            if (_mono == null || _mono.DeepDrillerContainer == null || 
                _mono.DisplayHandler == null || !_mono.IsConstructed || 
                !_initialized) return;
            
            ChargeBattery();
            UpdatePowerState();

            _timePassed += DayNightCycle.main.deltaTime;
            if (_timePassed >= 1)
            {
                TakePower(CalculatePowerUsage());
                _timePassed = 0.0f;
            }
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

        private void ChargeBattery()
        {
            

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

        private void ChargeSolarPanel()
        {
            _powerBank.SolarPanel = Mathf.Clamp(_powerBank.SolarPanel + GetRechargeScalar() * DayNightCycle.main.deltaTime * 0.50f * 5f, 0f, QPatch.Configuration.SolarCapacity);
        }

        internal void Initialize(FCSDeepDrillerController mono)
        {
            _mono = mono;
            _powerDraw = QPatch.Configuration.PowerDraw;
            _depthCurve = new AnimationCurve();
            _depthCurve.AddKey(0f, 0f);
            _depthCurve.AddKey(0.4245796f, 0.5001081f);
            _depthCurve.AddKey(1f, 1f);
            PowerState = FCSPowerStates.Powered;
            _powerBank.Battery = new PowercellData {Charge = 0, Capacity = 3000};
            InvokeRepeating(nameof(ChargeSolarPanel),0.5f,0.5f);
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

            if(data.SolarExtended)
            {
                ToggleSolarState();
            }

            PowerState = data.PowerState;
        }

        internal float GetCharge()
        {
            return _powerBank.Battery.Charge;
        }

        internal float CalculatePowerUsage()
        {
            return _powerDraw;
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
           return _mono.AnimationHandler.GetBoolHash(_mono.SolarStateHash);
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

        internal void TakePower(float amount)
        {
            if (PowerState != FCSPowerStates.Powered || _powerRelay == null || _powerBank == null) return;

            if (IsSolarExtended() && _powerBank.SolarPanel >= amount)
            {
                _powerBank.SolarPanel -= amount;
            }
            else if (_pullPowerFromRelay && _powerRelay.GetPower() > 0)
            {
                _powerRelay.ConsumeEnergy(amount, out var amountConsumed);
                QuickLogger.Debug($"Energy Consumed: {amountConsumed}");
            }
            else
            {
                _powerBank.Battery.TakePower(amount);
            }

            //Charge Battery

            if (_pullPowerFromRelay && _powerRelay.GetPower() > 0)
            {
                _powerRelay.ConsumeEnergy(amount / 2, out var amountConsumed);
                _powerBank.Battery.ChargeBattery(amountConsumed);
                QuickLogger.Debug($"Energy Consumed: {amountConsumed}");
                OnBatteryUpdate?.Invoke(_powerBank.Battery);
            }
            else if (IsSolarExtended())
            {
                _powerBank.Battery.ChargeBattery(_powerBank.SolarPanel/2);
            }




            OnBatteryUpdate?.Invoke(_powerBank.Battery);
        }

        //TODO Connect to base

        internal void UpdatePowerUsage(float amount)
        {
            OnUsageChange?.Invoke();
        }
        public float GetPowerUsage()
        {
            return _powerDraw;
        }

        public int GetContainerFreeSpace { get; }
        public bool IsFull => _powerBank.Battery.IsFull();
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

            if (IsFull)
            {
                return false;
            }

            return true;
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

        public void SetPowerRelay(PowerRelay powerRelay)
        {
            _powerRelay = powerRelay;
        }
    }
}