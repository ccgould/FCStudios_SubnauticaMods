using FCS_AlterraHub.Helpers;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.Elevator.Mono
{
    internal class PlatformTrigger : MonoBehaviour
    {
        private Transform _platFormTrans;
        private FCSElevatorController _controller;
        private Rigidbody _target;
        private Collider _cachedTarget;
        public bool IsPlayerInside { get;private set; }
        public void Initialize(FCSElevatorController controller)
        {
            _controller = controller;
            _platFormTrans = gameObject.transform;
        }

        private void OnTriggerEnter(Collider collider)
        {
            var go = WorldHelpers.GetRoot(collider.gameObject);
            _cachedTarget = collider;
            _target = go.GetComponent<Rigidbody>();
            IsPlayerInside = true;
        }

        private void OnTriggerExit(Collider collider)
        {
            if (_cachedTarget != collider)
            {
                return;
            }
            _cachedTarget = null;
            _target = null; 
            IsPlayerInside = false;
        }

        private void FixedUpdate()
        {
            if (!_target || !_controller.IsMoving())
            {
                return;
            }
            if (!_target.isKinematic)
            {
                _target.AddForce((_target.transform.position - base.transform.position).normalized * _pushStrength, ForceMode.Acceleration);
            }
        }

        private float _pushStrength = 1f;
    }
}