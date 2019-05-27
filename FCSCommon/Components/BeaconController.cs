using System.Collections;
using UnityEngine;

namespace FCSCommon.Components
{
    public class BeaconController : MonoBehaviour
    {
        private int _beaconStateHash;

        public bool IsBeingPinged => Animator.GetBool(_beaconStateHash);
        public Animator Animator { get; set; }

        private void Awake()
        {
            _beaconStateHash = UnityEngine.Animator.StringToHash("BeaconState");
        }

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
            yield return new WaitForEndOfFrame();

            Animator.SetBool(_beaconStateHash, false);

            yield return new WaitForEndOfFrame();

        }

        private IEnumerator ShowBeaconCoroutine()
        {
            yield return new WaitForEndOfFrame();

            Animator.SetBool(_beaconStateHash, true);

            yield return new WaitForEndOfFrame();
        }
    }
}
