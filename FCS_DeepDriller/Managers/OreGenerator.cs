using System;
using System.Collections.Generic;
using System.Linq;
using FCS_DeepDriller.Buildable.MK2;
using FCS_DeepDriller.Configuration;
using FCS_DeepDriller.Model.Upgrades;
using FCS_DeepDriller.Mono.MK2;
using FCSCommon.Enums;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using UnityEngine;
using Random = System.Random;


namespace FCS_DeepDriller.Managers
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

        private float _secondPerItem;
        private const float DayNight = 1200f;
        private int _oresPerDay = 12;
        private FCSDeepDrillerController _mono;
        private bool _isFocused;
        #endregion

        #region Internal Properties
        internal List<TechType> AllowedOres { get; set; } = new List<TechType>();
        internal Action OnItemsPerDayChanged { get; set; }
        internal Action OnUsageChange { get; set; }

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
                var index = _random2.Next(_focusOres.Count);
                var item = _focusOres.ElementAt(index);
                if (CheckUpgrades(item))return;
                OnAddCreated?.Invoke(item);
                QuickLogger.Debug($"Spawning item {item}", true);
            }
        }

        private void ResetPassTime()
        {
            _passedTime = 0;
        }

        private bool CheckUpgrades(TechType techType)
        {
            foreach (var function in _mono.UpgradeManager.Upgrades)
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
            if (_mono?.PowerManager == null || _mono?.DeepDrillerContainer == null) return;

            _allowTick = _mono.IsOperational();
        }

        internal void RemoveFocus(TechType focus)
        {
            IsFocused = false;
            QuickLogger.Debug($"Focus has been removed!", true);
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

        internal HashSet<TechType> GetFocuses()
        {
            return _focusOres;
        }

        public void Load(HashSet<TechType> dataFocusOres)
        {
            _focusOres = dataFocusOres;
        }

        public string GetItemsPerDay()
        {
            return string.Format(FCSDeepDrillerBuildable.ItemsPerDayFormat(), _oresPerDay);
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
            _mono.PowerManager.UpdatePowerUsage();
        }

        internal void UpdatePowerUsage()
        {
            OnUsageChange?.Invoke();
        }
    }
}
