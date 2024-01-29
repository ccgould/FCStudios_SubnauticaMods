using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Mono;
using FCSCommon.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace FCS_EnergySolutions.ModItems.Buildables.PowerStorage.Mono.Base;
internal class BasePowerStorage : MonoBehaviour
{
    private readonly HashSet<PowerStorageController> _registeredDevices = new HashSet<PowerStorageController>();
    private bool _isCharging;
    private float _baseCapacity;
    private float _basePower;
    private bool _showInLog;

    internal void SetHabitatManager(HabitatManager manager)
    {
        this.manager = manager;
    }

    private HabitatManager manager;

    private float CalculatePowerPercentage()
    {
        //Get Capactity
        _baseCapacity = CalculateBasePowerCapacity();
        _basePower = CalculateBasePower();

        if (_basePower <= 0 || _baseCapacity <= 0) return 0;

        return (_basePower / _baseCapacity) * 100;
    }

    private float CalculateBasePowerCapacity()
    {
        if (HabitatService.main.GetHabitatManager(Player.main.currentSub) == manager && _showInLog)
        {
            QuickLogger.Debug($"CalculateBasePowerCapacity:", true);
            QuickLogger.Debug($"Base Capacity {manager.GetBasePowerCapacity()}", true);
            QuickLogger.Debug($"Power Storage Capacity {TotalPowerStorageCapacityAtBase()}", true);
            QuickLogger.Debug($"Capacity Result {manager.GetBasePowerCapacity() - TotalPowerStorageCapacityAtBase()}", true);
        }
        return manager.GetBasePowerCapacity() - TotalPowerStorageCapacityAtBase();
    }

    private float TotalPowerStorageCapacityAtBase()
    {
        float total = 0f;
        foreach (PowerStorageController controller in _registeredDevices)
        {
            total += controller.GetMaxPower();
        }

        return total;
    }

    public float CalculateBasePower()
    {
        if (HabitatService.main.GetHabitatManager(Player.main.currentSub) == manager && _showInLog)
        {
            QuickLogger.Debug($"CalculateBasePower:", true);
            QuickLogger.Debug($"Base Power {manager.GetPower()}", true);
            QuickLogger.Debug($"Power Storage Power {TotalPowerStoragePowerAtBase()}", true);
            QuickLogger.Debug($"Power Result {manager.GetPower() - TotalPowerStoragePowerAtBase()}", true);
        }
        return manager.GetPower() - TotalPowerStoragePowerAtBase();
    }

    private float TotalPowerStoragePowerAtBase()
    {
        float total = 0f;

        foreach (PowerStorageController controller in _registeredDevices)
        {
            total += controller.GetStoredPower();
        }

        return total;
    }

    private void Update()
    {
        foreach (PowerStorageController controller in _registeredDevices)
        {
            if (CalculatePowerPercentage() > 40)
            {
                controller.Charge();
                _isCharging = true;
            }
            else
            {
                controller.Discharge();
                _isCharging = false;
            }
        }
    }

    public void Register(PowerStorageController controller)
    {
        if (!_registeredDevices.Contains(controller))
        {
            _registeredDevices.Add(controller);
        }
    }

    public void Unregister(PowerStorageController controller)
    {
        _registeredDevices.Remove(controller);
    }
}

