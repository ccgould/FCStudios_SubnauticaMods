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
        #region Public Properties

        private float randomTime;

        private float TimeRemaining;

        private bool _allowTick;

        public string OreGenerated { get; set; }
        public List<string> AllowedOres { get; set; }

        public event Action<string> OnAddCreated;
        public event Action<int> TimeOnUpdate;

        private Random _random;
        private Random _random2;
        private int _minTime;
        private int _maxTime;
        private float _passedTime;

        #endregion

        /// <summary>
        /// Sets up the ore generator
        /// </summary>
        /// <param name="minTime">The minimum amount of time to generate</param>
        /// <param name="maxTime">The maximum amount of time to generate</param>
        public void Start(int minTime, int maxTime)
        {
            _minTime = minTime;
            _maxTime = maxTime + 1; // Added one so the random can chose the maximum number if not wit wont chose the maximum
            _random = new Random();
            _random2 = new Random();
            randomTime = _random.Next(_minTime, _maxTime);
            QuickLogger.Debug($"New Time Goal: {randomTime}");

        }

        private void Update()
        {
            //QuickLogger.Debug($"AllowTick = {_allowTick} || PassedTime = {_passedTime} || AllowedOres = {AllowedOres?.Count}");

            if (_allowTick)
            {
                if (_minTime <= 0 || _maxTime <= 0)
                {
                    QuickLogger.Error($"{nameof(OreGenerator)}: MaxTime or MinTime is lower than or equal to 0");
                    return;
                }

                _passedTime += DayNightCycle.main.deltaTime;

                if (_passedTime >= randomTime / 0.016667)
                {
                    GenerateOre();
                }

                var timeLeft = _maxTime - (_passedTime * 0.016667);

                TimeOnUpdate?.Invoke(Convert.ToInt32(timeLeft));

            }
        }

        private void GenerateOre()
        {
            QuickLogger.Info("1");
            _random2.Next(AllowedOres.Count);

            QuickLogger.Info("2");
            if (AllowedOres?.Count == 0) return;

            QuickLogger.Info("3");
            var index = _random2.Next(AllowedOres.Count);

            QuickLogger.Info("4");
            var item = AllowedOres[index];

            QuickLogger.Info($"5 {item}");

            OnAddCreated?.Invoke(item);

            QuickLogger.Info("6");
            randomTime = _random.Next(_minTime, _maxTime);
            QuickLogger.Debug($"New Time Goal: {randomTime}");
            QuickLogger.Info("7");
            _passedTime = 0;

            QuickLogger.Info("8");

        }

        internal void SetAllowTick(bool value)
        {
            _allowTick = value;
        }
    }
}
