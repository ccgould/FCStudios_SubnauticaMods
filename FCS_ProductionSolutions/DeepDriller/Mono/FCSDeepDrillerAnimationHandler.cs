using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Mono.OreConsumer;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_ProductionSolutions.DeepDriller.Mono
{
    public class FCSDeepDrillerAnimationHandler : MonoBehaviour
    {
        #region Private Methods
        //private AudioHandler _audioHandler;
        private FCSDeepDrillerController _mono;
        private int _drillState;
        private MotorHandler _radarMotor;
        private MotorHandler _drillMotor;
        private AnimationCurve _drillAnimationCurve;
        private GameObject _drillbit;
        private bool _allowedToMove;
        private float maxProgress = 1f;
        private double _timeStartGrowth;

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
            _drillAnimationCurve = new AnimationCurve(new Keyframe(0, 1.686938f), new Keyframe(1, -0.581f));
        }

        private void Update()
        {
            if(!_allowedToMove) return;
            float progress = this.GetProgress();
            this.SetPosition(_drillbit.transform,progress);
            if (progress == 1f)
            {
                //this.SpawnGrownModel();
            }
        }

        public float GetProgress()
        {
            if (this._timeStartGrowth == -1f)
            {
                this.SetProgress(0f);
                return 0f;
            }
            return Mathf.Clamp((float)(DayNightCycle.main.timePassed - (double)this._timeStartGrowth) / 30, 0f, this.maxProgress);
        }

        public void SetProgress(float progress)
        {
            progress = Mathf.Clamp(progress, 0f, this.maxProgress);
            this._timeStartGrowth = DayNightCycle.main.timePassedAsFloat - 30 * progress;
        }

        public void SetPosition(Transform tr, float progress)
        {
            float y = Mathf.Clamp(_drillAnimationCurve.Evaluate(progress), 0, 1.686938f);
            tr.localPosition = new Vector3(tr.localPosition.x, y, tr.localPosition.z);
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

            if (_radarMotor == null)
            {
                _radarMotor = GameObjectHelpers.FindGameObject(gameObject, "radar").AddComponent<MotorHandler>();
                _radarMotor.Initialize(30);
                _radarMotor.StartMotor();
            }

            if (_drillMotor == null)
            {
                _drillbit = GameObjectHelpers.FindGameObject(gameObject, "dill_bit_");
                _drillMotor = _drillbit.AddComponent<MotorHandler>();
                _drillMotor.Initialize(200);
                _drillMotor.SetIncreaseRate(20);
            }
        }

        #endregion

        internal void DrillState(bool state)
        { 
            if (state)
            {
                if (_timeStartGrowth <= 0)
                {
                    _timeStartGrowth = DayNightCycle.main.timePassed;
                }

                _drillMotor.StartMotor();
                _allowedToMove = true;
            }
            else
            {
                _drillMotor.StopMotor();
                _allowedToMove = false;
            }
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
