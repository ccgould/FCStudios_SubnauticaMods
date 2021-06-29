using FCS_AlterraHub.Helpers;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_EnergySolutions.Mods.AlterraSolarCluster.Mono
{
    internal class AlterraSolarClusterMovementManager : MonoBehaviour
    {
        private AlterraSolarClusterController _mono;

        /// <summary>
        /// The transform to rotate around the X axis (elevate) when aiming.
        /// </summary>
        private Transform _elevator;

        /// <summary>
        /// The transform to rotate around the Y axis (rotate) when aiming.
        /// </summary>
        private Transform _rotator;

        private Transform _myTarget;

        private const float MaxPitch = 323f;
        private const float MinPitch = 90f;

        private float _targetPitch;

        private float _targetYaw;

        private float _currentPitch;

        private float _currentYaw;



        internal void Initialize(AlterraSolarClusterController mono)
        {
            _mono = mono;
            _rotator = GameObjectHelpers.FindGameObject(gameObject, "RotorMesh").transform;
            _elevator = GameObjectHelpers.FindGameObject(gameObject, "HingeMesh").transform;
        }


        public void LookAt(Vector3 point)
        {
            Vector3 eulerAngles = Quaternion.LookRotation(transform.InverseTransformDirection(Vector3.Normalize(point - transform.position))).eulerAngles;
            float num = eulerAngles.x;
            if (num > MaxPitch)
            {
                num = Mathf.Clamp(num, MinPitch, MaxPitch);
            }
            _targetPitch = num;
            _targetYaw = eulerAngles.y;
        }

        private void Update()
        {
            if(_mono != null && _mono.IsOperational && uSkyManager.main?.SunDir != null && FCS_AlterraHub.Patches.Player_Patches.SunTarget != null)
            {
                if (_myTarget == null)
                {
                    _myTarget = FCS_AlterraHub.Patches.Player_Patches.SunTarget.transform;
                }


                if (_myTarget)
                {
                    LookAt(_myTarget.transform.position);
                }

                _currentYaw = Mathf.LerpAngle(_currentYaw, _targetYaw, Time.deltaTime * 2f);
                _currentPitch = Mathf.LerpAngle(_currentPitch, _targetPitch, Time.deltaTime * 2f);
                _rotator.localEulerAngles = new Vector3(0f, _currentYaw, 0f);
                _elevator.localEulerAngles = new Vector3(_currentPitch, 0f, 0f);

            }
        }
    }
}
