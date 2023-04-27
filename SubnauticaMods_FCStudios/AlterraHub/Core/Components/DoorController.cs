using FCSCommon.Utilities;
using rail;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FCS_AlterraHub.Core.Components
{
    internal class DoorController : MonoBehaviour
    {
        private bool _isRotatingDoor;
        public Vector3 direction;
        public bool IsOpen;
        private float _speed = 1f;
        private float _rotationAmount = 90f;
        private float _forwardDirection = 0f;

        private Vector3 _startRotation;
        private Vector3 _forward;

        private Vector3 _slideDirection = Vector3.back;
        private float _slideAmount = 1.9f;
        private Vector3 _startPosition;

        private Coroutine _animationCoroutine;

        private void Awake()
        {
            _startPosition = transform.position;
            _startRotation = transform.rotation.eulerAngles;
            // Since "Forward" actually is pointing to the door frame, choose a direction to think about as "forward"
            _forward = transform.right;
        }

        public void Initialize ()
        {

        }

        public void Open(Vector3 userPosition)
        {
            if(!IsOpen)
            {
                if(_animationCoroutine != null)
                {
                    StopCoroutine(_animationCoroutine);
                }

                if (_isRotatingDoor)
                {
                    float dot = Vector3.Dot(_forward,(userPosition - transform.position).normalized);
                    QuickLogger.Debug(dot.ToString("N3"));
                    _animationCoroutine = StartCoroutine(DoRotationOpen(dot));
                }
                else
                {
                    _animationCoroutine = StartCoroutine(DoSlidingOpen());
                }
            }
        }

        private IEnumerator DoSlidingOpen()
        {
           Vector3 endPosition = _startPosition + _slideAmount * _slideDirection;
            Vector3 startPositon = transform.position;

            float time = 0;
            while (time < 1)
            {
                transform.position = Vector3.Lerp(startPositon, endPosition, time);
                yield return null;
                time += DayNightCycle.main.deltaTime * _speed;
            }
        }

        private IEnumerator DoSlidingClose()
        {
            Vector3 endPosition = _startPosition;
            Vector3 startPositon = transform.position;

            float time = 0;
            while (time < 1)
            {
                transform.position = Vector3.Lerp(startPositon, endPosition, time);
                yield return null;
                time += DayNightCycle.main.deltaTime * _speed;
            }
        }

        public void Close ()
        {
            if(IsOpen)
            {
                if( _animationCoroutine != null )
                {
                    StopCoroutine(_animationCoroutine);
                }

                if (_isRotatingDoor)
                {
                    _animationCoroutine = StartCoroutine(DoRotationClose());
                }
                else
                {
                    _animationCoroutine = StartCoroutine(DoSlidingClose());
                }
            }
        }

        private IEnumerator DoRotationOpen(float forwardAmount)
        {
            Quaternion startRotation = transform.rotation;
            Quaternion endRotation;

            if (forwardAmount >= _forwardDirection)
            {
                endRotation = Quaternion.Euler(new Vector3(_startRotation.x, _startRotation.y - _rotationAmount, _startRotation.z));
            }
            else
            {
                endRotation = Quaternion.Euler(new Vector3(_startRotation.x, _startRotation.y + _rotationAmount, _startRotation.z));
            }

            IsOpen = true;

            float time = 0;
            while (time < 1)
            {
                transform.rotation = Quaternion.Slerp(startRotation, endRotation, time);
                yield return null;
                time += DayNightCycle.main.deltaTime * _speed;
            }
        }

        private IEnumerator DoRotationClose()
        {
            Quaternion startRotation = transform.rotation;
            Quaternion endRotation = Quaternion.Euler(_startRotation);
            IsOpen = false;


            float time = 0;
            while (time < 1)
            {
                transform.rotation = Quaternion.Slerp(startRotation, endRotation, time);
                yield return null;
                time += DayNightCycle.main.deltaTime * _speed;
            }
        }
    }
}
