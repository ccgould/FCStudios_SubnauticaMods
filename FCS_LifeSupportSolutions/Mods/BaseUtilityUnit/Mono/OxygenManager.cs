using System;
using FCS_LifeSupportSolutions.Patches;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_LifeSupportSolutions.Mods.BaseUtilityUnit.Mono
{
    internal class OxygenManager : MonoBehaviour
    {
        #region Public Properties

        public Action<float, float> OnOxygenUpdated { get; set; }

        #endregion

        #region Private Fields

        private float _o2Level;
        private const float TankCapacity = 200f;
        private const float AmountPerSecond = 4f;
        private BaseUtilityUnitController _mono;
        private float _timeLeft = 1;

        #endregion

        #region Unity Methods

        private void Update()
        {
            _timeLeft -= DayNightCycle.main.deltaTime;
            if (_timeLeft < 0)
            {
                GenerateOxygen();
                _timeLeft = 1f;
            }
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Resets the tank O2 Level to full capacity value
        /// </summary>
        private void FillTank()
        {
            _o2Level = TankCapacity;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Setup the monobehaviour for operation
        /// </summary>
        /// <param name="mono"></param>
        internal void Initialize(BaseUtilityUnitController mono)
        {
            _mono = mono;
        }
        
        /// <summary>
        /// Removes oxygen from the unit.
        /// </summary>
        /// <param name="getOxygenPerBreath"></param>
        internal bool RemoveOxygen(float getOxygenPerBreath)
        {
            if (!_mono.IsConstructed) return false;

            float num = Mathf.Min(getOxygenPerBreath, _o2Level);
            _o2Level = Mathf.Max(0f, this._o2Level - num);
            if (_o2Level < 1)
            {
                _o2Level = 0;
            }
            OnOxygenUpdated?.Invoke(_o2Level, _o2Level / TankCapacity);
            QuickLogger.Debug($"Unit Oxygen Level: {_o2Level}", true);
            return true;
        }

        /// <summary>
        /// The current oxygen level of the unit.
        /// </summary>
        /// <returns></returns>
        internal float GetO2Level()
        {
            return _o2Level;
        }

        /// <summary>
        /// Set the O2 level of the unit.
        /// </summary>
        /// <param name="amount"></param>
        internal void SetO2Level(float amount)
        {
            _o2Level = Mathf.Clamp(amount, 0, TankCapacity);
            OnOxygenUpdated?.Invoke(_o2Level, _o2Level / TankCapacity);
        }

        /// <summary>
        /// Generates oxygen for the unit
        /// </summary>
        internal void GenerateOxygen()
        {
            if (_mono.Manager.HasEnoughPower(_mono.GetPowerUsage()))
            {
                SetO2Level(_o2Level + AmountPerSecond);
            }
        }

        /// <summary>
        /// Gives the player oxygen directly from the tank.
        /// </summary>
        internal void GivePlayerO2()
        {
            if (_o2Level <= 0 || !_mono.IsConstructed) return;

            var o2Manager = Player.main.oxygenMgr;

            float playerO2Request;
            if (QPatch.IsRefillableOxygenTanksInstalled && Player.main.oxygenMgr.HasOxygenTank())
            {
                playerO2Request = Player_CanBreathe.DefaultO2Level - o2Manager.GetOxygenAvailable();
            }
            else
            {
                playerO2Request = o2Manager.GetOxygenCapacity() - o2Manager.GetOxygenAvailable();
            }
            
            if (_o2Level >= playerO2Request)
            {
                _o2Level -= playerO2Request;
                o2Manager.AddOxygen(Mathf.Abs(playerO2Request));
                return;
            }

            var playerO2RequestRemainder = Mathf.Min(_o2Level, playerO2Request);
            _o2Level -= playerO2RequestRemainder;
            o2Manager.AddOxygen(Mathf.Abs(playerO2RequestRemainder));
            QuickLogger.Debug($"O2 Level: {_o2Level} || Tank Level {playerO2RequestRemainder}", true);
        }
        
        /// <summary>
        /// Calculates the percentage of oxygen available and returns a int.
        /// </summary>
        /// <returns>An <see cref="int"/> of a percentage ex. 50</returns>
        internal int GetO2LevelPercentageInt()
        {
            return Mathf.RoundToInt((100 * _o2Level) / TankCapacity);
        }

        /// <summary>
        /// Calculates the percentage of oxygen available and returns a float.
        /// </summary>
        /// <returns>An <see cref="float"/> of a percentage ex. 0.5</returns>
        internal float GetO2LevelPercentageFloat()
        {
            return _o2Level / TankCapacity;
        }

        #endregion
    }
}