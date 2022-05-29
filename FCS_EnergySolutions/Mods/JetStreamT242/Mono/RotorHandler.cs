using FCS_AlterraHub.Enumerators;
using FCS_EnergySolutions.Configuration;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_EnergySolutions.Mods.JetStreamT242.Mono
{
    internal class RotorHandler : MonoBehaviour
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

        internal void ResetToMag(bool global = true, float speed = 2f)
        {
            if (!_axisSet) return;
            ChangePosition(-Input.compass.magneticHeading,global,speed);
        }

        private void Update()
        {
            if (!_move) return;
            RotateRotor();
        }

        internal void ChangePosition(float deg,bool global = true,float speed = 2f)
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

        public void Save(JetStreamT242DataEntry savedData)
        {
            savedData.TilterDeg = _targetRotation;
            savedData.TilterUseGlobal = _useGlobal;
            savedData.TilterAxis = _targetAxis;
            savedData.AxisTilterSet = _axisSet;
            savedData.TilterMove = _move;
        }

        public void LoadSave(JetStreamT242DataEntry savedData)
        {
            QuickLogger.Debug($"Target Rotation: {savedData.TilterDeg} || Use Global: {savedData.TilterUseGlobal} || Target Axis: {savedData.TilterAxis} || Axis: {savedData.TilterAxis} || Move: {savedData.TilterMove}",true);
            _targetRotation = savedData.TilterDeg;
            _useGlobal = savedData.TilterUseGlobal;
            _targetAxis = savedData.TilterAxis;
            _axisSet = savedData.AxisTilterSet;
            _move = savedData.TilterMove;
        }
    }
}
