using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Helpers;
using DataStorageSolutions.Model;
using FCSCommon.Utilities;
using FMOD;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace DataStorageSolutions.Mono
{
    internal class DSSVehicleDockingManager : MonoBehaviour
    {
        private bool _extractingItems;

        private bool _isToggled;
        private readonly bool _isVDSInstalled = QPatch.IsDockedVehicleStorageAccessInstalled;
        private BaseManager _manager;
        private List<Vehicle> _vehiclesSnapshot;
        private Dictionary<int, int> Subscibers = new Dictionary<int, int>();
        private List<ItemsContainer> _trackedContainers = new List<ItemsContainer>();
        private int _prevContainerAmount;
        private IEnumerable<Vehicle> _newlyDockedVehicle;
        private bool _isBeingDestroyed;
        private Coroutine _tryExtractItems;

        internal List<VehicleDockingBay> DockingBays { get; set; } = new List<VehicleDockingBay>();
        internal List<Vehicle> Vehicles { get; }= new List<Vehicle>();

        private IEnumerator TryExtractItems()
        {
            List<Vehicle> localVehicles = _newlyDockedVehicle?.ToList();

            if (_extractingItems || !_isToggled || !QPatch.Configuration.Config.PullFromDockedVehicles || localVehicles == null)
            {
                yield break;
            }

            QuickLogger.Debug($"Local Vehicles: {localVehicles.Count}", true);

            foreach (var vehicle in localVehicles)
            {
                var vehicleName = vehicle.GetName();
                var vehicleContainers = DSSHelpers.GetVehicleContainers(vehicle);

#if DEBUG
                QuickLogger.Debug($"Vehicle {vehicleName} has {vehicleContainers.Count} containers",true);
#endif

                foreach (var vehicleContainer in vehicleContainers)
                {
                    foreach (var item in vehicleContainer.ToList())
                    {
                        var validCheck = ValidateCheck(item);
                        QuickLogger.Debug($"Valid Check Item: {validCheck}", true);
                        if (!validCheck) continue;

                        if (_manager.StorageManager.CanBeStored(1, item.item.GetTechType()))
                        {
                            vehicleContainer.RemoveItem(item.item);
                            var success = _manager.StorageManager.AddItemToContainer(item);

                            QuickLogger.Debug($"Attempt To Add To base result: {success}", true);
                            if (success)
                            {
                                if (_extractingItems == false)
                                {
                                    QuickLogger.ModMessage($"Extracting items from {vehicle.GetName()} storage...");
                                }
                                _extractingItems = true;
                                yield return new WaitForSeconds(QPatch.Configuration.Config.ExtractInterval);
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (!QPatch.Configuration.Config.PullFromDockedVehicles)
                    {
                        break;
                    }
                }
            }

            _extractingItems = false;
            _newlyDockedVehicle = null;
        }

        private bool ValidateCheck(InventoryItem item)
        {
            if (!Mod.IsFilterAddedWithType(item.item.GetTechType()))
                return _manager.StorageManager.IsAllowedToAdd(item.item, true);
            QuickLogger.ModMessage( string.Format(AuxPatchers.BlackListFormat(), Language.main.Get(item.item.GetTechType())));
            return false;
        }
        
        private void OnDockedVehicleChanged()
        {
            UpdateDockedVehicles();

            _newlyDockedVehicle = Vehicles.Except(_vehiclesSnapshot);

            _tryExtractItems = StartCoroutine(TryExtractItems());

            var list3 = _vehiclesSnapshot.Except(Vehicles);

            // Un-subscribe all un-docked vehicles
            foreach (Vehicle vehicle in list3)
            {
                var vehicleContainers = DSSHelpers.GetVehicleContainers(vehicle);
                UpdateSubscription(vehicleContainers, false,vehicle);
            }

            BaseManager.SendNotification();
        }

        private void UpdateDockedVehicles()
        {
            _vehiclesSnapshot = new List<Vehicle>(Vehicles);
            Vehicles.Clear();
            foreach (var dockingBay in DockingBays)
            {
                var vehicle = dockingBay.GetDockedVehicle();
                if (vehicle != null)
                {
                    vehicle.modules.onRemoveItem += ModulesOnRemoveItem;
                    var vehicleContainers = DSSHelpers.GetVehicleContainers(vehicle);
                    _trackedContainers.AddRange(vehicleContainers);
                    Vehicles.Add(vehicle);
                    UpdateSubscription(vehicleContainers, true,vehicle);
                }
            }

            if (_prevContainerAmount != _trackedContainers.Count)
            {
                BaseManager.SendNotification(true);
            }

            _prevContainerAmount = _trackedContainers.Count;
            _trackedContainers.Clear();
        }
        
        private void ModulesOnRemoveItem(InventoryItem item)
        {
            BaseManager.SendNotification();
        }

        private void UpdateSubscription(List<ItemsContainer> vehicleContainers, bool subscribing,Vehicle v)
        {
            foreach (ItemsContainer container in vehicleContainers)
            {

                if (container?.tr == null) continue;

                if (subscribing)
                {
                    if (!Subscibers.ContainsKey(container.tr.GetInstanceID()))
                    {
                        QuickLogger.Debug($"Subscribing vehicle {v.GetName()} {container.tr.GetInstanceID()}",true);
                        container.onAddItem += ContainerOnOnAddItem;
                        container.onRemoveItem += ContainerOnOnRemoveItem;
                        Subscibers.Add(container.tr.GetInstanceID(), v.GetInstanceID());
                    }
                }
                else
                {
                    QuickLogger.Debug($"Un-Subscribing vehicle {v.GetName()} {container.tr.GetInstanceID()}",true);
                    container.onAddItem -= ContainerOnOnAddItem;
                    container.onRemoveItem -= ContainerOnOnRemoveItem;
                    Subscibers.Clear();
                }
            }
        }
        
        private void ContainerOnOnRemoveItem(InventoryItem item)
        {
            BaseManager.SendNotification();
        }

        private void ContainerOnOnAddItem(InventoryItem item)
        {
            BaseManager.SendNotification();
        }

        internal void Initialize(BaseManager manager)
        {
            _manager = manager;
        }

        internal void ToggleIsEnabled(bool value)
        {

            if (_isVDSInstalled)
            {
                _isToggled = false;
                return;
            }

            _isToggled = value;

            QuickLogger.Debug($"Toggle State: {_isToggled}",true);
        }

        internal bool GetToggleState()
        {
            return _isToggled;
        }

        internal bool HasVehicles(bool verbose = false)
        {
            var value = DockingBays.Count > 0 && Vehicles.Count > 0;

            if (!value && verbose)
            {
                QuickLogger.Message(AuxPatchers.NoVehiclesDocked(), true);
            }

            return value;
        }
        
        internal void OpenContainer(Vehicle vehicle,ItemsContainer container)
        {
            var containers = DSSHelpers.GetVehicleContainers(vehicle);
            foreach (ItemsContainer currentContainer in containers)
            {
                if(container != currentContainer) continue;

                Player main = Player.main;
                PDA pda = main.GetPDA();
                Inventory.main.SetUsedStorage(container, false);
                pda.Open(PDATab.Inventory, null, null, 4f);
                break;
            }
        }

        private void OnDestroy()
        {
            _isBeingDestroyed = true;

            if (_tryExtractItems != null)
            {
                StopCoroutine(_tryExtractItems);
            }
        }

        internal bool IsVehicleDocked(Vehicle vehicle)
        {
            return Vehicles?.Contains(vehicle) ?? false;
        }

        internal void RegisterDockingBay(VehicleDockingBay dockingBay)
        {
            DockingBays.Add(dockingBay);
            dockingBay.onDockedChanged += OnDockedVehicleChanged;
        }

        internal void UnRegisterDockingBay(VehicleDockingBay dockingBay)
        {
            DockingBays.Remove(dockingBay);
            dockingBay.onDockedChanged -= OnDockedVehicleChanged;
        }
    }
}
