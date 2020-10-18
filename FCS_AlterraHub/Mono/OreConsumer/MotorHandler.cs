using System;
using UnityEngine;

namespace FCS_AlterraHub.Mono.OreConsumer
{
    internal class MotorHandler : MonoBehaviour
    {
        private float _currentRpm;
        private float _rpm;
        private bool _initialized;
        private const float RpmPerDeg = 0.16667f;
        private const float IncreaseRate = 2f;
        private bool _increasing = true;

        private void Update()
        {
            ChangeMotorSpeed();
        }

        private void ChangeMotorSpeed()
        {
            //increase or decrease the current speed depending on the value of increasing
            _currentRpm = Mathf.Clamp(_currentRpm + DayNightCycle.main.deltaTime * IncreaseRate * (_increasing ? 1 : -1), 0, _rpm);
            gameObject.transform.Rotate(Vector3.up, _currentRpm * DayNightCycle.main.deltaTime);
        }

        internal void Initialize(float rpm)
        {
            if(_initialized) return;
            _rpm = rpm;
            _initialized = true;
        }
        
        internal int GetSpeed()
        {
            return Convert.ToInt32(_currentRpm * RpmPerDeg);
        }

        /// <summary>
        /// Bypasses the spooling of the motor and sets the motor to the desired rpm
        /// </summary>
        /// <param name="speed"></param>
        internal void SpeedByPass(float rpm)
        {
            _currentRpm = rpm;
        }

        /// <summary>
        /// Starts the motor and brings it to the desired max speed.
        /// </summary>
        internal void Start()
        {
            _increasing = true;
        }

        /// <summary>
        /// Stops the motor from running.
        /// </summary>
        internal void Stop()
        {
            _increasing = false;
        }

        /// <summary>
        /// Toggles on and off the motor.
        /// </summary>
        internal void Toggle()
        {
            _increasing = !_increasing;
        }

        /// <summary>
        /// Gives you the current R.P.M of the motor
        /// </summary>
        /// <returns></returns>
        internal float GetRPM()
        {
            return _currentRpm;
        }
    }
}
