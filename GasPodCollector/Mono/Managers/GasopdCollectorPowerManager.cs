using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FCSCommon.Enums;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using FCSTechFabricator.Enums;
using GasPodCollector.Buildables;
using GasPodCollector.Models;
using UnityEngine;


namespace GasPodCollector.Mono.Managers
{
    internal class GasopdCollectorPowerManager : MonoBehaviour
    {
        private GaspodCollectorController _mono;
        private Equipment _equipment;
        private float _powerUsage = QPatch.Configuration.Config.PowerUsage;
        private const string Slot1 = "BatteryCharger1";
        private const string Slot2 = "BatteryCharger2";
        private readonly Dictionary<int,BatteryInfo> _batteries = new Dictionary<int, BatteryInfo>
        {
            {0,null},
            {1,null}
        };

        public Action<FCSPowerStates> OnPowerChanged { get; set; }

        internal void Setup(GaspodCollectorController mono)
        {
            _mono = mono;
            
            var equipmentRoot = new GameObject("BEquipmentRoot");
            equipmentRoot.transform.SetParent(gameObject.transform, false);
            equipmentRoot.AddComponent<ChildObjectIdentifier>();
            equipmentRoot.SetActive(false);

            _equipment = new Equipment(gameObject, equipmentRoot.transform);
            _equipment.SetLabel(GaspodCollectorBuildable.EquipmentContainerLabel());

            Type equipmentType = typeof(Equipment);
            EventInfo onEquipInfo = equipmentType.GetEvent("onEquip", BindingFlags.Public | BindingFlags.Instance);
            EventInfo onUnequipInfo = equipmentType.GetEvent("onUnequip", BindingFlags.Public | BindingFlags.Instance);

            Type powerManagerType = typeof(GasopdCollectorPowerManager);
            MethodInfo myOnEquipMethod = powerManagerType.GetMethod("OnEquip", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo myOnUnequipMethod = powerManagerType.GetMethod("OnUnequip", BindingFlags.Instance | BindingFlags.NonPublic);

            var onEquipDelegate = Delegate.CreateDelegate(typeof(Equipment.OnEquip), this, myOnEquipMethod);
            var onUnequipDelegate = Delegate.CreateDelegate(typeof(Equipment.OnUnequip), this, myOnUnequipMethod);

            onEquipInfo.AddEventHandler(_equipment, onEquipDelegate);
            onUnequipInfo.AddEventHandler(_equipment, onUnequipDelegate);

            _equipment.AddSlot(Slot1);
            _equipment.AddSlot(Slot2);


            if (_equipment == null)
            {
                QuickLogger.Error("Equipment is null on creation");
            }
        }

        private void OnUnequip(string slot, InventoryItem item)
        {
            switch (slot)
            {
                case Slot1:
                    _batteries[0] = null;
                    break;
                case Slot2:
                    _batteries[1] = null;
                    break;
            }

            _mono.UpdateBatteryDisplay(_batteries);
        }

        private void OnEquip(string slot, InventoryItem item)
        {
            var battery = item.item.gameObject.GetComponent<IBattery>();
            var newBattery = new BatteryInfo(item.item.GetTechType(), battery, slot);

            switch (slot)
            {
                case Slot1:
                    _batteries[0] = newBattery;
                    break;

                case Slot2:
                    _batteries[1] = newBattery;
                    break;
            }

            _mono.UpdateBatteryDisplay(_batteries);
        }

        internal void OpenEquipment()
        {
            PDA pda = Player.main.GetPDA();
            if (!pda.isInUse)
            {
                if (_equipment == null)
                {
                    QuickLogger.Debug("Equipment is null", true);
                }

                Inventory.main.SetUsedStorage(_equipment, false);
                pda.Open(PDATab.Inventory, gameObject.transform, null, 4f);
            }
        }

        internal void TakePower()
        {
            foreach (KeyValuePair<int, BatteryInfo> battery in _batteries)
            {
                if (battery.Value == null) continue;

                if (battery.Value.BatteryCharge >= _powerUsage)
                {
                    battery.Value.TakePower(_powerUsage);
                    break;
                }
            }

            _mono.UpdateBatteryDisplay(_batteries);
        }

        internal Dictionary<int, BatteryInfo> GetBatteries()
        {
            return _batteries;
        }

        internal bool HasPower()
        {
            float amount = 0f;

            if (_batteries[0] != null)
            {
                amount += _batteries[0].BatteryCharge;
            }

            if (_batteries[1] != null)
            {
                amount += _batteries[1].BatteryCharge;
            }

            return amount >= _powerUsage;
        }

        public void LoadSaveData(Dictionary<int, BatteryInfo> savedDataBatteries)
        {
            foreach (KeyValuePair<int,BatteryInfo> module in savedDataBatteries)
            {
                if (module.Value == null) continue;
                var attachment = module.Value.TechType.ToPickupable();
                attachment.GetComponent<IBattery>().charge = module.Value.BatteryCharge;
#if SUBNAUTICA
                _equipment.AddItem(module.Value.Slot, new InventoryItem(attachment.Pickup(false)));
#elif BELOWZERO
                attachment.Pickup(false);
                _equipment.AddItem(module.Value.Slot, new InventoryItem(attachment));
#endif
            }
        }
    }
}
