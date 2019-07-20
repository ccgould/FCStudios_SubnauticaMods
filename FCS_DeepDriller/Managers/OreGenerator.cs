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

        public float TimeRemaining { get; set; }

        public bool AllowTick { get; set; }



        public string OreGenerated { get; set; }
        public List<string> AllowedOres { get; set; }

        public event Action<string> OnAddCreated;
        public event Action<int> TimeOnupdate;

        private Random _random;
        private Random _random2;
        private int _minTime;
        private int _maxTime;
        private float _passedTime;

        #endregion

        public void Start(int minTime, int maxTime)
        {
            _minTime = minTime;
            _maxTime = maxTime;
            _random = new Random();
            _random2 = new Random();
            randomTime = _random.Next(minTime, maxTime);
        }

        private void Update()
        {
            if (AllowTick)
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

                TimeOnupdate?.Invoke(Convert.ToInt32(timeLeft));
                //Log.Info($"AllowTick = {AllowTick} || PassedTime = {_passedTime} || AllowedOres = {AllowedOres?.Count}");
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

            QuickLogger.Info("7");
            _passedTime = 0;

            QuickLogger.Info("8");

        }
    }
}
