using FCS_AlterraHub.Core.Enumerators;
using FCSCommon.Utilities;
using Newtonsoft.Json;
using UnityEngine;

namespace FCS_AlterraHub.Core.Components;
public class RotorHandler : MonoBehaviour
{
    private Transform _transform;
    private float _targetRotation;
    private TargetAxis _targetAxis;
    private bool _axisSet;
    private bool _move;
    private bool _useGlobal;
    private float _speed = 2f;

    private void Start()
    {
        _transform = gameObject.transform;
    }

    private void RotateRotor()
    {
        var currentRotation = _useGlobal ? _transform.rotation : _transform.localRotation;

        Quaternion wantedRotation = default;
        switch (_targetAxis)
        {
            case TargetAxis.X:
                wantedRotation = Quaternion.Euler(_targetRotation, currentRotation.y, currentRotation.z);
                break;
            case TargetAxis.Y:
                wantedRotation = Quaternion.Euler(currentRotation.x, _targetRotation, currentRotation.z);
                break;
            case TargetAxis.Z:
                wantedRotation = Quaternion.Euler(currentRotation.x, currentRotation.y, _targetRotation);
                break;
        }

        if (_useGlobal)
        {
            _transform.rotation = Quaternion.RotateTowards(currentRotation, wantedRotation, DayNightCycle.main.deltaTime * _speed);
        }
        else
        {
            _transform.localRotation = Quaternion.RotateTowards(currentRotation, wantedRotation, DayNightCycle.main.deltaTime * _speed);
        }
    }

    public void ResetToMag(bool global = true, float speed = 2f)
    {
        if (!_axisSet) return;
        ChangePosition(-Input.compass.magneticHeading, global, speed);
    }

    private void Update()
    {
        if (!_move) return;
        RotateRotor();
    }

    public void ChangePosition(float deg, bool global = true, float speed = 2f)
    {
        _targetRotation = deg;
        _useGlobal = global;
        _speed = speed;
    }

    public void SetTargetAxis(TargetAxis targetAxis)
    {
        _targetAxis = targetAxis;
        _axisSet = true;
    }

    public void Run()
    {
        _move = true;
    }

    public void Stop()
    {
        _move = false;
    }

    public RotorSaveData Save()
    {
        return new RotorSaveData()
        {
            TilterDeg = _targetRotation,
            TilterUseGlobal = _useGlobal,
            TilterAxis = _targetAxis,
            AxisTilterSet = _axisSet,
            TilterMove = _move
        };
    }

    public void LoadSave(RotorSaveData savedData)
    {
        QuickLogger.Debug($"Target Rotation: {savedData.TilterDeg} || Use Global: {savedData.TilterUseGlobal} || Target Axis: {savedData.TilterAxis} || Axis: {savedData.TilterAxis} || Move: {savedData.TilterMove}", true);
        _targetRotation = savedData.TilterDeg;
        _useGlobal = savedData.TilterUseGlobal;
        _targetAxis = savedData.TilterAxis;
        _axisSet = savedData.AxisTilterSet;
        _move = savedData.TilterMove;
    }
}

public struct RotorSaveData
{
    [JsonProperty] public float TilterDeg { get; set; }
    [JsonProperty] public bool TilterUseGlobal { get; set; }
    [JsonProperty] public TargetAxis TilterAxis { get; set; }
    [JsonProperty] public bool AxisTilterSet { get; set; }
    [JsonProperty] public bool TilterMove { get; set; }
}
