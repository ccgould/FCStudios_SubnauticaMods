using FCSCommon.Utilities;
using System;
using UnityEngine;

namespace AMMiniMedBay.Mono
{
    internal class AMMiniMedBayTrigger : MonoBehaviour
    {
        internal Action OnPlayerEnter;
        internal Action OnPlayerExit;
        internal Action OnPlayerStay;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.GetComponent<Player>() == null) return;
            OnPlayerEnter?.Invoke();
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.GetComponent<Player>() == null) return;
            OnPlayerStay?.Invoke();
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.GetComponent<Player>() == null) return;
            OnPlayerExit?.Invoke();
        }
    }
}
