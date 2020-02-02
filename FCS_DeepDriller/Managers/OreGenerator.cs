using FCS_DeepDriller.Mono;
using FCSCommon.Utilities;
using FCSCommon.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;


namespace FCSAlterraIndustrialSolutions.Models.Controllers.Logic
{
    /// <summary>
    /// This component handles ore generation after a certain amount of time based off the allowed TechTypes
    /// </summary>
    internal class OreGenerator : MonoBehaviour
    {
        #region private Properties
        private bool _allowTick;
        private Random _random2;
        private float _passedTime;
        private TechType _focus;
        private bool _isFocused;
        private float _secondPerItem;
        private FCSDeepDrillerController _mono;
        private const float DayNight = 1200f;
        private int _oresPerDay = 12;
        #endregion

        #region Internal Properties
        internal List<TechType> AllowedOres { get; set; }
        internal event Action<TechType> OnAddCreated;
        #endregion

        /// <summary>
        /// Sets up the ore generator
        /// </summary>
        internal void Initialize(FCSDeepDrillerController mono)
        {
            _mono = mono;
            _random2 = new Random();
            _secondPerItem = DayNight / _oresPerDay;
        }

        private void Update()
        {
            //QuickLogger.Debug($"PassedTime = {_passedTime} ||SecPerItem {_secondPerItem} || AllowedOres = {AllowedOres.Count} " +
            //$"|| IsFocused {_isFocused} || Focus {_focus} || Allow Tick {_allowTick}");

            SetAllowTick();


            if (_allowTick)
            {
                _passedTime += DayNightCycle.main.deltaTime;

                if (_passedTime >= _secondPerItem)
                {
                    GenerateOre();
                }
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
                OnAddCreated?.Invoke(item);
                QuickLogger.Debug($"Spawning item {item}", true);

            }
            else
            {
                item = _focus;

                if (_focus != TechType.None)
                {
                    OnAddCreated?.Invoke(item);
                }

                QuickLogger.Debug($"Spawning focus item {_focus}", true);
            }

            _passedTime = 0;
        }

        internal void SetAllowTick()
        {
            if (_mono.PowerManager.GetPowerState() == FCSPowerStates.Powered && !_mono.DeepDrillerContainer.IsContainerFull)
            {
                _allowTick = true;
            }
            else
            {
                _allowTick = false;
            }

            QuickLogger.Debug($"Allowed Tick: {_allowTick}",true);
        }

        internal void RemoveFocus()
        {
            _focus = TechType.None;
            _isFocused = false;
            QuickLogger.Debug($"Focus has been removed!", true);
        }

        internal void SetFocus(TechType techType)
        {
            _focus = techType;
            QuickLogger.Debug($"Focus set to {_focus}");
        }

        internal TechType GetFocus() => _focus;

        internal bool GetIsFocused() => _isFocused;

        internal void ToggleFocus()
        {
            _isFocused ^= true;

            QuickLogger.Debug(_isFocused ? $"Setting focus item {_focus}" : $"Disabling focus.", true);
        }

        internal void SetIsFocus(bool dataIsFocused)
        {
            _isFocused = dataIsFocused;
        }

        internal void SetOresPerDay(int amount)
        {
            _oresPerDay = amount;
            _secondPerItem = DayNight / _oresPerDay;
            QuickLogger.Info($"Deep Driller is now configured to drill {_oresPerDay} ores per day.", true);
        }
    }
}
