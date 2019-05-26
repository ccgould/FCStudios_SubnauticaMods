using FCSCommon.Components;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AIJetStreamT242.Mono
{
    internal class AIJetStreamT242AnimationManager : MonoBehaviour
    {
        private BeaconController _beaconController;
        public Animator Animator { get; set; }
        private void Awake()
        {
            Animator = GetComponentInParent<Animator>();
            _beaconController = GetComponentInParent<BeaconController>();
            _beaconController.Animator = Animator;
        }

        internal void ChangeBeaconState(bool state)
        {
            if (state)
            {
                _beaconController.ShowBeacon();
            }
            else
            {
                _beaconController.HideBeacon();
            }
        }

        internal void SetBoolHash(int stateHash, bool value)
        {
            if (Animator == null)
            {
                QuickLogger.Error("Animator is null on load");
                this.Animator = this.transform.GetComponent<Animator>();
            }

            this.Animator.SetBool(stateHash, value);
        }

        internal void SetFloatHash(int stateHash, float value)
        {
            if (Animator == null)
            {
                QuickLogger.Error("Animator is null on load");
                this.Animator = this.transform.GetComponent<Animator>();
            }

            this.Animator.SetFloat(stateHash, value);
        }

        public AnimatorControllerParameter[] GetParameters()
        {
            return Animator.parameters;
        }
    }
}
