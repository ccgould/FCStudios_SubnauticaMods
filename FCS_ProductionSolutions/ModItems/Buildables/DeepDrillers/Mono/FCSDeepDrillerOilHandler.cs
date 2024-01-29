using FCS_AlterraHub.Core.Extensions;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Interfaces;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Base;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono;
internal class FCSDeepDrillerOilHandler : MonoBehaviour, IFCSStorage
{
    [SerializeField] private DrillSystem drillSystem;
    public int GetContainerFreeSpace { get; }
    public Action<int, int> OnContainerUpdate { get; set; }
    public Action<FCSDevice, TechType> OnContainerAddItem { get; set; }
    public Action<FCSDevice, TechType> OnContainerRemoveItem { get; set; }
    public bool IsFull { get; }
    private const float KDayInSeconds = 1200f;
    private readonly float _setOilTime = KDayInSeconds * Plugin.Configuration.DDOilTimePeriodInDays;
    private readonly float _lubricantRefillAmount = KDayInSeconds * Plugin.Configuration.DDOilRestoresInDays;
    private float _oil;
    private float _elapsed;

    private void Update()
    {
        if (drillSystem == null || drillSystem.IsBreakSet()) return;
        
        _elapsed += DayNightCycle.main.deltaTime;

        if (_elapsed >= 1f)
        {
            if (_oil > 0 && drillSystem.IsOperational())
            {
                var lusePerDay = KDayInSeconds + 16 * (drillSystem.GetOresPerDayCountInt() - Plugin.Configuration.DDDefaultOrePerDay);
                var lusePerSecond = lusePerDay / 1200;
                _oil -= lusePerSecond;
            }

            _elapsed %= 1f;
            drillSystem.OnOilLevelChange?.Invoke(GetOilPercent());
        }
    }

    internal void Initialize(DrillSystem mono)
    {
        drillSystem = mono;
        _oil = 0f;

    }

    internal void SetOilTimeLeft(float amount)
    {
        _oil = amount;
    }

    internal float GetOilTimeLeft()
    {
        return _oil;
    }

    internal void ReplenishOil()
    {
        _oil = Mathf.Clamp(_oil + (_lubricantRefillAmount), 0, _setOilTime);
    }

    internal float GetOilPercent()
    {
        return _oil / _setOilTime;
    }

    public bool CanBeStored(int amount, TechType techType)
    {
        //return _oil + KDayInSeconds <= _setOilTime;
        return _oil + KDayInSeconds * amount <= _setOilTime;
    }

    public bool AddItemToContainer(InventoryItem item)
    {
        ReplenishOil();
        Destroy(item.item.gameObject);
        return true;
    }

    public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
    {
        return IsAllowedToAdd(pickupable.GetTechType(), verbose);
    }

    public bool IsAllowedToAdd(TechType techType, bool verbose)
    {
        if (techType != TechType.Lubricant)
        {
            QuickLogger.Message(AuxPatchers.NotAllowedItem(), true);
            return false;
        }

        if (!CanBeStored(drillSystem.GetOilDumpContainter().GetCount(), TechType.Lubricant))
        {
            QuickLogger.Message(AuxPatchers.OilTankNotFormatEmpty(TimeTilRefuel()), true);
            return false;
        }

        return true;
    }

    public bool IsAllowedToRemoveItems()
    {
        return false;
    }

    public Pickupable RemoveItemFromContainer(TechType techType)
    {
        if (_oil - KDayInSeconds >= 0)
        {
            var itemTask = new TaskResult<InventoryItem>();
            CoroutineManager.WaitCoroutine(techType.ToInventoryItem(itemTask));
            return itemTask.Get().item;
        }

        return null;
    }

    internal string TimeTilRefuel()
    {
        var mod = _oil % KDayInSeconds;
        return TimeConverters.SecondsToHMS(mod);
    }

    public Dictionary<TechType, int> GetItemsWithin()
    {
        if (Mathf.Approximately(_oil, 0)) return null;

        var result = _oil / KDayInSeconds;
        int amount = Convert.ToInt32(Math.Floor(result));

        return new Dictionary<TechType, int>
            {
                {TechType.Lubricant,amount}
            };
    }

    public bool ContainsItem(TechType techType)
    {
        return techType == TechType.Lubricant && !(_oil < KDayInSeconds);
    }

    public ItemsContainer ItemsContainer { get; set; }
    public int StorageCount()
    {
        return Mathf.RoundToInt(_oil / _lubricantRefillAmount);
    }

    internal bool HasOil()
    {
        return _oil > 0;
    }

    public int GetCount()
    {
        throw new NotImplementedException();
    }
}
