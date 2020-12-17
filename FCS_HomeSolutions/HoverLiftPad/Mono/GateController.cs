using System;
using UnityEngine;

namespace FCS_HomeSolutions.HoverLiftPad.Mono
{
    internal class GateController : MonoBehaviour
    {
        private bool _allowedToMove;
        private float _targetRotation;
        private bool _isOpen;
        internal Gate GateType;
        internal float RotateDegrees;
        internal float StartRotateDegrees;
        internal float RotateTime = 10.0f;

        internal Action<bool> OnGateStateChanged { get; set; }

        private void Update()
        {
            if (_allowedToMove)
            {
                RotateRotor();
            }
        }

        private void RotateRotor()
        {
            var currentRotation = transform.localRotation;
            var wantedRotation = Quaternion.Euler(currentRotation.x, _targetRotation, currentRotation.z);
            transform.localRotation = Quaternion.RotateTowards(currentRotation, wantedRotation, DayNightCycle.main.deltaTime * RotateTime);

            if (transform.localRotation == wantedRotation)
            {
                _isOpen = Mathf.Approximately(_targetRotation, RotateDegrees);
            }
        }

        internal void Open()
        {
            _isOpen = true;
            _allowedToMove = true;
            _targetRotation = RotateDegrees;
        }

        internal void Close()
        {
            _isOpen = true;
            _allowedToMove = true;
            _targetRotation = StartRotateDegrees;
        }

        internal bool IsOpen()
        {
            return _isOpen;
        }

        internal bool IsClosed()
        {
            return _isOpen == false;
        }
    }
}