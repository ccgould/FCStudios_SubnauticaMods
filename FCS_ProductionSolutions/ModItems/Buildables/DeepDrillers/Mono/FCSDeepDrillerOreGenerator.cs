using FCS_AlterraHub.Core.Helpers;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Enumerators;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Models;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Models.Upgrades;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Base;
using FCSCommon.Utilities;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono;
internal class FCSDeepDrillerOreGenerator : MonoBehaviour
{
    #region private Properties
    private bool _allowTick;
    private System.Random _random2;
    private float _passedTime;
    private HashSet<TechType> _focusOres = new HashSet<TechType>();
    [SerializeField] private FCSDeepDrillerContainer _container;
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
    [SerializeField] private DrillSystem drillsystem;
    private bool _isFocused;
    private bool _blacklistMode;

    #endregion

    #region Internal Properties
    internal List<TechType> AllowedOres { get; set; } = new List<TechType>();
    internal Action OnItemsPerDayChanged { get; set; }
    internal Action OnUsageChange { get; set; }
    public string CurrentBiome { get; private set; }

    //[System.Serializable]
    //public class MyTechTypeEvent : UnityEvent<TechType>
    //{
    //}

    //[SerializeField] private MyTechTypeEvent OnOreCreated;
    private bool _noBiomeMessageSent;
    private bool _biomeFoundMessageSent;
    private List<TechType> _bioData;

    #endregion

    private void Awake()
    {
        _random2 = new System.Random();
        _secondPerItem = DayNight / _oresPerDay;
        OnItemsPerDayChanged?.Invoke(); //Set Base Data
    }

    private IEnumerator TryGetLoot(Action callBack = null)
    {
        QuickLogger.Debug("In TryGetLoot");


        while (AllowedOres.Count <= 0)
        {
            if (string.IsNullOrEmpty(CurrentBiome))
            {
                if (!_noBiomeMessageSent)
                {
                    QuickLogger.Info($"No biome Found trying to find biome");
                    _noBiomeMessageSent = true;
                }

                CurrentBiome = drillsystem.GetBiomeDetector().GetCurrentBiome();

            }
            else
            {
                if (!_biomeFoundMessageSent)
                {
                    QuickLogger.Info($"biome Found: {CurrentBiome}");
                    _biomeFoundMessageSent = true;
                }

                var loot = Helpers.WorldHelpers.GetBiomeData(ref _bioData, CurrentBiome, transform);

                if (loot != null)
                {
                    AllowedOres = loot;
                    callBack?.Invoke();
                }
            }
            yield return null;
        }
        yield return 0;
    }

    private void Update()
    {
        SetAllowTick();

        if (_allowTick)
        {
            _passedTime += DayNightCycle.main.deltaTime;

            if (_passedTime >= _secondPerItem)
            {
                StartCoroutine(GenerateOre());
                _passedTime = 0;
            }
        }
    }
    internal void TryGetLootData(Action callBack)
    {
        StartCoroutine(TryGetLoot(callBack));
    }

    private IEnumerator GenerateOre()
    {
        if (!IsFocused)
        {

            if(AllowedOres.Count <= 0)
            {
                yield return StartCoroutine(TryGetLoot());
            }

            if (AllowedOres == null || AllowedOres.Count == 0) yield return null;

            var index = _random2.Next(AllowedOres.Count);
            var item = AllowedOres[index];
            QuickLogger.Debug($"Spawning item {item}", true);
            if (CheckUpgrades(item)) yield return null;
            _container.AddItemToContainer(item);

        }
        else
        {
            if (_blacklistMode)
            {
                var blacklist = AllowedOres.Except(_focusOres);
                var index = _random2.Next(blacklist.Count());
                var item = blacklist.ElementAt(index);
                if (CheckUpgrades(item)) yield return null;
                _container.AddItemToContainer(item);
                QuickLogger.Debug($"Spawning item {item}", true);
            }
            else
            {
                var index = _random2.Next(_focusOres.Count);
                var item = _focusOres.ElementAt(index);
                if (CheckUpgrades(item)) yield return null;
                _container.AddItemToContainer(item);
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
        var upgrades = drillsystem.GetUpgrades();

        if (upgrades == null) return false;

        foreach (var function in upgrades)
        {
            if (!function.IsEnabled) continue;

            if (function.UpgradeType == UpgradeFunctions.MaxOreCount)
            {
                var functionComp = (MaxOreCountUpgrade)function;
                if (functionComp.TechType != techType) continue;
                var itemCount = drillsystem.GetDDContainer().GetItemCount(functionComp.TechType);
                if (itemCount >= functionComp.Amount)
                {
                    QuickLogger.Debug($"Max ore count of {functionComp.Amount} has been reached skipping ore {techType}. Current amount: {itemCount}", true);
                    return true;
                }
            }
        }

        return false;
    }

    internal void SetAllowTick()
    {
        if (drillsystem == null) return;
        _allowTick = drillsystem.IsOperational();
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

    internal void SetFocus(bool value)
    {
        IsFocused = value;
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

    internal void GetAllowedOres(Action<List<TechType>> callBack)
    {
        TryGetLootData(() =>
        {
            callBack?.Invoke(AllowedOres);
        });
        
    }
}
