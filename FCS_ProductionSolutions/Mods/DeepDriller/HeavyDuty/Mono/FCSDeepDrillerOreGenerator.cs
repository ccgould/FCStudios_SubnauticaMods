﻿using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Models.Upgrades;
using FCS_ProductionSolutions.Mods.DeepDriller.Managers;
using FCSCommon.Utilities;
using UnityEngine;
using Random = System.Random;


namespace FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono
{
    /// <summary>
    /// This component handles ore generation after a certain amount of time based off the allowed TechTypes
    /// </summary>
    internal class FCSDeepDrillerOreGenerator : MonoBehaviour
    {
        #region private Properties
        private bool _allowTick;
        private Random _random2;
        private float _passedTime;
        private HashSet<TechType> _focusOres = new HashSet<TechType>();
        private bool IsFocused
        {
            get => _isFocused;
            set
            {
                _isFocused = value;
                OnIsFocusedChanged?.Invoke(value);
            }
        }

        public Action<bool> OnIsFocusedChanged { get; set; }


        //TODO implement weight system
        private Dictionary<TechType, int> OreWeight = new Dictionary<TechType, int>
        {
            { TechType.Titanium, 90},
            { TechType.Quartz, 85},
            { TechType.Salt, 80},
            { TechType.Copper, 75},
            { TechType.Silver, 70},
            { TechType.Lead, 65},
            { TechType.Gold, 60},
            { TechType.UraniniteCrystal, 55},
            { TechType.Magnetite, 50},
            { TechType.Nickel, 45},
            { TechType.AluminumOxide, 40},
            { TechType.Diamond, 35},
            { TechType.Kyanite, 30},
        };

        private float _secondPerItem;
        private const float DayNight = 1200f;
        private int _oresPerDay = 12;
        private DrillSystem _mono;
        private bool _isFocused;
        private bool _blacklistMode;

        #endregion

        #region Internal Properties
        internal List<TechType> AllowedOres { get; set; } = new List<TechType>();
        internal Action OnItemsPerDayChanged { get; set; }
        internal Action OnUsageChange { get; set; }

        internal Action<TechType> OnAddCreated { get; set; }
        #endregion

        /// <summary>
        /// Sets up the ore generator
        /// </summary>
        internal void Initialize(DrillSystem mono)
        {
            _mono = mono;
            _random2 = new Random();
            _secondPerItem = DayNight / _oresPerDay;
            OnItemsPerDayChanged?.Invoke(); //Set Base Data
        }

        private void Update()
        {
            SetAllowTick();

            if (_allowTick)
            {
                _passedTime += DayNightCycle.main.deltaTime;

                if (_passedTime >= _secondPerItem)
                {
                    GenerateOre();
                    _passedTime = 0;
                }
            }
        }

        private void GenerateOre()
        {
            if (!IsFocused)
            {
                if (AllowedOres == null || AllowedOres.Count == 0) return;

                var index = _random2.Next(AllowedOres.Count);
                var item = AllowedOres[index];
                QuickLogger.Debug($"Spawning item {item}", true);
                if (CheckUpgrades(item))return;
                OnAddCreated?.Invoke(item);
                QuickLogger.Debug($"Spawning item {item}", true);

            }
            else
            {
                if(_blacklistMode)
                {
                    var blacklist = AllowedOres.Except(_focusOres);
                    var index = _random2.Next(blacklist.Count());
                    var item = blacklist.ElementAt(index);
                    if (CheckUpgrades(item)) return;
                    OnAddCreated?.Invoke(item);
                    QuickLogger.Debug($"Spawning item {item}", true);
                }
                else
                {
                    var index = _random2.Next(_focusOres.Count);
                    var item = _focusOres.ElementAt(index);
                    if (CheckUpgrades(item)) return;
                    OnAddCreated?.Invoke(item);
                    QuickLogger.Debug($"Spawning item {item}", true);
                }
            }
        }

        internal int FocusCount()
        {
            return _focusOres.Count;
        }

        private void ResetPassTime()
        {
            _passedTime = 0;
        }

        private bool CheckUpgrades(TechType techType)
        {
            var upgrades = _mono.GetUpgrades();
            
            if (upgrades == null) return false;

            foreach (var function in upgrades)
            {
                if (!function.IsEnabled) continue;
                
                if (function.UpgradeType == UpgradeFunctions.MaxOreCount)
                {
                    var functionComp = (MaxOreCountUpgrade)function;
                    if (functionComp.TechType != techType) continue;
                    var itemCount = _mono.DeepDrillerContainer.GetItemCount(functionComp.TechType);
                    if (itemCount >= functionComp.Amount)
                    {
                        QuickLogger.Debug($"Max ore count of {functionComp.Amount} has been reached skipping ore {techType}. Current amount: {itemCount}",true);
                        return true;
                    }
                }
            }

            return false;
        }

        internal void SetAllowTick()
        {
            if (_mono == null) return;
            _allowTick = _mono.IsOperational;
        }

        internal void RemoveFocus(TechType techType)
        {
            _focusOres.Remove(techType);
        }

        internal void AddFocus(TechType techType)
        {
            if (!_focusOres.Contains(techType))
                _focusOres.Add(techType);

#if DEBUG
            //QuickLogger.Debug($"Added Focus: {_focus}");
            QuickLogger.Debug($"Focusing on:");
            for (int i = 0; i < _focusOres.Count; i++)
            {
                QuickLogger.Debug($"{i}: {_focusOres.ElementAt(i)}");
            }
#endif

        }

        internal HashSet<TechType> GetFocusedOres() => _focusOres;

        internal bool GetIsFocused() => IsFocused;

        internal void ToggleFocus()
        {
            IsFocused ^= true;
        }

        internal void SetIsFocus(bool dataIsFocused)
        {
            IsFocused = dataIsFocused;
        }

        internal void SetOresPerDay(int amount)
        {
            _oresPerDay = amount;
            _secondPerItem = DayNight / _oresPerDay;
            OnItemsPerDayChanged?.Invoke();
            QuickLogger.Info($"Deep Driller is now configured to drill {_oresPerDay} ores per day.", true);
        }
        public void Load(HashSet<TechType> dataFocusOres)
        {
            if (dataFocusOres == null) return;
            _focusOres = dataFocusOres;
        }

        public string GetItemsPerDay()
        {
            return _oresPerDay.ToKiloFormat();
        }

        public int GetItemsPerDayInt()
        {
            return _oresPerDay;
        }

        internal void ApplyUpgrade(UpgradeFunction upgrade)
        {
            switch (upgrade.UpgradeType)
            {
                case UpgradeFunctions.OresPerDay:
                    var function = (OresPerDayUpgrade)upgrade;
                    SetOresPerDay(function.OreCount);
                    break;
                case UpgradeFunctions.MaxOreCount:
                    upgrade.ActivateUpdate();
                    break;
                case UpgradeFunctions.SilkTouch:
                    break;
                case UpgradeFunctions.MinOreCount:
                    break;
            }
        }

        public bool GetInBlackListMode()
        {
            return _blacklistMode;
        }

        internal void SetBlackListMode(bool toggleState)
        {
            _blacklistMode = toggleState;
        }

        public bool GetIsDrilling()
        {
            return _allowTick;
        }
    }
}
