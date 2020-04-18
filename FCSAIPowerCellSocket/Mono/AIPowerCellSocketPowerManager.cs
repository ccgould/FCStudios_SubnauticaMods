using FCSAIPowerCellSocket.Buildables;
using FCSAIPowerCellSocket.Model;
using FCSCommon.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FCSAIPowerCellSocket.Mono
{
    internal class AIPowerCellSocketPowerManager : MonoBehaviour, IPowerInterface
    {
        private PowerRelay _connectedRelay;
        private AIPowerCellSocketController _mono;
        private const int ContainerHeight = 2;
        private const int ContainerWidth = 2;
        private ChildObjectIdentifier _containerRoot;
        private ItemsContainer _batteryContainer;
        private float _charge;
        private float _capacity;
        private readonly List<PowercellData> _powercellTracker = new List<PowercellData>(4);
        
        private void OnDestroy()
        {
            if (_batteryContainer == null) return;
            _batteryContainer.isAllowedToAdd -= IsAllowedToAdd;
            _batteryContainer.onAddItem -= BatteryContainerOnOnAddItem;
            _batteryContainer.onRemoveItem -= BatteryContainerOnOnRemoveItem;
        }

        //InvokeRepeating
        private void UpdateSlots()
        {
            _mono.UpdateSlots();
        }

        private void BatteryContainerOnOnRemoveItem(InventoryItem item)
        {
            var battery = item.item;

            var id = battery.GetComponent<PrefabIdentifier>().Id;

            var powercellData = _powercellTracker.Single(x => x.PrefabID == id);

            _powercellTracker.Remove(powercellData);

            UpdateCharge();

            UpdateCapacity();

            //_mono.UpdateSlots();

            _mono.EmptySlot(_powercellTracker.Count + 1);
        }

        private void BatteryContainerOnOnAddItem(InventoryItem item)
        {
            var battery = item.item;
            var techType = battery.GetTechType();
            var id = battery.GetComponent<PrefabIdentifier>().Id;
            var charge = battery.GetComponent<Battery>().charge;
            var capacity = battery.GetComponent<Battery>().capacity;

            var data = new PowercellData();
            data.Initialize(battery);
            _powercellTracker.Add(data);

            UpdateCapacity();
            UpdateCharge();

            //_mono.UpdateSlots();

            UpdateDisplay();
            QuickLogger.Debug($"Added {techType}|{charge} with id {id} to the trackers", true);
        }

        private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            bool flag = false;

            var techType = pickupable.GetTechType();
#if SUBNAUTICA
            var equipType = CraftData.GetEquipmentType(techType);
#elif BELOWZERO
            var equipType = TechData.GetEquipmentType(techType);
#endif


            if (equipType == EquipmentType.PowerCellCharger)
            {
                flag = true;
            }
            else
            {
                QuickLogger.Message(AIPowerCellSocketBuildable.OnlyPowercellsAllowed(), true);
            }

            return flag;
        }

        private void UpdateCharge()
        {
            var amount = 0f;

            foreach (var powercellData in _powercellTracker)
            {
                var charge = powercellData.Battery.charge;
                amount += charge;
                UpdateDisplay();
            }

            _charge = amount;
        }

        private void UpdateDisplay()
        {
            foreach (var powercellData in _powercellTracker)
            {
                _mono.Display.UpdateVisuals(powercellData, _powercellTracker.IndexOf(powercellData) + 1);
            }
        }

        private void UpdateCapacity()
        {
            var amount = 0f;

            foreach (var powercellData in _powercellTracker)
            {
                amount += powercellData.Battery.capacity;
            }

            _capacity = amount;
        }

        private IEnumerator UpdatePowerRelay()
        {
            QuickLogger.Debug("In UpdatePowerRelay");

            var i = 1;

            while (_connectedRelay == null)
            {
                QuickLogger.Debug($"Checking For Relay... Attempt {i}");

                PowerRelay relay = PowerSource.FindRelay(this.transform);
                if (relay != null && relay != _connectedRelay)
                {
                    _connectedRelay = relay;
                    _connectedRelay.AddInboundPower(this);
                    QuickLogger.Debug("PowerRelay found");
                }
                else
                {
                    _connectedRelay = null;
                }

                i++;
                yield return new WaitForSeconds(0.5f);
            }
        }

        internal void Initialize(AIPowerCellSocketController mono)
        {
            _mono = mono;
            StartCoroutine(UpdatePowerRelay());
            InvokeRepeating("UpdateSlots", 1f, 0.5f);
            if (_containerRoot == null)
            {
                QuickLogger.Debug("Initializing Filter StorageRoot");
                var storageRoot = new GameObject("FilterStorageRoot");
                storageRoot.transform.SetParent(transform, false);
                _containerRoot = storageRoot.AddComponent<ChildObjectIdentifier>();
            }

            _batteryContainer = new ItemsContainer(ContainerWidth, ContainerHeight, _containerRoot.transform,
                AIPowerCellSocketBuildable.PowercellContainterLabel(), null);

            _batteryContainer.isAllowedToAdd += IsAllowedToAdd;
            _batteryContainer.onAddItem += BatteryContainerOnOnAddItem;
            _batteryContainer.onRemoveItem += BatteryContainerOnOnRemoveItem;
        }

        internal int GetPowercellCount()
        {
            return _powercellTracker.Count;
        }

        public float GetPower()
        {
            if (_charge < 1f)
            {
                _charge = 0.0f;
            }

            return _charge;
        }

        public float GetMaxPower()
        {
            return _capacity;
        }

        public bool ModifyPower(float amount, out float modified)
        {
            modified = 0f;

            bool result = false;

            foreach (PowercellData powercellData in _powercellTracker)
            {
                if (powercellData.Battery.charge >= Mathf.Abs(amount))
                {

                    QuickLogger.Debug($"{powercellData.Battery.charge} || {amount}");

                    var battery = powercellData.Battery;

                    if (amount >= 0f)
                    {
                        result = (amount <= battery.capacity - battery.charge);
                        modified = Mathf.Min(amount, battery.capacity - battery.charge);
                        battery.charge += modified;
                    }
                    else
                    {
                        result = (battery.charge >= -amount);
                        if (GameModeUtils.RequiresPower())
                        {
                            modified = -Mathf.Min(-amount, battery.charge);
                            battery.charge += modified;
                        }
                        else
                        {
                            modified = amount;
                        }
                    }
                    break;
                }
            }
            UpdateCharge();
            return result;
        }

        public bool HasInboundPower(IPowerInterface powerInterface)
        {
            return false;
        }

        public bool GetInboundHasSource(IPowerInterface powerInterface)
        {
            return false;
        }
        
        internal void OpenSlots()
        {
            QuickLogger.Debug($"Powercell Slots Open", true);

            //if (!isContstructed.Invoke())
            //    return;

            Player main = Player.main;
            PDA pda = main.GetPDA();
            Inventory.main.SetUsedStorage(_batteryContainer, false);
            pda.Open(PDATab.Inventory, null, null, 4f);
        }
        
        internal void LoadPowercellItems(IEnumerable<PowercellData> savedDataPowercellDatas)
        {
            foreach (PowercellData powercellData in savedDataPowercellDatas)
            {
                QuickLogger.Debug($"Adding entity {powercellData.TechType}");

                var prefab = GameObject.Instantiate(CraftData.GetPrefabForTechType(powercellData.TechType));
                prefab.gameObject.GetComponent<PrefabIdentifier>().Id = powercellData.PrefabID;

                var battery = prefab.gameObject.GetComponent<Battery>();
                battery._charge = powercellData.Charge;

#if SUBNAUTICA
                var item = new InventoryItem(prefab.gameObject.GetComponent<Pickupable>().Pickup(false));
#elif BELOWZERO
                Pickupable pickupable = prefab.gameObject.GetComponent<Pickupable>();
                pickupable.Pickup(false);
                var item = new InventoryItem(pickupable);
#endif

                _batteryContainer.UnsafeAdd(item);
                QuickLogger.Debug($"Load Item {item.item.name}");
            }
        }

        internal IEnumerable<PowercellData> GetSaveData()
        {
            foreach (PowercellData powercellData in _powercellTracker)
            {
                powercellData.SaveData();
                yield return powercellData;
            }
        }
    }
}
