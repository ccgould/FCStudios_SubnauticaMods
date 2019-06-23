using FCSCommon.Utilities;
using FCSCommon.Utilities.Enums;
using FCSPowerStorage.Configuration;
using FCSPowerStorage.Model;
using FCSPowerStorage.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FCSPowerStorage.Managers
{
    internal class FCSPowerManager : MonoBehaviour, IPowerInterface
    {
        private List<PowercellModel> _powerCells = new List<PowercellModel>(4);
        private float _charge;
        private PowerRelay _connectedRelay;
        private CustomBatteryController _mono;
        internal FCSPowerStates PowerState;
        private bool _hasBreakerTripped;
        private FCSPowerStates _previousPowerState;

        internal void Initialize(CustomBatteryController mono)
        {
            _mono = mono;

            StartCoroutine(UpdatePowerRelay());

            // Create four batteries
            for (int i = 0; i < 4; i++)
            {
                PowercellModel powercell = new PowercellModel();
                powercell.SetSlot(i);
                _powerCells.Add(powercell);
            }

            InvokeRepeating("Recharge", 0, 1);
        }

        private void OnDestroy()
        {
            CancelInvoke("Recharge");
            StopCoroutine(UpdatePowerRelay());
            if (!(_connectedRelay != null))
                return;
            QuickLogger.Debug("RemoveInboundPower");
            _connectedRelay.RemoveInboundPower(this);
        }

        //TODO Modify Recharging
        private void Recharge()
        {
            if (!_hasBreakerTripped)
            {
                if (ChargeMode == PowerToggleStates.TrickleMode)
                {
                    int num1 = 0;
                    bool flag = false;
                    float amount = 0.0f;
                    bool charging = false;
                    PowerRelay relay = PowerSource.FindRelay(transform);

                    if (relay != null)
                    {
                        if (_charge < LoadData.BatteryConfiguration.Capacity)
                        {
                            ++num1;
                            float num2 = DayNightCycle.main.deltaTime * LoadData.BatteryConfiguration.ChargeSpeed *
                                         LoadData.BatteryConfiguration.Capacity;
                            if (_charge + num2 > LoadData.BatteryConfiguration.Capacity)
                                num2 = LoadData.BatteryConfiguration.Capacity - _charge;
                            amount += num2;
                        }

                        UWE.Utils.Assert(amount >= 0.0, "Charger must request positive amounts", this);
                        float amountConsumed = 0.0f;
                        if (amount > 0.0 && relay.GetPower() > amount)
                        {
                            flag = true;
                            relay.ConsumeEnergy(amount, out amountConsumed);
                        }


                        UWE.Utils.Assert(amountConsumed >= 0.0, "Charger must result in positive amounts", this);
                        if (amountConsumed > 0.0)
                        {
                            charging = true;
                            float num2 = amountConsumed / num1;

                            if (_charge < (double)LoadData.BatteryConfiguration.Capacity)
                            {
                                float num3 = num2;
                                float num4 = LoadData.BatteryConfiguration.Capacity - _charge;
                                if (num3 > (double)num4)
                                    num3 = num4;
                                ChargeBatteries(num3);
                            }
                        }
                    }
                }
                else
                {

                    int num1 = 0;
                    bool flag = false;
                    float amount = 0.0f;
                    bool charging = false;
                    PowerRelay relay = PowerSource.FindRelay(transform);

                    if (relay != null)
                    {
                        if (StoredPower < LoadData.BatteryConfiguration.Capacity)
                        {
                            ++num1;
                            float num2 = DayNightCycle.main.deltaTime * LoadData.BatteryConfiguration.ChargeSpeed *
                                         LoadData.BatteryConfiguration.Capacity;
                            if (StoredPower + num2 > LoadData.BatteryConfiguration.Capacity)
                                num2 = LoadData.BatteryConfiguration.Capacity - _charge;
                            amount += num2;
                        }

                        UWE.Utils.Assert(amount >= 0.0, "Charger must request positive amounts", this);
                        float amountConsumed = 0.0f;
                        if (amount > 0.0 && relay.GetPower() > amount)
                        {
                            flag = true;
                            relay.ConsumeEnergy(amount, out amountConsumed);
                        }


                        UWE.Utils.Assert(amountConsumed >= 0.0, "Charger must result in positive amounts", this);
                        if (amountConsumed > 0.0)
                        {
                            charging = true;
                            float num2 = amountConsumed / num1;

                            if (StoredPower < (double)LoadData.BatteryConfiguration.Capacity)
                            {
                                float num3 = num2;
                                float num4 = LoadData.BatteryConfiguration.Capacity - StoredPower;
                                if (num3 > (double)num4)
                                    num3 = num4;
                                StoredPower += num3;
                            }

                        }
                    }
                }
            }
        }

        internal void ConsumePower(float amount)
        {
            foreach (PowercellModel powerCell in _powerCells)
            {
                if (powerCell.GetPower() >= amount)
                {
                    powerCell.Consume(amount);
                    break;
                }
            }
        }

        internal void ChargeBatteries(float amount)
        {
            foreach (PowercellModel powerCell in _powerCells)
            {
                if (powerCell.GetIsFull()) continue;
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
            //TODO edit save to work with this new model
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

            if (_mono.ChargeMode == PowerToggleStates.ChargeMode &&
                PowerState == FCSPowerStates.Unpowered) return _charge;

            foreach (var powerCell in _powerCells)
            {
                _charge += powerCell.GetPower();
            }

            return _charge;
        }

        public float GetMaxPower()
        {
            return LoadData.BatteryConfiguration.Capacity;
        }

        public bool ModifyPower(float amount, out float modified)
        {
            modified = 0f;

            bool result;
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
                    if (ChargeMode == PowerToggleStates.TrickleMode)
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

            return result;
        }

        public PowerToggleStates ChargeMode { get; set; }

        public bool HasInboundPower(IPowerInterface powerInterface)
        {
            return false;
        }

        public bool GetInboundHasSource(IPowerInterface powerInterface)
        {
            return false;
        }
        #endregion

        //TODO Change name is needed
        public void StorePower()
        {
            _charge = 0.0f;
        }

        /// <summary>
        /// Sets the breaker state.
        /// </summary>
        /// <param name="hasTripped"></param>
        internal void SetBreakerTrip(bool hasTripped)
        {
            //Lets get the current powerstate if not unpowered
            if (PowerState != FCSPowerStates.Unpowered)
            {
                _previousPowerState = PowerState;
            }

            // Trip the breaker
            _hasBreakerTripped = hasTripped;

            //Set the power state;
            PowerState = hasTripped ? FCSPowerStates.Unpowered : _previousPowerState;
        }

        internal PowercellModel GetPowerCell(int slot)
        {
            return _powerCells[slot];
        }

        internal bool GetBreakerTrip()
        {
            return _hasBreakerTripped;
        }
    }
}
