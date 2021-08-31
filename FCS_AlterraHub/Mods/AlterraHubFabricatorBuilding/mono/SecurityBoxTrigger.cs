using UnityEngine;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono
{
    internal class SecurityBoxTrigger : MonoBehaviour
    {
        private void OnTriggerExit(Collider collider)
        {
            if (collider.gameObject.layer != 19) return;
            IsPlayerInRange = false;
        }

        public bool IsPlayerInRange { get; private set; }

        private void OnTriggerStay(Collider collider)
        {
            if (collider.gameObject.layer != 19) return;
            IsPlayerInRange = true;
        }
    }
}