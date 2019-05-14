using System;
using UnityEngine;

namespace FCSCommon.Components
{
    /// <summary>
    /// This class allows you to add an internal battery that doesn't notify the base in any situation
    /// </summary>
    public class InternalBatteryController : HandTarget, IHandTarget
    {
        private bool _depleted;
        public float Charge { get; set; }
        public float Capacity { get; private set; } = 200f;
        public event Action OnDepleted;

        private void Awake()
        {
            InvokeRepeating("CheckBatteryLevel", 0, 0.5f);
        }

        private void CheckBatteryLevel()
        {
            if (Charge <= 0 && _depleted == false)
            {
                OnDepleted?.Invoke();
                _depleted = true;
            }
        }

        /// <summary>
        /// Creates the battery with the provided information
        /// use this method to set both properties
        /// </summary>
        /// <param name="charge">The amount of charge the battery has.</param>
        /// <param name="capacity">The capacity of the battery</param>
        public void CreateBattery(float charge, float capacity)
        {
            Charge = charge;
            Capacity = capacity;
        }

        public void ChargeBattery(float amount)
        {
            Charge = Mathf.Clamp(Charge + amount, 0, Capacity);
        }

        public void DischargeBattery(float amount)
        {
            Charge = Mathf.Clamp(Charge - amount, 0, Capacity);
        }

        public void KillBattery()
        {
            Charge = 0;
        }

        public void OnHandHover(GUIHand hand)
        {
            HandReticle.main.SetInteractText($"{Charge}/{Capacity}", false);
        }

        public void OnHandClick(GUIHand hand)
        {

        }
    }
}
