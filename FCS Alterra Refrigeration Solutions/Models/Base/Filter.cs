using FCS_Alterra_Refrigeration_Solutions.Logging;
using System;
using UnityEngine;

namespace FCS_Alterra_Refrigeration_Solutions.Models.Base
{
    public class FilterArgs : EventArgs
    {
        public string CurrentTime { get; set; }
    }

    public class Filter : MonoBehaviour
    {
        /// <summary>
        /// The remaining time in seconds
        /// </summary>
        public string RemainingTime { get; private set; }

        /// <summary>
        /// The max time for the count down
        /// </summary>
        public float MaxTime { get; set; }

        /// <summary>
        /// The Name of the Filter
        /// </summary>
        public string FilterName { get; set; } = "Filter";

        public bool IsExpired { get; set; } = false;

        /// <summary>
        /// Event that is triggered when the timer ends
        /// </summary>
        public EventHandler TimerEnd;

        /// <summary>
        /// Event that is triggered when the timer start
        /// </summary>
        public EventHandler TimerStart;

        /// <summary>
        /// Event that is triggered when the timer ticks
        /// </summary>
        public EventHandler<FilterArgs> TimerTick;

        private bool canCount = true;

        private bool _doOnce;

        private bool _runTimer;

        private void Awake()
        {
            Log.Info($"//  ==========  {transform.name}  ========== //");
            MaxTime = transform.name.Equals("ShortTermFilter(Clone)") ? 2400f : 36000f;
            FilterName = transform.name;
        }

        /// <summary>
        /// Starts the timer
        /// </summary>
        public void StartTimer()
        {
            _runTimer = true;
        }

        /// <summary>
        /// Resets the filter's timer
        /// </summary>
        public void Reset()
        {
            //_timer = MaxTime;
            canCount = true;
            _doOnce = false;
        }

        private string SecondsToMinutes(float seconds)
        {
            var t = TimeSpan.FromSeconds(seconds);

            return $"{t.Hours:D2}h:{t.Minutes:D2}m:{t.Seconds:D2}s";
        }

        public void UpdateTimer()
        {

            if (!_runTimer) return;

            if (MaxTime >= 0.0f && canCount)
            {
                MaxTime -= DayNightCycle.main.deltaTime;
                RemainingTime = SecondsToMinutes(MaxTime);
                //Log.Info(RemainingTime);
                OnTimerTick(RemainingTime);
            }
            else if (MaxTime <= 0.0f && !_doOnce)
            {
                canCount = false;
                _doOnce = true;
                RemainingTime = SecondsToMinutes(0);
                MaxTime = 0.0f;
                OnTimerTick(RemainingTime);
                OnTimerEnd();
            }
        }

        protected virtual void OnTimerStart()
        {
            TimerStart?.Invoke(this, EventArgs.Empty);

        }

        protected virtual void OnTimerEnd()
        {
            TimerEnd?.Invoke(this, EventArgs.Empty);
            IsExpired = true;
        }

        protected virtual void OnTimerTick(string args)
        {
            TimerTick?.Invoke(this, new FilterArgs { CurrentTime = args });
        }
    }
}
