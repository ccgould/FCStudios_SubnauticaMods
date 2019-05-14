using System.Collections;
using UnityEngine;

namespace FCSCommon.Components
{
    public class BeaconController : MonoBehaviour
    {
        public Animator Animator { get; set; }

        public void ShowBeacon()
        {
            StartCoroutine(ShowBeaconCoroutine());
        }

        public void HideBeacon()
        {
            StartCoroutine(HideBeaconCoroutine());
        }

        private IEnumerator HideBeaconCoroutine()
        {
            Animator.enabled = true;

            yield return new WaitForEndOfFrame();

            Animator.SetBool("BeaconOn", false);
            Animator.SetBool("BeaconOff", true);

            yield return new WaitForEndOfFrame();

        }

        private IEnumerator ShowBeaconCoroutine()
        {
            yield return new WaitForEndOfFrame();

            Animator.SetBool("BeaconOff", false);
            Animator.SetBool("BeaconOn", true);

            yield return new WaitForEndOfFrame();
        }
    }
}
