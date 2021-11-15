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
using FCS_ProductionSolutions.Mods.DeepDriller.Configuration;
using FCS_ProductionSolutions.Mods.DeepDriller.Interfaces;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono
{
    internal class FCSDeepDrillerPowerHandler : FCSPowerManager, IFCSStorage, IPowerManager
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
        private const float SolarPanelRateControl = 25f;
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
                !IsInitialized || _powerBank?.Battery == null) return;

            ChargeSolarPanel();
            UpdatePowerState();
             
            _timePassed += DayNightCycle.main.deltaTime;
            
            if (_timePassed >= 1)
            {
                AttemptToChargeBattery();
                ConsumePower(CalculatePowerUsage());
                _timePassed = 0.0f;
            }
        }

        private void ConsumePower(float amount)
        {
            if (!_mono.IsOperational || !_mono.OreGenerator.GetIsDrilling()) return;
            
            _powerBank.Battery.RemoveCharge(amount);
            QuickLogger.Debug($"Remove {amount} from battery",true);
            OnBatteryUpdate?.Invoke(_powerBank.Battery);
        }

        private void AttemptToChargeBattery()
        {
            if (/*GetTotalCharge() <= 0 ||*/ _powerBank.Battery.IsFull()) return;

            var amount = _powerBank.Battery.GetCapacity() - _powerBank.Battery.GetCharge();
            
            ModifyPower(_powerBank.Thermal, amount, out var thermalConsumed);
            amount -= thermalConsumed;
            _powerBank.Battery.AddCharge(thermalConsumed);

            if (amount <= 0)
            {
                OnBatteryUpdate?.Invoke(_powerBank.Battery);
                return;
            }

            ModifyPower(_powerBank.SolarPanel, amount, out var solarConsumed);
            _powerBank.Battery.AddCharge(solarConsumed);

            if (_powerBank.Battery.IsFull() || amount <= 0)
            {
                OnBatteryUpdate?.Invoke(_powerBank.Battery);
                return;
            }

            if (_powerRelay != null && _powerRelay.GetPower() > 0)
            {
                _powerRelay.ConsumeEnergy(amount, out float amountConsumed);
                _powerBank.Battery.AddCharge(amountConsumed);
                amount -= amountConsumed;
            }

            if (amount <= 0)
            {
                OnBatteryUpdate?.Invoke(_powerBank.Battery);
                return;
            }

            if (_mono.Manager != null)
            {
                _mono.Manager.Habitat.powerRelay.ConsumeEnergy(amount, out float amountConsumed2);
                _powerBank.Battery.AddCharge(amountConsumed2);
                amount -= amountConsumed2;
            }



            OnBatteryUpdate?.Invoke(_powerBank.Battery);
        }


        private void ModifyPower(PowercellData devicePower, float amount, out float consumed)
        {
            consumed = 0;
            //First find out how much power the device has
            if (devicePower.GetCharge() <= 0) return;

            if (devicePower.GetCharge() <= amount)
            {
                consumed = devicePower.GetCharge();
                devicePower.Flush();
                return;
            }

            consumed = devicePower.GetCharge() - amount;
            devicePower.RemoveCharge(consumed);
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

        #region Solar Panel

        private void ChargeSolarPanel()
        {
            _powerBank.SolarPanel.AddCharge(Mathf.Clamp(GetRechargeScalar() * DayNightCycle.main.deltaTime * 0.25f * SolarPanelRateControl, 0f, _powerBank.SolarPanel.GetCapacity()));
        }

        internal float GetRechargeScalar()
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

        #endregion

        private float CalculatePowerUsage()
        {
            float sum = _mono.UpgradeManager.Upgrades.Where(upgrade => upgrade.IsEnabled).Sum(upgrade => upgrade.PowerUsage);
            return _powerDraw + sum;
        }

        #endregion

        #region Internal Methods

        internal void Initialize(FCSDeepDrillerController mono)
        {
            if (IsInitialized) return;
            _mono = mono;

            var thermal = gameObject.EnsureComponent<FCSDeepDrillerThermalController>();
            thermal.Initialize(_powerBank);

            _powerDraw = QPatch.Configuration.DDPowerDraw + 
                         QPatch.Configuration.DDOrePerDayUpgradePowerUsage + 12 * QPatch.Configuration.DDOreReductionValue;
            _depthCurve = new AnimationCurve();
            _depthCurve.AddKey(0f, 0f);
            _depthCurve.AddKey(0.4245796f, 0.5001081f);
            _depthCurve.AddKey(1f, 1f);
            IsInitialized = true;
        }

        internal void LoadData(DeepDrillerSaveDataEntry data)
        {
            if (data?.PowerData?.Battery == null) return;

            QuickLogger.Message($"Solar Panel: {data.PowerData.SolarPanel} || Battery: {data.PowerData.Battery.GetCharge()}",true);


            if (_powerBank?.Battery == null)
            {
                _powerBank = new DeepDrillerPowerData();
            }

            _powerBank.Thermal = data.PowerData.Thermal;
            _powerBank.SolarPanel = data.PowerData.SolarPanel;
            _powerBank.Battery.AddCharge(data.PowerData.Battery.GetCharge());
            //PullPowerFromRelay = data.PullFromRelay; Disabled to until I decide to make it a toggle option
            PowerState = data.PowerState;
        }

        internal float GetPowerUsage()
        {
            return CalculatePowerUsage();
        }

        internal string GetSolarPowerData()
        {
            return $"Solar panel (sun: {Mathf.RoundToInt(GetRechargeScalar() * 100f)}% charge {Mathf.RoundToInt(_powerBank.SolarPanel.GetCharge())}/{Mathf.RoundToInt(QPatch.Configuration.DDSolarCapacity)})";
        }

        /// <summary>
        /// Add power to the battery from another power source
        /// </summary>
        /// <param name="powercell">The powercell  to pull power from</param>
#if SUBNAUTICA_STABLE
        internal void ChargeBatteryFromPowercell(Battery powercell)
        {
            if (powercell.charge <= 0 || _powerBank.Battery.IsFull())
            {
                Inventory.main.Pickup(powercell.gameObject.GetComponent<Pickupable>());
                return;
            }

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
#else
        internal IEnumerator ChargeBatteryFromPowercell(Battery powercell)
        {
            TaskResult<bool> taskResult = new TaskResult<bool>();

            if (powercell.charge <= 0 || _powerBank.Battery.IsFull()) 
            {
                yield return Inventory.main.PickupAsync(powercell.gameObject.GetComponent<Pickupable>(),taskResult);
                yield break;
            }

            //Get the amount the battery needs
            var remainder = MathHelpers.GetRemainder(_powerBank.Battery.GetCharge(), _powerBank.Battery.GetCapacity());

            //Get the minium amount of power from the battery and the power requirements
            var amount = Mathf.Min(powercell.charge, remainder);

            //Set the new battery value
            powercell.charge = Mathf.Max(0f, powercell.charge - amount);

            //Add the new battery amount
            _powerBank.Battery.AddCharge(amount);

            yield return Inventory.main.PickupAsync(powercell.gameObject.GetComponent<Pickupable>(), taskResult);

            //Notify the drill of the change
            OnBatteryUpdate?.Invoke(_powerBank.Battery);
            yield break;
        }
#endif

        /// <summary>
        /// Checks to see if all the conditions are met for being powered
        /// </summary>
        /// <returns>Returns true if power is available</returns>
        public bool IsPowerAvailable()
        {
            return !(GetTotalCharge() <= 0);
        }

        internal float GetTotalCharge()
        {
            if (_powerBank?.Battery == null) return 0f;
            return (_powerRelay?.GetPower() ?? 0) + _powerBank.Battery.GetCharge() + _powerBank.SolarPanel.GetCharge() + _powerBank.Thermal.GetCharge();
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
            return QPatch.Configuration.DDInternalBatteryCapacity;
        }
        
        public override void TogglePowerState()
        {
            if (GetPowerState() == FCSPowerStates.Tripped)
            {
                _mono.DisplayHandler.TurnOnDisplay();
                _mono.DeepDrillerPowerManager.SetPowerState(FCSPowerStates.Powered);
            }
            else
            {
                _mono.DisplayHandler.TurnOffDisplay();
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

           return _powerRelay?.GetPower() + _powerBank.SolarPanel.GetCharge() + _powerBank.Thermal.GetCharge() + _powerBank.Battery.GetCharge() >= amount;
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
#if SUBNAUTICA_STABLE
            ChargeBatteryFromPowercell(battery);
#else
            StartCoroutine(ChargeBatteryFromPowercell(battery));
#endif
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

        public Pickupable RemoveItemFromContainer(TechType techType)
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
        public bool IsInitialized { get; set; }

        public bool ContainsItem(TechType techType)
        {
            return false;
        }

        public ItemsContainer ItemsContainer { get; set; }
        public int StorageCount()
        {
            return ItemsContainer?.count ?? 0;
        }

        #endregion

        public float GetSourcePower(DeepDrillerPowerSources powerSource)
        {
            switch (powerSource)
            {
                case DeepDrillerPowerSources.PowerRelay:
                    return _powerRelay?.GetPower() ?? 0f;
                case DeepDrillerPowerSources.Solar:
                    return _powerBank.SolarPanel.GetCharge();
                case DeepDrillerPowerSources.Thermal:
                    return _powerBank.Thermal.GetCharge();
                default:
                    return 0f;
            }
        }

        public float GetSourcePowerCapacity(DeepDrillerPowerSources powerSource)
        {
            switch (powerSource)
            {
                case DeepDrillerPowerSources.PowerRelay:
                    return _powerRelay?.GetMaxPower() ?? 0f;
                case DeepDrillerPowerSources.Solar:
                    return _powerBank.SolarPanel.GetCapacity();
                case DeepDrillerPowerSources.Thermal:
                    return _powerBank.Thermal.GetCapacity();
                default:
                    return 0f;
            }
        }
    }

    internal enum DeepDrillerPowerSources
    {
        None,
        Thermal,
        Solar,
        PowerRelay
    }
}