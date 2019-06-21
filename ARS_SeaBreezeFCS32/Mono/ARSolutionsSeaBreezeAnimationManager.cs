using FCSCommon.Utilities;
using UnityEngine;

namespace ARS_SeaBreezeFCS32.Mono
{
    internal class ARSolutionsSeaBreezeAnimationManager : MonoBehaviour
    {
        #region Private Members
        private int _driveStateHash;
        #endregion

        #region Public Properties

        public Animator Animator { get; private set; }

        #endregion

        #region Unity Methods
        private void Awake()
        {
            Animator = GetComponentInParent<Animator>();
            Animator.enabled = true;
            _driveStateHash = UnityEngine.Animator.StringToHash("DriveState");
        }
        #endregion

        #region Internal Methods
        internal void ToggleDriveState()
        {
            var state = Animator.GetBool(_driveStateHash);

            SetBoolHash(_driveStateHash, !state);
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

        internal void SetIntHash(int stateHash, int value)
        {
            if (Animator == null)
            {
                QuickLogger.Error("Animator is null on load");
                this.Animator = this.transform.GetComponent<Animator>();
            }

            this.Animator.SetInteger(stateHash, value);
        }

        internal void SetFloatHash(int stateHash, float value)
        {
            if (Animator == null)
            {
                QuickLogger.Error("Animator is null on load");
                this.Animator = this.transform.GetComponent<Animator>();
            }

            QuickLogger.Debug($"Set Hash {value}", true);
            this.Animator.SetFloat(stateHash, value);
        }
        #endregion

        public int GetIntHash(int stateHash)
        {
            return Animator.GetInteger(stateHash);
        }
    }
}
