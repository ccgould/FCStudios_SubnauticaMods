using FCS_AlterraHub.Core.Components;
using FCSCommon.Utilities;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.C40Terminal.Pages;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.C40Terminal.Pages.uGUI;

namespace FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.C40Terminal.Mono;
internal class DSSVehicleListDialog : MonoBehaviour
{
    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeSelf);

        if(gameObject.activeSelf )
        {
            _vehicleGrid.DrawPage();
        }
    }

    [SerializeField] private DSSMoonPoolPage _mono;
    [SerializeField] private Text _label;
    [SerializeField] private GridHelper _vehicleGrid;
    [SerializeField] private List<uGUI_VehicleButton> uGUIVehicleButtons;
    private DSSTerminalController _controller;

    private void Awake()
    {
        _vehicleGrid.OnLoadDisplay += OnLoadItemsGrid;
        _mono.IPCMessage += IpcMessage;
        _controller = gameObject.GetComponentInParent<DSSTerminalController>();
    }

    private void IpcMessage(string message)
    {
        if (message.Equals("VehicleUpdate"))
        {
            _vehicleGrid.DrawPage();
        }

        if (message.Equals("VehicleModuleAdded") ||
            message.Equals("VehicleModuleRemoved") ||
            message.Equals("VehicleUpdate"))
        {
            _mono.ShowVehicleContainers(null);
        }
    }

    private void OnLoadItemsGrid(DisplayData data)
    {
        try
        {
            if (uGUIVehicleButtons?.Count == 0) return;

            var grouped = _controller.CachedHabitatManager.GetDockingManager().Vehicles;

            if (data.EndPosition > grouped.Count)
            {
                data.EndPosition = grouped.Count;
            }

            QuickLogger.Debug($"// ====== Resetting Vehicle Grid {grouped.Count} || Tracked Vehicles {uGUIVehicleButtons.Count} || Max Per Page{data.MaxPerPage}====== //");
            for (int i = data.EndPosition; i < data.MaxPerPage; i++)
            {
                uGUIVehicleButtons[i].Reset();
                QuickLogger.Debug($"Reset index {i}");
            }
            QuickLogger.Debug("// ====== Resetting Vehicle Grid ====== //");

            QuickLogger.Debug("// ====== Setting Vehicle Grid ====== //");
            for (int i = data.StartPosition; i < data.EndPosition; i++)
            {
#if SUBNAUTICA
                uGUIVehicleButtons[i].Set(grouped[i].GetName(), grouped[i]);
#else
                    _trackedVehicles[i].Set(grouped[i].vehicleName, grouped[i]);
#endif
                QuickLogger.Debug($"Set index {i}");
            }
            QuickLogger.Debug("// ====== Setting Vehicle Grid ====== //");

        }
        catch (Exception e)
        {
            QuickLogger.Error("Error Caught");
            QuickLogger.Error($"Error Message: {e.Message}");
            QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
        }
    }

    public void OnVehicleItemButtonClick(Vehicle arg2)
    {
        _mono.ShowVehicleContainers(arg2);
    }
}