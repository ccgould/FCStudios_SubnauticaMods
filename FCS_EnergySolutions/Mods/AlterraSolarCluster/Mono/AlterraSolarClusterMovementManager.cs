using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_EnergySolutions.Mods.AlterraSolarCluster.Mono
{
    internal class AlterraSolarClusterMovementManager : MonoBehaviour
    {
        private AlterraSolarClusterController _mono;
        private uSkyManager USkyManager => uSkyManager.main;

        /// <summary>
        /// The transform to rotate around the X axis (elevate) when aiming.
        /// </summary>
        private Transform _elevator;

        /// <summary>
        /// The maximum elevation, in degress, below the zero point.
        /// </summary>
        private const float MinElevation = 42;

        /// <summary>
        /// The maximum elevation, in degress, above the zero point.
        /// </summary>
        private const float MaxElevation = 90;

        /// <summary>
        /// The transform to rotate around the Y axis (rotate) when aiming.
        /// </summary>
        private Transform _rotator;

        private Transform _myTarget;


        /// <summary>
        /// The maximum rotation, in degress, to the left of the zero point.
        /// </summary>
        private const float MinRotation = 0;

        /// <summary>
        /// The maximum rotation, in degress, to the right of the zero point.
        /// </summary>
        private const float MaxRotation = 0;

        /// <summary>
        /// The accuracy of the solar, in meters.
        /// </summary>
        private const float Accuracy = 0;

        /// <summary>
        /// The speed at which the rotor may be turned.
        /// </summary>
        private const float TurnSpeed = 5;

        /// <summary>
        /// Rotates and elevates the tilter torwards the target.
        /// </summary>
        /// <param name="target">The target to aim at.</param>
        public virtual void Aim(Vector3 target)
        {
            // Get a plane through the rotation of the weapon
            Plane rot = new Plane(_rotator.right, _rotator.position);
            if (Mathf.Abs(rot.GetDistanceToPoint(target)) < Accuracy)
                return;

            // And rotate towards target
            if (rot.GetSide(target))
                Rotate(1.0f); //right
            else
                Rotate(-1.0f); //left

            // Get a plane through the elevation of the weapon
            Plane elev = new Plane(_elevator.up, _elevator.position);
            if (Mathf.Abs(elev.GetDistanceToPoint(target)) < Accuracy)
                return;

            // And elevate towards target
            if (elev.GetSide(target))
                Elevate(1.0f); //up
            else
                Elevate(-1.0f); //down
        }
        
        private void FixedUpdate()
        {
            if(_mono != null && _mono.IsOperational && uSkyManager.main?.SunDir != null && FCS_AlterraHub.Patches.Player_Awake_Patch.SunTarget != null)
            {
                if (_myTarget == null)
                {
                    _myTarget = FCS_AlterraHub.Patches.Player_Awake_Patch.SunTarget.transform;
                }
                Aim(_myTarget.position);
            }

        }

        internal void Initialize(AlterraSolarClusterController mono)
        {
            _mono = mono;
            _rotator = GameObjectHelpers.FindGameObject(gameObject, "RotorMesh_3").transform;
            _elevator = GameObjectHelpers.FindGameObject(gameObject, "HingeMesh_3").transform;
        }

        /// <summary>
        /// Pivots the weapon up(+) and down(-).
        /// </summary>
        /// <param name="direction">The direction to pivot; up is positive, down is negative.</param>
        public virtual void Elevate(float direction)
        {
            // Clamp the direction input between -1 and 1...
            direction = Mathf.Clamp(-direction, -1.0f, 1.0f);

            // Calculate the new angle...
            float angle = _elevator.localEulerAngles.x + direction * TurnSpeed * Time.deltaTime;
            if (angle > 180)
                angle -= 360;

            // Clamp the new angle between the given minimum and maximum...
            angle = Mathf.Clamp(angle, -MaxElevation, -MinElevation);

            // Update the transform...
            _elevator.localEulerAngles = new Vector3(angle, _elevator.localEulerAngles.y, _elevator.localEulerAngles.z);
        }

        /// <summary>
        /// Pivots the weapon right(+) and left(-).
        /// </summary>
        /// <param name="direction">The direction to pivot; right is positive, left is negative.</param>
        public virtual void Rotate(float direction)
        {
            // Clamp the direction input between -1 and 1...
            direction = Mathf.Clamp(direction, -1.0f, 1.0f);

            // Calculate the new angle...
            float angle = _rotator.localEulerAngles.y + direction * TurnSpeed * Time.deltaTime;
            if (angle > 180)
                angle -= 360;

            // Clamp the new angle between the given minimum and maximum...
            if (Mathf.Abs(MinRotation) + Mathf.Abs(MaxRotation) > 0)
                angle = Mathf.Clamp(angle, MinRotation, MaxRotation);

            // Update the transform...
            _rotator.localEulerAngles = new Vector3(_rotator.localEulerAngles.x, angle, _rotator.localEulerAngles.z);
        }
    }
}
