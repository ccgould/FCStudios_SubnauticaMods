using UnityEngine;

namespace FCS_LifeSupportSolutions.Mods.MiniMedBay.mono
{
    internal class MiniMedBayTrigger : MonoBehaviour
    {
        public bool InPlayerInTrigger { get; set; }

        private void OnTriggerEnter(Collider collision)
        {
            if (!collision.gameObject.tag.Equals("Player")) return;
            InPlayerInTrigger = true;
        }

        private void OnTriggerStay(Collider collision)
        {
            if (!collision.gameObject.tag.Equals("Player")) return;
            InPlayerInTrigger = true;
        }

        private void OnTriggerExit(Collider collision)
        {
            if (!collision.gameObject.tag.Equals("Player")) return;
            InPlayerInTrigger = false;
        }
    }
}
