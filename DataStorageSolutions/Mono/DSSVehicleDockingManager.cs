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
    internal class DSSVehicleDockingManager
    {
        private bool _extractingItems;
        private SubRoot _mono;
        private bool _isToggled;
        private readonly bool _isVDSInstalled = QPatch.IsDockedVehicleStorageAccessInstalled;
        private BaseManager _manager;
        private int _prevAmount;
        private List<Vehicle> _vehiclesSnapshot;
        private Dictionary<int, int> Subscibers = new Dictionary<int, int>();
        private List<ItemsContainer> _trackedContainers = new List<ItemsContainer>();
        private int _prevContainerAmount;

        internal List<VehicleDockingBay> DockingBays { get; set; } = new List<VehicleDockingBay>();
        internal List<Vehicle> Vehicles { get; }= new List<Vehicle>();

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

                        if (!Mod.IsFilterAdded(item.item.GetTechType()) && _manager.IsAllowedToAdd(item.item,true) && _manager.CanBeStored(1, item.item.GetTechType()))
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
                var vehicleContainers = DSSHelpers.GetVehicleContainers(vehicle);
                UpdateSubscription(vehicleContainers, false,vehicle);
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
                    vehicle.modules.onRemoveItem += ModulesOnOnRemoveItem;
                    var vehicleContainers = DSSHelpers.GetVehicleContainers(vehicle);
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

        private void ModulesOnOnRemoveItem(InventoryItem item)
        {
            _manager.OnContainerUpdate?.Invoke(_manager);
        }

        private void UpdateSubscription(List<ItemsContainer> vehicleContainers, bool subscribing,Vehicle v)
        {
            foreach (ItemsContainer container in vehicleContainers)
            {
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
            _mono = mono;
            _manager = manager;
            GetDockingBays();
            Player.main.StartCoroutine(StartCheck());
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

        public bool IsAllowedToAdd(Vehicle vehicle, Pickupable pickupable)
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

        public void OpenContainer(Vehicle vehicle,ItemsContainer container)
        {
            var containers = DSSHelpers.GetVehicleContainers(vehicle);
            foreach (ItemsContainer currentContainer in containers)
            {
                if(container != currentContainer) continue;

                if (!currentContainer.IsFull())
                {
                    Player main = Player.main;
                    PDA pda = main.GetPDA();
                    Inventory.main.SetUsedStorage(container, false);
                    pda.Open(PDATab.Inventory, null, null, 4f);
                }
                break;
            }
        }
    }
}
