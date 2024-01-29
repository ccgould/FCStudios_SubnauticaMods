using FCS_AlterraHub.Models.Mono;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.Base;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.C40Terminal.Mono;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.C40Terminal.uGUI;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.C40Terminal.Pages;
internal class DSSVehicleSectionPage : DSSPage
{
    [SerializeField] private Text _vehicleSectionName;
    [SerializeField] private DSSTerminalController controller;
    [SerializeField] private List<DSSVehicleItemButton> _vehicleItemButtons = new List<DSSVehicleItemButton>();

    private Vehicle _currentVehicle;

    public override void Awake()
    {
        base.Awake();
        InvokeRepeating(nameof(Refresh), 1, 1);
    }

    private void  Refresh()
    {
        RefreshVehicleName();
    }

    public void RefreshVehicleName()
    {
        if (_vehicleSectionName == null) return;
#if SUBNAUTICA
        _vehicleSectionName.text = _currentVehicle?.GetName();
#else
            _vehicleSectionName.text = _currentVehicle?.vehicleName;
#endif
    }

    public override void Enter(object arg = null)
    {
        base.Enter(arg);
        
        var veh = (Vehicle)arg;

        ShowVehicleContainers(veh);

        QuickLogger.Debug(veh.vehicleName);
    }

    public void ShowVehicleContainers(Vehicle vehicle)
    {
        if (vehicle == null)
        {
            if (GetManager().GetDockingManager().IsVehicleDocked(_currentVehicle))
            {
                vehicle = _currentVehicle;
            }
            else
            {
                //_vehiclesSection.SetActive(false);
                //_vehiclesSettingsSection.SetActive(false);
                for (int i = 0; i < 8; i++)
                {
                    _vehicleItemButtons[i].Reset();
                }

                return;
            }
        }

        var storage = GetManager().GetDockingManager().GetVehicleContainers(vehicle);
        //_vehiclesSection.SetActive(true);
        //_vehiclesSettingsSection.SetActive(false);

        for (int i = 0; i < 8; i++)
        {
            _vehicleItemButtons[i].Reset();
        }

        for (int i = 0; i < storage.Count; i++)
        {
            _vehicleItemButtons[i].Set(vehicle, storage[i], i);
        }

        _currentVehicle = vehicle;
    }

    private HabitatManager GetManager()
    {
        return controller.CachedHabitatManager;
    }
}
