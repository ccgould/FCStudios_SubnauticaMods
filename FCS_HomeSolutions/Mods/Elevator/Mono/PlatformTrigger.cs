using UnityEngine;

namespace FCS_HomeSolutions.Mods.Elevator.Mono
{
    internal class PlatformTrigger : MonoBehaviour
    {
        private Transform _platFormTrans;
        private FCSElevatorController _controller;
        public bool IsPlayerInside { get;private set; }
        public void Initialize(FCSElevatorController controller)
        {
            _controller = controller;
            _platFormTrans = gameObject.transform;
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.layer != 19 || IsPlayerInside || _controller == null || !_controller.IsOperational)
            {
                Player.main.transform.parent = null;
                return;
            }

            Player.main.transform.parent = _platFormTrans;
            IsPlayerInside = true;
        }

        private void OnTriggerExit(Collider collider)
        {
            if (collider.gameObject.layer != 19) return;
            Player.main.transform.parent = null;
            IsPlayerInside = false;
        }
    }
}