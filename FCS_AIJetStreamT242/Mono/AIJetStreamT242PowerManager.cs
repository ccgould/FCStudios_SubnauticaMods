using FCS_AIJetStreamT242.Buildable;
using FCSCommon.Utilities;
using System;
using UnityEngine;

namespace FCS_AIJetStreamT242.Mono
{
    /// <summary>
    // This class handles all power management  of the turbine
    /// </summary>
    internal class AIJetStreamT242PowerManager : MonoBehaviour, IPowerInterface
    {
        private bool _hasBreakerTripped;
        public bool IsSafeToContinue { get; set; }
        public Action OnKillBattery { get; set; }
        public Action OnBreakerTripped { get; set; }
        public Action OnBreakerReset { get; set; }
        private float _charge;
        public float MaxPowerPerMin { get; set; } = 100;

        private float _capacity;
        private PowerRelay _powerRelay;
        private bool _isBatteryDestroyed;
        private AIJetStreamT242Controller _mono;


        #region Unity Methods

        private void Start()
        {
            _capacity = AIJetStreamT242Buildable.JetStreamT242Config.MaxCapacity;
            InvokeRepeating("UpdatePowerRelay", 0, 1);
        }
        private void Update()
        {
            //TODO See if needed
            //if (!IsSafeToContinue) return;
            UpdatePowerRelay();
            ProducePower();
        }
        #endregion

        private void UpdatePowerRelay()
        {
            try
            {
                var relay = PowerSource.FindRelay(transform);

                if (relay != null && relay != _powerRelay)
                {
                    if (_powerRelay != null)
                    {
                        _powerRelay.RemoveInboundPower(this);
                    }
                    _powerRelay = relay;
                    _powerRelay.AddInboundPower(this);
                    CancelInvoke("UpdatePowerRelay");
                }
                else
                {
                    _powerRelay = null;
                }

                if (_powerRelay != null)
                {
                    _powerRelay.RemoveInboundPower(this);
                    _powerRelay.AddInboundPower(this);
                    CancelInvoke("UpdatePowerRelay");
                }
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }
        }

        private void ProducePower()
        {
            if (_hasBreakerTripped)
            {
                _charge = 0.0f;
            }
            else
            {
                var decPercentage = (MaxPowerPerMin / _mono.MaxSpeed) / 60;

                var energyPerSec = _mono.GetCurrentSpeed() * decPercentage;

                _charge = Mathf.Clamp(_charge + energyPerSec * DayNightCycle.main.deltaTime, 0, AIJetStreamT242Buildable.JetStreamT242Config.MaxCapacity);
            }
        }

        internal void KillBattery()
        {
            QuickLogger.Debug($"KillBattery");
            _capacity = _charge = 0;
            OnKillBattery?.Invoke();
        }

        internal void DestroyBattery()
        {
            QuickLogger.Debug($"Destroy Battery");
            _charge = _capacity = 0f;
            _isBatteryDestroyed = true;
        }

        internal void RepairBattery()
        {
            _capacity = AIJetStreamT242Buildable.JetStreamT242Config.MaxCapacity;
            _isBatteryDestroyed = false;
        }

        private void OnDestroy()
        {
            try
            {
                KillBattery();

                if (_powerRelay != null)
                {
                    _powerRelay.RemoveInboundPower(this);
                }
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }
        }

        #region IPowerInterface

        public float GetPower()
        {
            if (_charge < 0.1)
            {
                _charge = 0.0f;
            }

            return _charge;
        }

        public float GetMaxPower()
        {
            return _capacity;
        }

        public bool ModifyPower(float amount, out float modified)
        {
            modified = 0f;


            bool result;
            if (amount >= 0f)
            {
                result = (amount <= AIJetStreamT242Buildable.JetStreamT242Config.MaxCapacity - _charge);
                modified = Mathf.Min(amount, AIJetStreamT242Buildable.JetStreamT242Config.MaxCapacity - _charge);
                _charge += Mathf.Round(modified);
            }
            else
            {
                result = (_charge >= -amount);
                if (GameModeUtils.RequiresPower())
                {
                    modified = -Mathf.Min(-amount, _charge);
                    _charge += Mathf.Round(modified);
                }
                else
                {
                    modified = amount;
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

        internal float GetCharge()
        {
            return _charge;
        }

        public void SetCharge(float savedDataCharge)
        {
            _charge = savedDataCharge;
        }

        internal void TriggerPowerOff()
        {
            _hasBreakerTripped = true;
            OnBreakerReset?.Invoke();
        }

        public void TriggerPowerOn()
        {
            _hasBreakerTripped = false;
            OnBreakerTripped?.Invoke();
        }

        public void Initialize(AIJetStreamT242Controller mono)
        {
            _mono = mono;
        }

        internal bool GetHasBreakerTripped()
        {
            return _hasBreakerTripped;
        }

        internal void SetHasBreakerTripped(bool value)
        {
            _hasBreakerTripped = value;
        }

        internal bool IsBatteryDestroyed()
        {
            return _isBatteryDestroyed;
        }
    }
}
