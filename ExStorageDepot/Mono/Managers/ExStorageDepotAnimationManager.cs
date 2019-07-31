using FCSCommon.Utilities;
using UnityEngine;

namespace ExStorageDepot.Mono.Managers
{
    internal class ExStorageDepotAnimationManager : MonoBehaviour
    {
        #region Private Members
        private int _driveStateHash;
        private int _screenStateHash;

        #endregion

        #region Public Properties

        public Animator Animator { get; private set; }

        #endregion

        #region Unity Methods
        private void Awake()
        {
            Animator = GetComponentInParent<Animator>();
            Animator.enabled = true;
            _driveStateHash = Animator.StringToHash("ShaftState");
            _screenStateHash = Animator.StringToHash("ScreenState");
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

        internal bool GetBoolHash(int stateHash)
        {
            if (Animator == null)
            {
                QuickLogger.Error("Animator is null on load");
                this.Animator = this.transform.GetComponent<Animator>();
            }

            return Animator.GetBool(stateHash);
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

        internal int GetIntHash(int stateHash)
        {
            return Animator.GetInteger(stateHash);
        }
        #endregion

        public void ToggleScreenState()
        {
            bool value = !GetBoolHash(_screenStateHash);
            SetBoolHash(_screenStateHash, value);
        }
    }
}
