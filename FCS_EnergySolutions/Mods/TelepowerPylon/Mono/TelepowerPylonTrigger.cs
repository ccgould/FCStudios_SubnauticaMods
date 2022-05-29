using System;
using UnityEngine;

namespace FCS_EnergySolutions.Mods.TelepowerPylon.Mono
{
    internal class TelepowerPylonTrigger : MonoBehaviour
    {
        internal bool IsPlayerInRange;

        internal Action<bool> onTriggered { get; set; }

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.layer != 19) return;
            IsPlayerInRange = true;
            onTriggered?.Invoke(true);
        }

        private void OnTriggerStay(Collider collider)
        {
            if (collider.gameObject.layer != 19 || IsPlayerInRange) return;
            onTriggered?.Invoke(true);
            IsPlayerInRange = true;
        }

        private void OnTriggerExit(Collider collider)
        {
            if (collider.gameObject.layer != 19) return;
            IsPlayerInRange = false;
            onTriggered?.Invoke(false);

        }
    }
}