using System.Collections;
using UnityEngine;

namespace FCSCommon.Models
{
    public class PowerController : MonoBehaviour, IPowerInterface
    {
        private float _charge;
        private float _capacity = 200f;
        private float _prevCharge;
        private float _prevCapacity;

        /// <summary>
        /// Creates the battery with the provided information
        /// use this method to set both properties
        /// </summary>
        /// <param name="charge">The amount of charge the battery has.</param>
        /// <param name="capacity">The capacity of the battery</param>
        public void CreateBattery(float charge, float capacity)
        {
            _charge = charge;
            _capacity = capacity;
        }

        public void SetCharge(float charge)
        {
            _charge = charge;
        }

        public void SetCapacity(float capacity)
        {
            _capacity = capacity;

        }

        public void KillBattery()
        {
            _prevCharge = _charge;
            _prevCapacity = _capacity;
            _charge = _capacity = 0;
            IsBatteryDistroyed = true;
        }

        public void DisChargeBattery()
        {
            _charge = 0;
        }

        public void RestoreBattery(bool restoreCharge)
        {
            if (restoreCharge)
            {
                _charge = _prevCharge;
            }

            _capacity = _prevCapacity;
            IsBatteryDistroyed = false;
        }

        public bool HasBreakerTripped { get; set; }
        public bool IsBatteryDistroyed { get; set; }

        public void ChargeBattery(float chargeAmount, bool invokeRepeat = true, float repeatInterval = 1f)
        {
            if (invokeRepeat)
            {
                StartCoroutine(ChargeBattery(repeatInterval, chargeAmount));
            }
            else
            {
                _charge = HasBreakerTripped ? 0.0f : Mathf.Clamp(_charge + chargeAmount * DayNightCycle.main.deltaTime, 0, _capacity);
            }
        }

        public void StopCharging()
        {
            StopCoroutine("ChargeBattery");
        }

        public bool GetInboundHasSource(IPowerInterface powerInterface)
        {
            return false;
        }

        public float GetMaxPower()
        {
            return _capacity;
        }

        public float GetPower()
        {
            return _charge;
        }

        public bool HasInboundPower(IPowerInterface powerInterface)
        {
            return false;
        }

        public bool ModifyPower(float amount, out float modified)
        {
            modified = 0f;

            bool result;

            if (amount >= 0f)
            {
                result = (amount <= _capacity - _charge);
                modified = Mathf.Min(amount, _capacity - _charge);
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

        private IEnumerator ChargeBattery(float repeatInterval, float chargeAmount)
        {
            while (true)
            {
                yield return new WaitForSeconds(repeatInterval);
                _charge = HasBreakerTripped ? 0.0f : Mathf.Clamp(_charge + chargeAmount * DayNightCycle.main.deltaTime, 0, _capacity);
            }
        }
    }
}
