using FCSAlterraShipping.Models;
using FCSAlterraShipping.Mono;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCSAlterraShipping.Display
{
    internal class AlterraShippingAnimator : MonoBehaviour
    {
        private AudioHandler _audioHandler;
        private AlterraShippingTarget _mono;

        private void Start()
        {
            this.Animator = this.transform.GetComponent<Animator>();

            if (this.Animator == null)
            {
                QuickLogger.Error("Animator component not found on the GameObject.");
            }

            if (this.Animator != null && this.Animator.enabled == false)
            {
                QuickLogger.Debug("Animator was disabled and now has been enabled");
                this.Animator.enabled = true;
            }

            _audioHandler = new AudioHandler(transform);
        }

        public Animator Animator { get; set; }

        #region Internal Methods
        /// <summary>
        /// Sets the an animator float to a certain value (For use with setting the page on the screen)
        /// </summary>
        /// <param name="stateHash">The hash of the parameter</param>
        /// <param name="value">Float to set</param>
        internal void SetFloatHash(int stateHash, float value)
        {
            this.Animator.SetFloat(stateHash, value);
        }

        /// <summary>
        /// Sets the an animator boolean to a certain value
        /// </summary>
        /// <param name="stateHash">The hash of the parameter</param>
        /// <param name="value">Float to set</param>
        internal void SetBoolHash(int stateHash, bool value)
        {
            if (Animator == null)
            {
                QuickLogger.Error("Animator is null on load");
                this.Animator = this.transform.GetComponent<Animator>();
            }

            this.Animator.SetBool(stateHash, value);
        }

        /// <summary>
        /// Sets the an animator integer to a certain value
        /// </summary>
        /// <param name="stateHash">The hash of the parameter</param>
        /// <param name="value">Float to set</param>
        internal void SetIntHash(int stateHash, int value)
        {
            this.Animator.SetInteger(stateHash, value);
        }

        internal int GetIntHash(int hash)
        {
            return this.Animator.GetInteger(hash);
        }

        internal bool GetBoolHash(int hash)
        {
            return this.Animator.GetBool(hash);
        }

        internal void OpenDoors()
        {
            if (GetBoolHash(_mono.DoorStateHash)) return;
            SetBoolHash(_mono.DoorStateHash, true);
            _audioHandler.PlaySound(true);
        }

        internal void CloseDoors()
        {
            if (GetBoolHash(_mono.DoorStateHash) == false) return;
            SetBoolHash(_mono.DoorStateHash, false);
            _audioHandler.PlaySound(false);
        }

        internal void Initialize(AlterraShippingTarget mono)
        {
            _mono = mono;
        }
        #endregion

    }
}
