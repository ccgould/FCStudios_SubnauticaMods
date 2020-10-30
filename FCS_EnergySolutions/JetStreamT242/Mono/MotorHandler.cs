using System;
using FCS_EnergySolutions.Configuration;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_EnergySolutions.JetStreamT242.Mono
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
            gameObject.transform.Rotate(Vector3.forward, _currentRpm * DayNightCycle.main.deltaTime);
        }

        internal void Initialize(float rpm = 0)
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
            _rpm = rpm;
        }

        /// <summary>
        /// Starts the motor and brings it to the desired max speed.
        /// </summary>
        internal void Run()
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

        public void Save(JetStreamT242DataEntry savedData)
        {
            savedData.IsIncreasing = _increasing;
            savedData.CurrentSpeed = _currentRpm;
            savedData.TargetRPM = _rpm;
        }

        public void LoadSave(JetStreamT242DataEntry savedData)
        {
            QuickLogger.Debug($"Increasing: {savedData.IsIncreasing} || Current RPM: {savedData.CurrentSpeed} || RPM {savedData.TargetRPM}",true);

            _increasing = savedData.IsIncreasing;
            _currentRpm = savedData.CurrentSpeed;
            _rpm = savedData.TargetRPM;
            
        }
    }
}
