using FCS_DeepDriller.Enumerators;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;


namespace FCSAlterraIndustrialSolutions.Models.Controllers.Logic
{
    /// <summary>
    /// This component handles ore generation after a certain amount of time based off the allowed TechTypes
    /// </summary>
    public class OreGenerator : MonoBehaviour
    {
        #region private Properties

        private float _randomTime;
        private bool _allowTick;
        private Random _random;
        private Random _random2;
        private int _minTime;
        private int _maxTime;
        private float _passedTime;
        private TechType _focus;
        private bool _isFocused;

        #endregion

        #region Internal Properties
        internal List<TechType> AllowedOres { get; set; }
        internal event Action<TechType> OnAddCreated;
        internal event Action<int> TimeOnUpdate;
        #endregion

        /// <summary>
        /// Sets up the ore generator
        /// </summary>
        /// <param name="minTime">The minimum amount of time to generate</param>
        /// <param name="maxTime">The maximum amount of time to generate</param>
        public void Initialize(int minTime, int maxTime)
        {
            _minTime = minTime;
            _maxTime = maxTime + 1; // Added one so the random can chose the maximum number if not wit wont chose the maximum
            _random = new Random();
            _random2 = new Random();
            _randomTime = _random.Next(_minTime, _maxTime);
            QuickLogger.Debug($"New Time Goal: {_randomTime}");

        }

        private void Update()
        {
            QuickLogger.Debug($"AllowTick = {_allowTick} || PassedTime = {_passedTime} || AllowedOres = {AllowedOres?.Count}");

            if (_allowTick)
            {
                if (_minTime <= 0 || _maxTime <= 0)
                {
                    QuickLogger.Error($"{nameof(OreGenerator)}: MaxTime or MinTime is lower than or equal to 0");
                    return;
                }

                _passedTime += DayNightCycle.main.deltaTime;

                if (_passedTime >= _randomTime / 0.016667)
                {
                    GenerateOre();
                }

                var timeLeft = _randomTime - (_passedTime * 0.016667);

                TimeOnUpdate?.Invoke(Convert.ToInt32(timeLeft));

            }
        }

        private void GenerateOre()
        {
            TechType item;

            if (!_isFocused)
            {
                if (AllowedOres == null || AllowedOres.Count == 0) return;

                _random2.Next(AllowedOres.Count);
                var index = _random2.Next(AllowedOres.Count);
                item = AllowedOres[index];

            }
            else
            {
                item = _focus;
                QuickLogger.Debug($"Spawning focus item {_focus}");
            }

            OnAddCreated?.Invoke(item);
            _randomTime = _random.Next(_minTime, _maxTime);
            QuickLogger.Debug($"New Time Goal: {_randomTime}");
            _passedTime = 0;
        }

        internal void SetAllowTick(bool value)
        {
            _allowTick = value;
        }

        public void RemoveFocus()
        {
            _focus = TechType.None;
            _isFocused = false;
        }

        public void SetModule(DeepDrillModules module)
        {
            if (module == DeepDrillModules.Focus)
            {
                //TODO Get focus material from selection
                _focus = TechType.Silver;
                _isFocused = true;
                QuickLogger.Debug($"Setting focus item {_focus}");
            }
        }

        internal void SetFocus(TechType techType)
        {
            _focus = techType;
        }

        internal TechType GetFocus() => _focus;

        internal void SetIsFocused(bool value)
        {
            _isFocused = value;
        }

        internal bool GetIsFocused() => _isFocused;
    }
}
