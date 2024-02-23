using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Systems;
using FCS_AlterraHub.Models.Enumerators;
using System;
using System.Collections.Generic;
using System.Linq;
using static FCS_EnergySolutions.Configuration.SaveData;
using UnityEngine;
using FCS_EnergySolutions.ModItems.Buildables.AlterraGen.Helpers;
using FCS_AlterraHub.Models.Mono;

namespace FCS_EnergySolutions.ModItems.Buildables.AlterraGen.Mono;
internal class AlterraGenPowerManager : FCSPowerManager
{
    [SerializeField] private PowerRelay _powerRelay;
    [SerializeField] private PowerSource _powerSource;
    [SerializeField] private float powerPerSecond = 1.167f; // Old Value 0.8333333f;
    [SerializeField] private AlterraGenController controller;
    [SerializeField] private int maxSlots = 9;
    [SerializeField] private FCSStorage storageContainer;

    private float _toConsume;
    private readonly List<TechType> _toRemove = new();
    private FCSPowerStates _prevPowerState;
    private float _storedPower;
    private FCSPowerStates _powerState = FCSPowerStates.Powered;
    public float _targetEnergyValue { get; private set; }
    private float _multiplier;
    public Action<int, int> OnContainerUpdate { get; set; }
    internal bool ProducingPower
    {
        get
        {
            var value = controller != null && controller.IsConstructed && storageContainer.container.Count() > 0 && PowerState != FCSPowerStates.Tripped && UWEHelpers.RequiresPower();
            return value;
        }
    }
    internal Action<AlterraGenPowerManager> OnPowerUpdateCycle { get; set; }

    internal FCSPowerStates PowerState
    {
        get => _powerState;
        set
        {
            _powerState = value;
        }
    }

    private void Start()
    {
        controller = gameObject.GetComponent<AlterraGenController>();
        this._powerRelay = base.gameObject.GetComponentInParent<PowerRelay>();
        if (this._powerRelay == null)
        {
            Debug.LogError("AlterraGen could not find PowerRelay", this);
        }
        this._powerSource = base.GetComponent<PowerSource>();
        if (this._powerSource == null)
        {
            Debug.LogError("AlterraGen could not find PowerSource", this);
        }

        InvokeRepeating(nameof(UpdateSubscribers), 0.5f, 0.5f);
    }

    private void UpdateSubscribers()
    {
        OnPowerUpdateCycle?.Invoke(this);
    }

    private void Update()
    {
        if (this.ProducingPower)
        {
            float num = powerPerSecond * DayNightCycle.main.deltaTime;

            float num2 = _powerSource.maxPower - _powerSource.power;
            if (num2 > 0f)
            {
                if (num2 < num)
                {
                    num = num2;
                }
                float amount = this.ProducePower(num);
                _powerSource.AddEnergy(amount, out var num3);
            }
        }
    }

    private float GetMultiplier(TechType techType)
    {
        var multiplier = 0f;

        var size = CraftData.GetItemSize(techType);

        if (size.x > 1 || size.y > 1)
        {
            multiplier = 2.2f;
        }
        else
        {
            multiplier = 1f;
        }
        return multiplier;
    }

    private float ProducePower(float requested)
    {
        float modifiedAmount = 0f;
        if (requested > 0f && storageContainer.container.Count() > 0)
        {
            _toConsume += requested;

            modifiedAmount = requested;

            foreach (InventoryItem item in storageContainer.container)
            {
                if (AlterraGenHelpers.GetBioChargeValues().TryGetValue(item.item.GetTechType(), out var value))
                {
                    _multiplier = GetMultiplier(item.item.GetTechType());
                    _targetEnergyValue = _multiplier * value;

                    if (_toConsume >= _targetEnergyValue)
                    {
                        _toConsume -= _targetEnergyValue;
                        _toRemove.Add(item.item.GetTechType());
                    }
                }
            }


            for (int i = _toRemove.Count - 1; i >= 0; i--)
            {
                TechType techType = _toRemove[i];
                storageContainer.container.RemoveItem(techType);
                OnContainerUpdate?.Invoke(storageContainer.container.Count(), maxSlots);
            }

            _toRemove.Clear();

            if (storageContainer.container.Count() == 0)
            {
                modifiedAmount -= _toConsume;
                _toConsume = 0f;
            }
        }
        return modifiedAmount;
    }

    #region IFCSStorage

    //public bool CanBeStored(int amount, TechType techType)
    //{

    //    if (!AlterraGenHelpers.GetBioChargeValues().ContainsKey(techType))
    //    {
    //        QuickLogger.ModMessage(AlterraGenBuildable.NotAllowedItem());
    //        return false;
    //    }

    //    var storageResult = !IsFull && amount + _container.Count <= MaxSlots;

    //    return storageResult;
    //}

    //public bool AddItemToContainer(InventoryItem item)
    //{
    //    _container.Add(item.item.GetTechType());
    //    Destroy(item.item.gameObject);
    //    OnContainerUpdate?.Invoke(_container.Count, MaxSlots);

    //    return true;
    //}

    //public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
    //{
    //    bool flag = false;
    //    if (pickupable != null)
    //    {
    //        TechType techType = pickupable.GetTechType();
    //        flag = CanBeStored(controller.DumpContainer.GetCount() + 1, techType);
    //    }

    //    if (!flag && verbose)
    //    {
    //        QuickLogger.ModMessage(AlterraGenBuildable.StorageFullMessage());
    //    }
    //    return flag;
    //}


    //public bool IsAllowedToAdd(TechType techType, bool verbose = false)
    //{
    //    bool flag = false;
    //    if (techType != TechType.None)
    //    {
    //        flag = CanBeStored(controller.DumpContainer.GetCount() + 1, techType);
    //    }

    //    if (!flag && verbose)
    //    {
    //        QuickLogger.ModMessage(AlterraGenBuildable.StorageFullMessage());
    //    }
    //    return flag;
    //}


    //public bool ContainsItem(TechType techType)
    //{
    //    return _container.Contains(techType);
    //}

    //public ItemsContainer ItemsContainer { get; set; }
    //public int StorageCount()
    //{
    //    return _container.Count;
    //}

    #endregion

    #region FCSPowerManager

    internal string GetTotalPowerString()
    {
        return _powerSource == null ? "0 kW" : $"{Mathf.RoundToInt(_powerSource.power)} kW";
    }

    internal float GetStoredPower()
    {
        return _storedPower;
    }

    internal PowerSource GetBatteryData()
    {
        return _powerSource == null ? null : _powerSource;
    }

    internal float GetToConsume()
    {
        return _toConsume;
    }

    internal void LoadFromSave(AlterraGenDataEntry saveData)
    {
        _storedPower = saveData.StoredPower;
        _toConsume = saveData.ToConsume;
        PowerState = saveData.PowerState;
        _powerSource?.SetPower(saveData.Power);
        OnContainerUpdate?.Invoke(storageContainer.container.Count(), maxSlots);
    }

    internal float GetPowerSourcePower()
    {
        return _powerSource.power;
    }

    public override float GetDevicePowerCharge()
    {
        return _powerSource?.power ?? 0f;
    }

    public override float GetDevicePowerCapacity()
    {
        return _powerSource?.maxPower ?? 0f;
    }

    public override FCSPowerStates GetPowerState()
    {
        return PowerState;
    }

    public override void TogglePowerState()
    {
        PowerState = PowerState == FCSPowerStates.Powered ? FCSPowerStates.Tripped : FCSPowerStates.Powered;

        if (PowerState == FCSPowerStates.Tripped)
        {
            _storedPower = _powerSource.power;
            _powerSource.SetPower(0f);
        }
        else
        {
            _powerSource.SetPower(_storedPower);
            _storedPower = 0f;
        }
    }

    public override void SetPowerState(FCSPowerStates state)
    {
        PowerState = state;
    }

    public override bool IsDevicePowerFull()
    {
        if (_powerSource == null) return true;
        return _powerSource.power >= _powerSource.maxPower;
    }

    public override bool ModifyPower(float amount, out float consumed)
    {
        var result = _powerSource.ModifyPower(amount, out var consumedO);
        consumed = consumedO;
        return result;
    }

    public override float GetPowerUsagePerSecond() => powerPerSecond;

    #endregion
}