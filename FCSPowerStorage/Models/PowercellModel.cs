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
        public float Power { get; set; }
        private string _name;
        private int _slot;
        private readonly float _capacity = LoadData.BatteryConfiguration.Capacity / 6;
        private Image _batteryStatus1Bar;
        private Text _batteryStatus1Percentage;
        private bool IsFull => Power >= _capacity;
        private GameObject _meterDisplay;

        /// <summary>
        /// Sets the amount of power available in cell
        /// </summary>
        /// <param name="amount"></param>
        internal void SetPower(float amount)
        {
            Power = amount;
        }

        /// <summary>
        /// Set the name of the cell
        /// </summary>
        /// <param name="name"></param>
        internal void SetName(string name)
        {
            if (name == string.Empty)
            {
                _name = CreateID();
                return;
            }
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
            return Power;
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

        internal float GetPowerValue()
        {
            return Power;
        }

        /// <summary>
        /// Get the current Percentage of the battery
        /// </summary>
        /// <returns></returns>
        internal float GetPercentage()
        {
            return Power / _capacity;
        }

        /// <summary>
        /// Removes power from the battery
        /// </summary>
        /// <param name="amount"></param>
        internal void Consume(float amount)
        {
            // I am adding because the amount will be negative
            if (Power < 1)
            {
                Drain();
                return;
            }

            Power = Mathf.Clamp(Power + amount, 0, _capacity);
            //QuickLogger.Debug($"Consume {GetName()}: Power Value {_power} || Capacity {_capacity} || Amount {amount}");
        }

        /// <summary>
        /// Charges the power cell
        /// </summary>
        /// <param name="amount"></param>
        internal void Charge(float amount)
        {
            Power = Mathf.Clamp(Power + amount, 0, _capacity);
            //QuickLogger.Debug($"Charge {GetName()}: Power Value {_power} || Capacity {_capacity} || Amount {amount}");
        }

        /// <summary>
        /// Completely drains the battery;
        /// </summary>
        internal void Drain()
        {
            Power = 0f;
        }

        /// <summary>
        /// Sets the GUI representation of this powercell
        /// </summary>
        /// <param name="meter"></param>
        internal void SetMeter(GameObject meter)
        {
            QuickLogger.Debug("Setting Meters");
            _meterDisplay = meter;

            if (meter != null)
            {
                _batteryStatus1Bar = meter.FindChild("ProgressBar")?.GetComponent<Image>();

                if (_batteryStatus1Bar == null)
                {
                    QuickLogger.Error("Battery Status Bar is null");
                    return;
                }

                _batteryStatus1Percentage = meter.FindChild("Percentage")?.GetComponent<Text>();

                if (_batteryStatus1Percentage == null)
                {
                    QuickLogger.Error("Battery Status Percentage is null");
                    return;
                }

                var name = meter.FindChild("Battery_Name_LBL").GetComponent<Text>();

                if (name == null)
                {
                    QuickLogger.Error("Battery Name is null");
                    return;
                }

                name.text = _name;
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
            return $"{LanguageHelpers.GetLanguage(FCSPowerStorageBuildable.BatteryKey)} {_slot + 1}";
        }
    }
}
