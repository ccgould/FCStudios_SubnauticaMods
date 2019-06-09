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
            QuickLogger.Debug("In OnTriggerEnter");
            OnPlayerEnter?.Invoke();
        }

        private void OnTriggerStay(Collider other)
        {
            //QuickLogger.Debug("In OnTriggerStay");
            OnPlayerStay?.Invoke();
        }

        private void OnTriggerExit(Collider other)
        {
            QuickLogger.Debug("In OnTriggerExit");
            OnPlayerExit?.Invoke();
        }
    }
}
