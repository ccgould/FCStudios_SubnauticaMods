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

        private const float maxPitch = 323f;
        private const float minPitch = 90f;

        private float targetPitch;

        private float targetYaw;

        private float currentPitch;

        private float currentYaw;



        internal void Initialize(AlterraSolarClusterController mono)
        {
            _mono = mono;
            _rotator = GameObjectHelpers.FindGameObject(gameObject, "RotorMesh_3").transform;
            _elevator = GameObjectHelpers.FindGameObject(gameObject, "HingeMesh_3").transform;
        }


        public void LookAt(Vector3 point)
        {
            Vector3 eulerAngles = Quaternion.LookRotation(base.transform.InverseTransformDirection(Vector3.Normalize(point - base.transform.position))).eulerAngles;
            float num = eulerAngles.x;
            if (num > maxPitch)
            {
                num = Mathf.Clamp(num, minPitch, maxPitch);
            }
            this.targetPitch = num;
            this.targetYaw = eulerAngles.y;
        }

        private void Update()
        {
            if(_mono != null && _mono.IsOperational && uSkyManager.main?.SunDir != null && FCS_AlterraHub.Patches.Player_Awake_Patch.SunTarget != null)
            {
                if (_myTarget == null)
                {
                    _myTarget = FCS_AlterraHub.Patches.Player_Awake_Patch.SunTarget.transform;
                }


                if (_myTarget)
                {
                    LookAt(_myTarget.transform.position);
                }

                currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, Time.deltaTime * 2f);
                currentPitch = Mathf.LerpAngle(currentPitch, targetPitch, Time.deltaTime * 2f);
                _rotator.localEulerAngles = new Vector3(0f, currentYaw, 0f);
                _elevator.localEulerAngles = new Vector3(currentPitch, 0f, 0f);

            }
        }
    }
}
