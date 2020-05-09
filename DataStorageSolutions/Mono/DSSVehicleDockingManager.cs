using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Helpers;
using DataStorageSolutions.Model;
using FCSCommon.Utilities;
using UnityEngine;

namespace DataStorageSolutions.Mono
{
    internal class DSSVehicleDockingManager
    {
        internal List<VehicleDockingBay> DockingBays { get; set; } = new List<VehicleDockingBay>();
        internal List<Vehicle> Vehicles { get; }= new List<Vehicle>();
        private bool _extractingItems;
        private SubRoot _mono;
        private bool _isToggled;
        private readonly bool _isVDSInstalled = QPatch.IsDockedVehicleStorageAccessInstalled;
        private BaseManager _manager;
        private int _prevAmount;
        private List<Vehicle> _vehiclesSnapshot;

        private IEnumerator StartCheck()
        {
            while (true)
            {
                if (QPatch.Configuration.Config.PullFromDockedVehicles)
                {
                    GetDockingBays();
                    yield return TryExtractItems();
                }
                yield return new WaitForSeconds(QPatch.Configuration.Config.CheckVehiclesInterval);
            }
        }

        private IEnumerator TryExtractItems()
        {
            if(!_isToggled) yield break;

            if (_extractingItems)
            {
                yield break;
            }
            if (!QPatch.Configuration.Config.PullFromDockedVehicles)
            {
                yield break;
            }

            bool extractedAnything = false;
            Dictionary<string, int> extractionResults = new Dictionary<string, int>();

            List<Vehicle> localVehicles = Vehicles.ToList();
            foreach (var vehicle in localVehicles)
            {
                var vehicleName = vehicle.GetName();
                extractionResults[vehicleName] = 0;
                var vehicleContainers = GetVehicleContainers(vehicle);
                bool couldNotAdd = false;
                foreach (var vehicleContainer in vehicleContainers)
                {
                    foreach (var item in vehicleContainer.ToList())
                    {
                        if (!QPatch.Configuration.Config.PullFromDockedVehicles)
                        {
                            break;
                        }

                        if (_manager.IsAllowedToAdd(item.item,true) && _manager.CanBeStored(1, item.item.GetTechType()))
                        {
                            var success = _manager.AddItemToContainer(item);
                            if (success)
                            {
                                extractionResults[vehicleName]++;
                                if (_extractingItems == false)
                                {
                                    ErrorMessage.AddDebug("Extracting items from vehicle storage...");
                                }
                                extractedAnything = true;
                                _extractingItems = true;
                                yield return new WaitForSeconds(QPatch.Configuration.Config.ExtractInterval);
                            }
                            else
                            {
                                couldNotAdd = true;
                                break;
                            }
                        }
                        else
                        {
                            couldNotAdd = true;
                            break;
                        }
                    }

                    if (couldNotAdd || !QPatch.Configuration.Config.PullFromDockedVehicles)
                    {
                        break;
                    }
                }
            }

            //if (extractedAnything)
            //{
            //    NotifyExtraction(extractionResults);
            //}
            _extractingItems = false;
        }

        private void GetDockingBays()
        {
            RemoveDockingBayListeners();
            DockingBays = _manager.Habitat.GetComponentsInChildren<VehicleDockingBay>().ToList();
            AddDockingBayListeners();
            UpdateDockedVehicles();
        }

        private void AddDockingBayListeners()
        {
            foreach (var dockingBay in DockingBays)
            {
                dockingBay.onDockedChanged += OnDockedVehicleChanged;
            }
        }

        private void RemoveDockingBayListeners()
        {
            foreach (var dockingBay in DockingBays)
            {
                dockingBay.onDockedChanged -= OnDockedVehicleChanged;
            }
        }

        private void OnDockedVehicleChanged()
        {
            UpdateDockedVehicles();
            _mono.StartCoroutine(TryExtractItems());

            var list3 = _vehiclesSnapshot.Except(Vehicles);
            foreach (Vehicle vehicle in list3)
            {
                var vehicleContainers = GetVehicleContainers(vehicle);
                UpdateSubscription(vehicleContainers, false);
            }
            _manager.OnVehicleUpdate?.Invoke(Vehicles,_manager);
        }

        private void UpdateDockedVehicles()
        {
            _prevAmount = DockingBays.Count;
            _vehiclesSnapshot = new List<Vehicle>(Vehicles);
            Vehicles.Clear();
            foreach (var dockingBay in DockingBays)
            {
                var vehicle = dockingBay.GetDockedVehicle();
                if (vehicle != null)
                {
                    var vehicleContainers = GetVehicleContainers(vehicle);
                    Vehicles.Add(vehicle);
                    UpdateSubscription(vehicleContainers, true);
                }
            }
        }

        private void UpdateSubscription(List<ItemsContainer> vehicleContainers, bool subscribing)
        {
            foreach (ItemsContainer container in vehicleContainers)
            {
                if (subscribing)
                {
                    container.onAddItem += ContainerOnOnAddItem;
                    container.onRemoveItem += ContainerOnOnRemoveItem;
                }
                else
                {
                    container.onAddItem -= ContainerOnOnAddItem;
                    container.onRemoveItem -= ContainerOnOnRemoveItem;
                }
            }
        }

        internal static List<ItemsContainer> GetVehicleContainers(Vehicle vehicle)
        {
            var vehicleContainers = vehicle.gameObject.GetComponentsInChildren<StorageContainer>().Select((x) => x.container)
                .ToList();
            vehicleContainers.AddRange(DSSHelpers.GetSeamothStorage(vehicle));
            return vehicleContainers;
        }

        private void ContainerOnOnRemoveItem(InventoryItem item)
        {
            _manager.OnVehicleStorageUpdate?.Invoke();
        }

        private void ContainerOnOnAddItem(InventoryItem item)
        {
            _manager.OnVehicleStorageUpdate?.Invoke();
        }

        internal void Initialize(SubRoot mono, BaseManager manager)
        {
            _mono = mono;
            _manager = manager;
            GetDockingBays();
            Player.main.StartCoroutine(StartCheck());
        }

        internal void ToggleIsEnabled()
        {
            if (_isVDSInstalled)
            {
                _isToggled = false;
                return;
            }

            _isToggled = !_isToggled;

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
    }
}
