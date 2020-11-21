using System;
using UnityEngine;

namespace FCS_AlterraHub.Mono.AlterraHub
{
    internal class HubTrigger : MonoBehaviour
    {
        private AlterraHubController _mono;
        internal bool IsPlayerInRange;
        
        internal Action<bool> onTriggered { get; set; }

        internal void Initialize(AlterraHubController mono)
        {
            _mono = mono;
        }

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