using System;
using System.Collections.Generic;
using FCS_AlterraHub.Mono;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal
{
    internal class MoonPoolDialog : MonoBehaviour
    {
        private MoonPoolPageController _mono;
        private Text _label;
        private GameObject _grid;
        private BaseManager _baseManager;
        private List<VehicleButton> _trackedVehicles = new List<VehicleButton>();
        private GridHelperV2 _vehicleGrid;

        internal void Initialize(BaseManager baseManager, MoonPoolPageController mono)
        {
            _grid = GameObjectHelpers.FindGameObject(gameObject, "Grid");

            foreach (Transform child in _grid.transform)
            {
                var vehicleButton = child.gameObject.EnsureComponent<VehicleButton>();
                vehicleButton.OnButtonClick += OnVehicleItemButtonClick;
                _trackedVehicles.Add(vehicleButton);
            }

            _baseManager = baseManager;
            
            _mono = mono;
            _label = gameObject.GetComponentInChildren<Text>();
            _vehicleGrid = _grid.EnsureComponent<GridHelperV2>();
            _vehicleGrid.OnLoadDisplay += OnLoadItemsGrid;
            _vehicleGrid.Setup(8, gameObject, Color.gray, Color.white, null);
            
            mono.GetController().IPCMessage += IpcMessage;
            
            _vehicleGrid.DrawPage();
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
                if(_trackedVehicles?.Count == 0) return;

                var grouped = _baseManager.DockingManager.Vehicles;
                
                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                QuickLogger.Debug($"// ====== Resetting Vehicle Grid {grouped.Count} || Tracked Vehicles {_trackedVehicles.Count} || Max Per Page{data.MaxPerPage}====== //");
                for (int i = data.EndPosition; i < data.MaxPerPage; i++)
                {
                    _trackedVehicles[i].Reset();
                    QuickLogger.Debug($"Reset index {i}");
                }
                QuickLogger.Debug("// ====== Resetting Vehicle Grid ====== //");

                QuickLogger.Debug("// ====== Setting Vehicle Grid ====== //");
                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    _trackedVehicles[i].Set(grouped[i].GetName(), grouped[i]);
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

        private void OnVehicleItemButtonClick(string arg1, object arg2)
        {
            _mono.ShowVehicleContainers((Vehicle) arg2);
        }
    }
}