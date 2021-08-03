using UnityEngine;

namespace FCS_HomeSolutions.Mods.Elevator.Mono
{
    internal class PlatformTrigger : MonoBehaviour
    {
        private Transform _platFormTrans;
        public bool IsPlayerInside { get;private set; }
        private void Start()
        {
            _platFormTrans = gameObject.transform;
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.layer != 19 || IsPlayerInside) return;
            Player.main.gameObject.transform.SetParent(_platFormTrans);
            IsPlayerInside = true;
        }

        private void OnTriggerExit(Collider collider)
        {
            if (collider.gameObject.layer != 19) return;
            Player.main.gameObject.transform.SetParent(null);
            IsPlayerInside = false;
        }
    }
}