using System;
using System.Collections;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_DeepDriller.Mono.MK2
{
    public class FCSDeepDrillerAnimationHandler : MonoBehaviour
    {
        #region Private Methods
        //private AudioHandler _audioHandler;
        private FCSDeepDrillerController _mono;
        private bool _checkBootup;
        private int _drillState;

        #endregion

        #region Public Properties

        #endregion

        #region Unity Methods   
        private void Start()
        {
            this.Animator = this.transform.GetComponentInChildren<Animator>();

            _drillState = Animator.StringToHash("DrillingState");

            if (this.Animator == null)
            {
                QuickLogger.Error("Animator component not found on the GameObject.");
            }

            if (this.Animator != null && this.Animator.enabled == false)
            {
                QuickLogger.Debug("Animator was disabled and now has been enabled");
                this.Animator.enabled = true;
            }

            //_audioHandler = new AudioHandler(transform);
        }

        private void Update()
        {
            //if (_checkBootup)
            //{
            //    BootUp();
            //}
        }

        #endregion

        #region Public Methods
        public Animator Animator { get; set; }
        #endregion

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
            if (Animator == null)
            {
                this.Animator = this.transform.GetComponent<Animator>();
            }

            if (Animator == null) return;

            this.Animator.SetInteger(stateHash, value);
        }

        internal int GetIntHash(int hash)
        {
            if (Animator == null)
            {
                this.Animator = this.transform.GetComponent<Animator>();
            }

            if (Animator == null) return 0;

            return this.Animator.GetInteger(hash);
        }

        internal bool GetBoolHash(int hash)
        {
            if (Animator == null)
            {
                this.Animator = this.transform.GetComponent<Animator>();
            }

            if (Animator == null) return false;

            return this.Animator.GetBool(hash);
        }

        internal void Initialize(FCSDeepDrillerController mono)
        {
            _mono = mono;
        }

        internal void BootUp(int pageHash)
        {
            StartCoroutine(BootUpCoroutine(pageHash));
        }

        private IEnumerator BootUpCoroutine(int pageHash)
        {
            while (GetIntHash(pageHash) != 2)
            {
                if (GetIntHash(pageHash) != 1)
                {
                    SetIntHash(pageHash, 1);
                }
               
                if (this.Animator.GetCurrentAnimatorStateInfo(3).IsName("DeepDriller_MK2_BootPage") &&
                    this.Animator.GetCurrentAnimatorStateInfo(3).normalizedTime >= 1.0f)
                {
                    QuickLogger.Debug("Going to home.");
                    SetIntHash(pageHash, 2);
                }
                yield return null;
            }
        }

        #endregion

        public void DrillState(bool state)
        { 
            SetIntHash(_drillState,state ? 1:0);
        }
    }
}
