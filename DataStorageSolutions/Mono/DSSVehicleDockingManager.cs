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
        private Coroutine _startCheck;
        private Coroutine _tryExtractItems;

        internal List<VehicleDockingBay> DockingBays { get; set; } = new List<VehicleDockingBay>();
        internal List<Vehicle> Vehicles { get; }= new List<Vehicle>();

        private IEnumerator StartCheck()
        {
            while (!_isBeingDestroyed)
            {
                if (QPatch.Configuration.Config.PullFromDockedVehicles)
                {
                    GetDockingBays();
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

            Dictionary<string, int> extractionResults = new Dictionary<string, int>();

            List<Vehicle> localVehicles = _newlyDockedVehicle?.ToList(); //Vehicles.ToList();

            if (localVehicles == null) yield break;

            foreach (var vehicle in localVehicles)
            {
                var vehicleName = vehicle.GetName();
                extractionResults[vehicleName] = 0;
                var vehicleContainers = DSSHelpers.GetVehicleContainers(vehicle);
                bool couldNotAdd = false;
                foreach (var vehicleContainer in vehicleContainers)
                {
                    foreach (var item in vehicleContainer.ToList())
                    {
                        if (!QPatch.Configuration.Config.PullFromDockedVehicles)
                        {
                            break;
                        }

                        if (ValidateCheck(item)) continue;

                        if (_manager.CanBeStored(1, item.item.GetTechType()))
                        {
                            var success = _manager.AddItemToContainer(item);
                            if (success)
                            {
                                extractionResults[vehicleName]++;
                                if (_extractingItems == false)
                                {
                                    ErrorMessage.AddDebug("Extracting items from vehicle storage...");
                                }
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

            _extractingItems = false;
            _newlyDockedVehicle = null;
        }

        private bool ValidateCheck(InventoryItem item)
        {
            if (Mod.IsFilterAddedWithType(item.item.GetTechType()))
            {
                QuickLogger.Info(string.Format(AuxPatchers.BlackListFormat(), Language.main.Get(item.item.GetTechType())),
                    true);
                return true;
            }

            if (!_manager.IsAllowedToAdd(item.item, true))
            {
                return true;
            }

            return false;
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

            _newlyDockedVehicle = Vehicles.Except(_vehiclesSnapshot);

            _tryExtractItems = StartCoroutine(TryExtractItems());

            var list3 = _vehiclesSnapshot.Except(Vehicles);

            // Un-subscribe all undocked vehicles
            foreach (Vehicle vehicle in list3)
            {
                var vehicleContainers = DSSHelpers.GetVehicleContainers(vehicle);
                UpdateSubscription(vehicleContainers, false,vehicle);
            }
            
            _manager.OnVehicleUpdate?.Invoke(Vehicles,_manager);
        }

        private void UpdateDockedVehicles()
        {
            _vehiclesSnapshot = new List<Vehicle>(Vehicles);
            Vehicles.Clear();
            ItemsTracker.Clear();
            foreach (var dockingBay in DockingBays)
            {
                var vehicle = dockingBay.GetDockedVehicle();
                if (vehicle != null)
                {
                    vehicle.modules.onRemoveItem += ModulesOnRemoveItem;
                    var vehicleContainers = DSSHelpers.GetVehicleContainers(vehicle);
                    ItemsTracker.Add(vehicle, GetItems(vehicleContainers));
                    _trackedContainers.AddRange(vehicleContainers);
                    Vehicles.Add(vehicle);
                    UpdateSubscription(vehicleContainers, true,vehicle);
                }
            }

            if (_prevContainerAmount != _trackedContainers.Count)
            {
                _manager.OnContainerUpdate?.Invoke(_manager);
            }

            _prevContainerAmount = _trackedContainers.Count;
            _trackedContainers.Clear();
        }

        public Dictionary<Vehicle, IEnumerable<TechType>> ItemsTracker { get; set; } = new Dictionary<Vehicle, IEnumerable<TechType>>();

        private IEnumerable<TechType> GetItems(List<ItemsContainer> vehicleContainers)
        {
            foreach (ItemsContainer container in vehicleContainers)
            {
                foreach (InventoryItem inventoryItem in container)
                {
                    yield return inventoryItem.item.GetTechType();
                }
            }
        }

        private void ModulesOnRemoveItem(InventoryItem item)
        {
            _manager.OnContainerUpdate?.Invoke(_manager);
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
            _manager.OnVehicleStorageUpdate?.Invoke(_manager);
        }

        private void ContainerOnOnAddItem(InventoryItem item)
        {
            _manager.OnVehicleStorageUpdate?.Invoke(_manager);
        }

        internal void Initialize(SubRoot mono, BaseManager manager)
        {
            _manager = manager;
            GetDockingBays();
           _startCheck = StartCoroutine(StartCheck());
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

        internal ItemsContainer AddItem(Vehicle vehicle, InventoryItem item)
        {
            var containers = DSSHelpers.GetVehicleContainers(vehicle);
            foreach (ItemsContainer container in containers)
            {
                if (container.HasRoomFor(item.width, item.height))
                {
                    return container;
                }
            }

            return null;
        }

        internal bool IsAllowedToAdd(Vehicle vehicle, Pickupable pickupable)
        {
            var containers = DSSHelpers.GetVehicleContainers(vehicle);
            foreach (ItemsContainer container in containers)
            {
                if (container.HasRoomFor(pickupable))
                {
                    return true;
                }
            }

            return false;
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

            if (_startCheck != null)
            {
                StopCoroutine(_startCheck);
            }

            if (_tryExtractItems != null)
            {
                StopCoroutine(_tryExtractItems);
            }
        }

        public IEnumerable<TechType> GetVehicleItems(Vehicle currentVehicle)
        {
            foreach (KeyValuePair<Vehicle, IEnumerable<TechType>> valuePair in ItemsTracker)
            {
                if (valuePair.Key != null)
                {
                    return valuePair.Value;
                }
            }

            return null;
        }
    }
}
