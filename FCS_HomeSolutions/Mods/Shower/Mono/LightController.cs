using UnityEngine;

namespace FCS_HomeSolutions.Mods.Shower.Mono
{
    internal class LightController : MonoBehaviour
    {
        public Light TargetLight { get; set; }

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.layer != 19) return;
            if (TargetLight != null )
            {
                TargetLight.enabled = true;
            }
        }

        private void OnTriggerExit(Collider collider)
        {
            if (collider.gameObject.layer != 19) return;
            if (TargetLight != null)
            {
                TargetLight.enabled = false;
            }
        }

        private void OnTriggerStay(Collider collider)
        {
            if (collider.gameObject.layer != 19) return;
            if (TargetLight != null)
            {
                TargetLight.enabled = true;
            }
        }
    }
}