using FCSCommon.Utilities;
using UnityEngine;

namespace FCSAlterraShipping.Display
{
    internal class AlterraShippingAnimator : MonoBehaviour
    {
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
        }

        public Animator Animator { get; set; }

        #region Internal Methods
        /// <summary>
        /// Sets the an animator float to a certain value (For use with setting the page on the screen)
        /// </summary>
        /// <param name="stateHash">The hash of the parameter</param>
        /// <param name="value">Float to set</param>
        public void SetFloatHash(int stateHash, float value)
        {
            this.Animator.SetFloat(stateHash, value);
        }

        /// <summary>
        /// Sets the an animator boolean to a certain value
        /// </summary>
        /// <param name="stateHash">The hash of the parameter</param>
        /// <param name="value">Float to set</param>
        public void SetBoolHash(int stateHash, bool value)
        {
            this.Animator.SetBool(stateHash, value);
        }

        /// <summary>
        /// Sets the an animator integer to a certain value
        /// </summary>
        /// <param name="stateHash">The hash of the parameter</param>
        /// <param name="value">Float to set</param>
        public void SetIntHash(int stateHash, int value)
        {
            this.Animator.SetInteger(stateHash, value);
        }
        #endregion
    }
}
