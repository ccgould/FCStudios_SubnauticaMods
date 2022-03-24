using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mono
{
    public class DSSVehicleDockingManager : MonoBehaviour
    {
        private bool _extractingItems;
        private readonly bool _isVDSInstalled = QPatch.IsDockedVehicleStorageAccessInstalled;
        private BaseManager _manager;
        private List<Vehicle> _vehiclesSnapshot;
        private Dictionary<int, int> Subscibers = new Dictionary<int, int>();
        private List<ItemsContainer> _trackedContainers = new List<ItemsContainer>();
        private int _prevContainerAmount;
        private IEnumerable<Vehicle> _newlyDockedVehicle;
        private bool _isBeingDestroyed;
        private Coroutine _tryExtractItems;
        private const float ExtractInterval = 0.25f;

        public List<VehicleDockingBay> DockingBays { get; set; } = new List<VehicleDockingBay>();
        public List<Vehicle> Vehicles { get; } = new List<Vehicle>();

        private IEnumerator TryExtractItems()
        {

            QuickLogger.Debug("TryExtractItems",true);

            List<Vehicle> localVehicles = _newlyDockedVehicle?.ToList();
            QuickLogger.Debug($"Local Vehicles Count: {localVehicles?.Count}", true);
            QuickLogger.Debug($"Extracting Items: {_extractingItems} || Pull From Docked Vehicles: {_manager.PullFromDockedVehicles} || Local Vehicles: {localVehicles}", true);
            if (_extractingItems || !_manager.PullFromDockedVehicles || localVehicles == null)
            {
                yield break;
            }

            QuickLogger.Debug($"Local Vehicles: {localVehicles.Count}", true);

            foreach (var vehicle in localVehicles)
            {
#if SUBNAUTICA
                var vehicleName = vehicle.GetName();
#else
                var vehicleName = vehicle.vehicleName;
#endif
                var vehicleContainers = GetVehicleContainers(vehicle);

#if DEBUG
                QuickLogger.Debug($"Vehicle {vehicleName} has {vehicleContainers.Count} containers", true);
#endif

                foreach (var vehicleContainer in vehicleContainers)
                {
                    foreach (var item in vehicleContainer.ToList())
                    {
                        var validCheck = ValidateCheck(item);
                        QuickLogger.Debug($"Valid Check Item: {validCheck}", true);
                        if (!validCheck) continue;

                        if (_manager.IsAllowedToAdd(item.item.GetTechType(),false))
                        {
                            vehicleContainer.RemoveItem(item.item);
                            var success = _manager.AddItemToContainer(item);

                            QuickLogger.Debug($"Attempt To Add To base result: {success}", true);
                            if (success)
                            {
                                if (_extractingItems == false)
                                {
                                    QuickLogger.ModMessage($"Extracting items from {vehicleName} storage...");
                                }
                                _extractingItems = true;
                                yield return new WaitForSeconds(ExtractInterval);
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

                    if (!_manager.PullFromDockedVehicles)
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
            if (!_manager.IsDockingFilterAddedWithType(item.item.GetTechType()))
                return _manager.IsAllowedToAdd(item.item.GetTechType(), true);
            QuickLogger.ModMessage(Buildables.AlterraHub.BlackListFormat(Language.main.Get(item.item.GetTechType())));
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
                var vehicleContainers = GetVehicleContainers(vehicle);
                UpdateSubscription(vehicleContainers, false, vehicle);
            }

            BaseManager.GlobalNotifyByID(null, "VehicleUpdate");
        }

        private void UpdateDockedVehicles()
        {
            _vehiclesSnapshot = new List<Vehicle>(Vehicles);
            Vehicles.Clear();
            foreach (var dockingBay in DockingBays)
            {
#if SUBNAUTICA
                var vehicle = dockingBay.GetDockedVehicle();
#else
                var vehicle = dockingBay._dockedObject.vehicle;
#endif
                if (vehicle != null)
                {
                    vehicle.modules.onRemoveItem += ModulesOnRemoveItem;
                    vehicle.modules.onAddItem += ModulesOnAddItem;
                    var vehicleContainers = GetVehicleContainers(vehicle);
                    _trackedContainers.AddRange(vehicleContainers);
                    Vehicles.Add(vehicle);
                    UpdateSubscription(vehicleContainers, true, vehicle);
                }
            }

            if (_prevContainerAmount != _trackedContainers.Count)
            {
                //BaseManager.SendNotification(true);
            }

            _prevContainerAmount = _trackedContainers.Count;
            _trackedContainers.Clear();
        }
            
        private void ModulesOnAddItem(InventoryItem item)
        {
            BaseManager.GlobalNotifyByID(null, "VehicleModuleAdded");
        }

        private void ModulesOnRemoveItem(InventoryItem item)
        {
            BaseManager.GlobalNotifyByID(null, "VehicleModuleRemoved");
        }

        private void UpdateSubscription(List<ItemsContainer> vehicleContainers, bool subscribing, Vehicle v)
        {
            foreach (ItemsContainer container in vehicleContainers)
            {

                if (container?.tr == null) continue;

#if SUBNAUTICA
                        var vehicleName = v.GetName();
#else
                var vehicleName = v.vehicleName;
#endif

                if (subscribing)
                {
                    if (!Subscibers.ContainsKey(container.tr.GetInstanceID()))
                    {
                        QuickLogger.Debug($"Subscribing vehicle {vehicleName} {container.tr.GetInstanceID()}", true);
                        container.onAddItem += ContainerOnOnAddItem;
                        container.onRemoveItem += ContainerOnOnRemoveItem;
                        Subscibers.Add(container.tr.GetInstanceID(), v.GetInstanceID());
                    }
                }
                else
                {
                    QuickLogger.Debug($"Un-Subscribing vehicle {vehicleName} {container.tr.GetInstanceID()}", true);
                    container.onAddItem -= ContainerOnOnAddItem;
                    container.onRemoveItem -= ContainerOnOnRemoveItem;
                    Subscibers.Clear();
                }
            }
        }

        private void ContainerOnOnRemoveItem(InventoryItem item)
        {
            //BaseManager.SendNotification();
        }

        private void ContainerOnOnAddItem(InventoryItem item)
        {
            //BaseManager.SendNotification();
        }

        public void Initialize(BaseManager manager)
        {
            _manager = manager;
        }
        
        public bool HasVehicles(bool verbose = false)
        {
            var value = DockingBays.Count > 0 && Vehicles.Count > 0;

            if (!value && verbose)
            {
                QuickLogger.Message(Buildables.AlterraHub.NoVehiclesDocked(), true);
            }

            return value;
        }

        public void OpenContainer(Vehicle vehicle, ItemsContainer container)
        {
            var containers = GetVehicleContainers(vehicle);
            foreach (ItemsContainer currentContainer in containers)
            {
                if (container != currentContainer) continue;

                Player main = Player.main;
                PDA pda = main.GetPDA();
                Inventory.main.SetUsedStorage(container);
#if SUBNAUTICA
                pda.Open(PDATab.Inventory, null, null, 4f);
#else
                pda.Open(PDATab.Inventory);
#endif
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

        public bool IsVehicleDocked(Vehicle vehicle)
        {
            return Vehicles?.Contains(vehicle) ?? false;
        }

        public void RegisterDockingBay(VehicleDockingBay dockingBay)
        {
            DockingBays.Add(dockingBay);
            dockingBay.onDockedChanged += OnDockedVehicleChanged;
        }

        public void UnRegisterDockingBay(VehicleDockingBay dockingBay)
        {
            DockingBays.Remove(dockingBay);
            dockingBay.onDockedChanged -= OnDockedVehicleChanged;
        }

        public static List<ItemsContainer> GetVehicleContainers(Vehicle vehicle)
        {
            var vehicleContainers = vehicle?.gameObject.GetComponentsInChildren<StorageContainer>().Select((x) => x?.container).ToList();
            vehicleContainers?.AddRange(GetSeamothStorage(vehicle));
            return vehicleContainers;
        }

        public static IEnumerable<ItemsContainer> GetSeamothStorage(Vehicle seamoth)
        {
            if (seamoth is SeaMoth && seamoth.modules != null)
            {
                using (var e = seamoth.modules.GetEquipment())
                {
                    while (e.MoveNext())
                    {
                        var module = e.Current.Value;
                        if (module == null || module.item == null)
                        {
                            continue;
                        }

                        var container = module.item.GetComponent<SeamothStorageContainer>();
                        if (container != null && !container.gameObject.name.Contains("Torpedo"))
                        {
                            yield return container.container;
                        }
                    }
                }
            }
        }
    }
}
