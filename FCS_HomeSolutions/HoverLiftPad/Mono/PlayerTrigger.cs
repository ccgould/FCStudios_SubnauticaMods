using System;
using UnityEngine;

namespace FCS_HomeSolutions.HoverLiftPad.Mono
{
    internal class PlayerTrigger : MonoBehaviour
    {
        public Action OnPlayerEntered;
        public Action OnPlayerExited;
        public Action OnPlayerStay;
        public bool IsPlayerInRange { get; set; }

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.layer != 19) return;
            IsPlayerInRange = true;
            OnPlayerEntered?.Invoke();
        }

        private void OnTriggerStay(Collider collider)
        {
            if (collider.gameObject.layer != 19 || IsPlayerInRange) return;
            OnPlayerStay?.Invoke();
            IsPlayerInRange = true;
        }

        private void OnTriggerExit(Collider collider)
        {
            if (collider.gameObject.layer != 19) return;
            IsPlayerInRange = false;
            OnPlayerExited?.Invoke();
        }
    }
}