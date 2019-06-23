using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSPowerStorage.Buildables;
using UnityEngine;
using UnityEngine.UI;

namespace FCSPowerStorage.Models
{
    /// <summary>
    /// This class defines a power cell in PowerStorage
    /// </summary>
    internal class PowercellModel
    {
        private float _power;
        private string _name;
        private int _slot;
        private readonly float _capacity = LoadData.BatteryConfiguration.Capacity / 4;
        private Image _batteryStatus1Bar;
        private Text _batteryStatus1Percentage;
        private bool IsFull => _power <= _capacity;
        private GameObject _meterDisplay;

        /// <summary>
        /// Sets the amount of power available in cell
        /// </summary>
        /// <param name="amount"></param>
        internal void SetPower(float amount)
        {
            _power = amount;
        }

        /// <summary>
        /// Set the name of the cell
        /// </summary>
        /// <param name="name"></param>
        internal void SetName(string name)
        {
            _name = name;
        }

        /// <summary>
        /// Sets the slot id for linking
        /// </summary>
        /// <param name="slotNumber"></param>
        internal void SetSlot(int slotNumber)
        {
            _slot = slotNumber;
        }

        /// <summary>
        /// Gets the currently available power in the powercell
        /// </summary>
        /// <returns></returns>
        internal float GetPower()
        {
            return _power;
        }

        /// <summary>
        /// Get the name of the powercell
        /// </summary>
        /// <returns></returns>
        internal string GetName()
        {
            return _name;
        }

        /// <summary>
        /// Get the slot id of this cell
        /// </summary>
        /// <returns></returns>
        internal int GetSlot()
        {
            return _slot;
        }

        /// <summary>
        /// Get the current Percentage of the battery
        /// </summary>
        /// <returns></returns>
        internal float GetPercentage()
        {
            return _power / _capacity;
        }

        /// <summary>
        /// Removes power from the battery
        /// </summary>
        /// <param name="amount"></param>
        internal void Consume(float amount)
        {
            // I am adding because the amount will be negative
            _power = Mathf.Clamp(_power + amount, 0, _capacity);
        }

        /// <summary>
        /// Charges the power cell
        /// </summary>
        /// <param name="amount"></param>
        internal void Charge(float amount)
        {
            _power = Mathf.Clamp(_power + amount, 0, _capacity);
        }

        /// <summary>
        /// Completely drains the battery;
        /// </summary>
        internal void Drain()
        {
            _power = 0f;
        }

        /// <summary>
        /// Sets the GUI representation of this powercell
        /// </summary>
        /// <param name="meter"></param>
        internal void SetMeter(GameObject meter)
        {
            _meterDisplay = meter;

            if (meter != null)
            {
                _batteryStatus1Bar = meter.FindChild("ProgressBar").GetComponent<Image>();
                _batteryStatus1Percentage = meter.FindChild("Percentage").GetComponent<Text>();
                meter.FindChild("Battery_Name_LBL").GetComponent<Text>().text = CreateID();
            }
            else
            {
                QuickLogger.Error(" Meter Display is null");
            }
        }

        /// <summary>
        /// Updates the battery information on screen
        /// </summary>
        internal void UpdateBatteryMeter()
        {
            if (_meterDisplay != null)
            {
                var percent = GetPercentage();
                _batteryStatus1Bar.fillAmount = percent;
                _batteryStatus1Percentage.text = $"{Mathf.CeilToInt(percent * 100)}%";
            }
            else
            {
                QuickLogger.Error(" Meter Display is null");
            }
        }

        /// <summary>
        /// Gets is the battery is full
        /// </summary>
        /// <returns></returns>
        internal bool GetIsFull()
        {
            return IsFull;
        }

        /// <summary>
        /// Creates name for the battery meter
        /// </summary>
        /// <returns></returns>
        private string CreateID()
        {
            return $"{LanguageHelpers.GetLanguage(FCSPowerStorageBuildable.BatteryKey)} {_slot}";
        }
    }
}
