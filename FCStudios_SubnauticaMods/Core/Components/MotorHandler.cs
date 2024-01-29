using FCS_AlterraHub.Core.Enumerators;
using FCSCommon.Utilities;
using Newtonsoft.Json;
using System;
using UnityEngine;

namespace FCS_AlterraHub.Core.Components;
public class MotorHandler : MonoBehaviour
{
    private float _currentRpm;
    [SerializeField] private float _rpm;
    [SerializeField] private const float RpmPerDeg = 0.16667f;
    [SerializeField] private float _increaseRate = 2f;
    [SerializeField] private Vector3 spinAxis = Vector3.up;
    private bool _increasing = true;
    public bool IsRunning => _increasing;

    private void Update()
    {
        ChangeMotorSpeed();
    }

    private void ChangeMotorSpeed()
    {
        //increase or decrease the current speed depending on the value of increasing
        _currentRpm = Mathf.Clamp(_currentRpm + DayNightCycle.main.deltaTime * _increaseRate * (_increasing ? 1 : -1), 0, _rpm);
        gameObject.transform.Rotate(spinAxis, _currentRpm * DayNightCycle.main.deltaTime);
    }

    public int GetSpeed()
    {
        return Convert.ToInt32(_currentRpm * RpmPerDeg);
    }

    /// <summary>
    /// Bypasses the spooling of the motor and sets the motor to the desired rpm
    /// </summary>
    /// <param name="speed"></param>
    public void SpeedByPass(float rpm)
    {
        _currentRpm = rpm;
    }

    /// <summary>
    /// Bypasses the spooling of the motor and sets the motor to the desired rpm
    /// </summary>
    /// <param name="speed"></param>
    public void RPMByPass(float rpm)
    {
        _rpm = rpm;
    }

    /// <summary>
    /// Starts the motor and brings it to the desired max speed.
    /// </summary>
    public void StartMotor()
    {
        _increasing = true;
    }

    /// <summary>
    /// Stops the motor from running.
    /// </summary>
    public void StopMotor()
    {
        _increasing = false;
    }

    /// <summary>
    /// Toggles on and off the motor.
    /// </summary>
    public void Toggle()
    {
        _increasing = !_increasing;
    }

    /// <summary>
    /// Gives you the current R.P.M of the motor
    /// </summary>
    /// <returns></returns>
    public float GetRPM()
    {
        return _currentRpm;
    }

    public void SetIncreaseRate(int value = 2)
    {
        _increaseRate = value;
    }

    /// <summary>
    /// Gives you the is increasing.
    /// </summary>
    /// <returns></returns>
    public bool GetIsIncreasing()
    {
        return _increasing;
    }

    public MotorSaveData Save()
    {
        return new MotorSaveData()
        {
            IsIncreasing = _increasing,
            CurrentSpeed = _currentRpm,
            TargetRPM = _rpm,
        };
    }

    public void Load(MotorSaveData savedData)
    {
        QuickLogger.Debug($"Increasing: {savedData.IsIncreasing} || Current RPM: {savedData.CurrentSpeed} || RPM {savedData.TargetRPM}", true);

        _increasing = savedData.IsIncreasing;
        _currentRpm = savedData.CurrentSpeed;
        _rpm = savedData.TargetRPM;

    }
}

public struct MotorSaveData
{
    public bool IsIncreasing { get; set; }
    public float CurrentSpeed { get; set; }
    public float TargetRPM { get; set; }
}