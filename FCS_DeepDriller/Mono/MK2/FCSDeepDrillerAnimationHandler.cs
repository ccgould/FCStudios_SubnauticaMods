using System;
using System.Collections;
using FCSCommon.Utilities;
using FCSTechFabricator.Enums;
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
        private bool _booting;
        private IEnumerator _bootRoutine;

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
            _booting = true;
            _bootRoutine = BootUpCoroutine(pageHash);
            StartCoroutine(_bootRoutine);
        }

        private IEnumerator BootUpCoroutine(int pageHash)
        {
            while (_booting)
            {
                if (GetIntHash(pageHash) != 1)
                {
                    SetIntHash(pageHash, 1);
                }
               
                if (this.Animator.GetCurrentAnimatorStateInfo(3).IsName("DeepDriller_MK2_BootPage") &&
                    this.Animator.GetCurrentAnimatorStateInfo(3).normalizedTime >= 1.0f)
                {
                    if (_mono.DeepDrillerPowerManager.GetPowerState() == FCSPowerStates.Tripped)
                    {
                        QuickLogger.Debug("Going to Powered Off.");
                        SetIntHash(pageHash, 6);
                    }
                    else
                    {
                        QuickLogger.Debug("Going to home.");
                        SetIntHash(pageHash, 2);
                    }
                    StopCoroutine(_bootRoutine);
                    _booting = false;
                }
                yield return null;
            }
        }

        #endregion

        internal void DrillState(bool state)
        { 
            SetIntHash(_drillState,state ? 1:0);
        }

        internal bool AnimationIsPlaying(string animationName)
        {
            if (Animator == null) return false;

            var result = Animator.GetCurrentAnimatorStateInfo(4).IsName(animationName);





            QuickLogger.Debug($"Animation with hash {animationName} is playing returned {result}",true);

            return result;
        }
    }
}
