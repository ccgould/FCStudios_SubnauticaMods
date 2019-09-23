using FCSCommon.Enums;
using FCSCommon.Utilities;
using FCSCommon.Utilities.Enums;
using FCSPowerStorage.Configuration;
using FCSPowerStorage.Models;
using FCSPowerStorage.Mono;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FCSPowerStorage.Managers
{
    internal class FCSPowerManager : MonoBehaviour, IPowerInterface
    {
        private readonly List<PowercellModel> _powerCells = new List<PowercellModel>(4);
        private float _charge;
        private PowerRelay _connectedRelay;
        private FCSPowerStorageController _mono;
        private FCSPowerStates _powerState;
        private readonly int _savedBattery = -1;
        private PowercellModel _battery;
        private PowerToggleStates _chargeMode;
        private float _passedTime;

        internal void Initialize(FCSPowerStorageController mono)
        {
            _mono = mono;

            StartCoroutine(UpdatePowerRelay());

            // Create four batteries
            for (int i = 0; i < _mono.BatteryCount; i++)
            {
                PowercellModel powercell = new PowercellModel();
                powercell.SetSlot(i);
                powercell.SetName(string.Empty); //Let it create its own name
                _powerCells.Add(powercell);
            }

            if (_savedBattery == -1)
            {
                _battery = _powerCells[0];
            }
        }

        private void OnDestroy()
        {
            foreach (var powerCell in _powerCells)
            {
                powerCell.Drain();
            }

            _powerCells.Clear();
            CancelInvoke("Recharge");
            StopCoroutine(UpdatePowerRelay());
            if (!(_connectedRelay != null))
                return;
            QuickLogger.Debug("RemoveInboundPower");
            _connectedRelay.RemoveInboundPower(this);
        }

        private void Update()
        {
            Recharge();
            CheckIfUnpowered();
        }

        private void CheckIfUnpowered()
        {
            if (_powerState == FCSPowerStates.Unpowered)
            {
                _mono.AnimationManager.SetIntHash(_mono.StateHash, 4);
            }
        }

        private void Recharge()
        {
            if (DayNightCycle.main == null || _mono.Manager == null || GetPowerSum() >= GetMaxPower()) return;

            if (_mono.GetBaseDrainProtection())
            {
                if (GetBasePower() <= _mono.GetBasePowerProtectionGoal())
                    return;
            }

            _passedTime += DayNightCycle.main.deltaTime;

            if (_powerState != FCSPowerStates.Unpowered && _chargeMode == PowerToggleStates.ChargeMode && _passedTime >= 2f)
            {
                float num2 = 0f;
                _passedTime = num2;

                float num3 = DayNightCycle.main.deltaTime * LoadData.BatteryConfiguration.ChargeSpeed * LoadData.BatteryConfiguration.Capacity;
                if (_charge + num3 > LoadData.BatteryConfiguration.Capacity)
                {
                    num3 = LoadData.BatteryConfiguration.Capacity - _charge;
                }
                num2 += num3;

                float num4 = 0f;

                if (num2 > 0f && _connectedRelay.GetPower() > num2)
                {
                    _connectedRelay.ConsumeEnergy(num2, out num4);
                }

                if (num4 > 0f)
                {
                    ChargeBatteries(num4);
                }
            }
        }

        internal int GetBasePower()
        {
            return _connectedRelay != null ? Mathf.RoundToInt(_connectedRelay.GetPower()) : 0;
        }

        internal void ConsumePower(float amount)
        {
            if (_battery == null)
            {
                QuickLogger.Error("Battery is null cannot consume power!");
                return;
            }

            var slot = _battery.GetSlot();

            var power = _battery.GetPower();

            if (!Mathf.Approximately(power, 0) && power >= amount)
            {
                _battery.Consume(amount);
                return;
            }

            //try next battery
            if (slot - 1 < 0) return; // we are using the firs battery no need to check
            var powercell = _powerCells[slot - 1];

            power = powercell.GetPowerValue();

            if (!Mathf.Approximately(power, 0) && power >= amount)
            {
                _battery = powercell;
                _battery.Consume(amount);
            }
        }

        internal void ChargeBatteries(float amount)
        {
            foreach (PowercellModel powerCell in _powerCells)
            {
                if (powerCell.GetIsFull()) continue;

                if (_battery != null && _battery != powerCell)
                {
                    _battery = powerCell;
                }

                powerCell.Charge(amount);
                break;
            }
        }

        /// <summary>
        /// Loads the save data into the power manager
        /// </summary>
        /// <param name="save"></param>
        internal void LoadSave(SaveData save)
        {
            SetPowerState(save.PowerState);
            SetChargeMode(save.ChargeMode);
            _mono.SetAutoActivate(save.AutoActivate);

            for (int i = 0; i < _mono.BatteryCount; i++)
            {
                var power = save.Batteries[i].Power;
                QuickLogger.Debug($"Loading Power From Save : {power}");
                _powerCells[i].SetPower(power);
            }
        }

        /// <summary>
        /// Gets all the battery information to be saved
        /// </summary>
        /// <returns>A list of all batteries</returns>
        internal List<PowercellModel> SaveData()
        {
            return _powerCells;
        }

        /// <summary>
        /// Gets the percentage of the battery in the current slot
        /// </summary>
        /// <param name="slot">The battery slot to get the percentage from</param>
        /// <returns>a float of the percentage</returns>
        internal float GetPercentage(int slot)
        {
            return _powerCells[slot].GetPercentage();
        }

        internal float StoredPower { get; set; }

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
                    _connectedRelay.AddInboundPower(this);
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

        #region IPowerInterface
        public float GetPower()
        {
            _charge = 0f;

            if (_chargeMode == PowerToggleStates.ChargeMode ||
                _powerState == FCSPowerStates.Unpowered) return 0f;

            _charge = GetPowerSum();

            return _charge;
        }

        internal float GetPowerSum()
        {
            float sum = 0;

            foreach (var powerCell in _powerCells)
            {
                sum += powerCell.GetPower();
            }

            return Mathf.Clamp(sum, 0, LoadData.BatteryConfiguration.Capacity);
        }

        public float GetMaxPower()
        {
            return LoadData.BatteryConfiguration.Capacity;
        }

        public bool ModifyPower(float amount, out float modified)
        {

            bool result = false;
            modified = 0f;

            if (_powerState != FCSPowerStates.Unpowered && _chargeMode == PowerToggleStates.TrickleMode)
            {
                if (amount >= 0f)
                {
                    result = (amount <= LoadData.BatteryConfiguration.Capacity - _charge);
                    modified = Mathf.Min(amount, LoadData.BatteryConfiguration.Capacity - _charge);
                    ChargeBatteries(modified);
                }
                else
                {
                    result = (_charge >= -amount);
                    if (GameModeUtils.RequiresPower())
                    {
                        if (_chargeMode == PowerToggleStates.TrickleMode)
                        {
                            modified = -Mathf.Min(-amount, _charge);
                            ConsumePower(modified);
                        }
                    }
                    else
                    {
                        modified = amount;
                    }
                }
            }

            return result;
        }

        public bool HasInboundPower(IPowerInterface powerInterface)
        {
            return false;
        }

        public bool GetInboundHasSource(IPowerInterface powerInterface)
        {
            return false;
        }
        #endregion

        public void StorePower()
        {
            _charge = 0.0f;
        }

        internal PowercellModel GetPowerCell(int slot)
        {
            return _powerCells[slot];
        }

        public float GetCharge()
        {
            return _charge;
        }

        internal List<PowercellModel> Save()
        {
            return _powerCells;
        }

        internal void SetChargeMode(PowerToggleStates savedDataChargeMode)
        {
            QuickLogger.Debug($"Toggle State {savedDataChargeMode}");
            _chargeMode = savedDataChargeMode;

            if (_powerState == FCSPowerStates.Unpowered) return;

            switch (savedDataChargeMode)
            {
                case PowerToggleStates.TrickleMode:
                    _mono.SystemLightManager.ChangeSystemLights(SystemLightState.Default);
                    _mono.AnimationManager.SetBoolHash(_mono.ToggleHash, false);
                    break;
                case PowerToggleStates.ChargeMode:
                    _mono.SystemLightManager.ChangeSystemLights(SystemLightState.Warning);
                    _mono.AnimationManager.SetBoolHash(_mono.ToggleHash, true);
                    break;
            }
        }

        internal PowerToggleStates GetChargeMode()
        {
            return _chargeMode;
        }

        internal void SetPowerState(FCSPowerStates state)
        {
            QuickLogger.Debug($"Current State: {state}", true);

            switch (state)
            {
                //Reset The systemLight
                case FCSPowerStates.Powered:
                    _mono.AnimationManager.SetIntHash(_mono.StateHash, 1);
                    _mono.SystemLightManager.RestoreStoredState();
                    break;

                case FCSPowerStates.Unpowered:
                    _mono.SystemLightManager.StoreCurrentState();
                    _mono.SystemLightManager.ChangeSystemLights(SystemLightState.Unpowered);
                    break;
            }

            _powerState = state;
        }

        internal FCSPowerStates GetPowerState()
        {
            return _powerState;
        }

    }
}
