using FCS_DeepDriller.Buildable;
using FCSCommon.Utilities;
using System;
using UnityEngine;

namespace FCS_DeepDriller.Mono.Handlers
{
    internal class FCSDeepDrillerModuleContainer
    {
        private FCSDeepDrillerController _mono;
        private Func<bool> _isConstructed;
        private Equipment _equipment;
        public ChildObjectIdentifier equipmentRoot;


        internal static readonly string[] SlotIDs = new string[2]
        {
            "ExosuitModule1",
            "ExosuitModule2",
        };


        internal void Setup(FCSDeepDrillerController mono)
        {
            _mono = mono;

            _isConstructed = () => mono.IsConstructed;

            var equipmentRoot = new GameObject("EquipmentRoot");
            equipmentRoot.transform.SetParent(_mono.transform, false);
            equipmentRoot.AddComponent<ChildObjectIdentifier>();
            equipmentRoot.SetActive(false);

            _equipment = new Equipment(mono.gameObject, equipmentRoot.transform);
            _equipment.SetLabel(FCSDeepDrillerBuildable.EquipmentContainerLabel());
            _equipment.isAllowedToAdd = IsAllowedToAdd;
            _equipment.onEquip += OnEquipmentAdded;
            _equipment.onUnequip += OnEquipmentRemmoved;
            _equipment.AddSlot(SlotIDs[0]);
            _equipment.AddSlot(SlotIDs[1]);
        }

        private void OnEquipmentRemmoved(string slot, InventoryItem item)
        {
            //switch (item.item.GetTechType())
            //{
            //    case "AIDeepDrillerBattery":
            //        _battery_Module.SetActive(false);
            //        _moduleInserted = false;
            //        break;
            //    case "AIDeepDrillerSolar":
            //        _solar_Panel_Module.SetActive(false);
            //        _moduleInserted = false;
            //        break;
            //}
        }

        private void OnEquipmentAdded(string slot, InventoryItem item)
        {
            QuickLogger.Info($"ModuleContainerOnAddItem Item Name {item.item.name}");

            //switch (item.item.GetTechType())
            //{
            //    case "AIDeepDrillerBattery":
            //        //_modulePower = item.item.GetComponent<InternalBatteryController>();
            //        //_battery_Module.SetActive(true);
            //        //_solar_Panel_Module.SetActive(false);
            //        //_moduleInserted = true;
            //        break;
            //    case "AIDeepDrillerSolar":
            //        _modulePower = item.item.GetComponent<PowerSource>();
            //        var module = item.item.GetComponent<AIDeepDrillerSolarController>();
            //        module.DeepDrillerObject = gameObject;
            //        _battery_Module.SetActive(false);
            //        _solar_Panel_Module.SetActive(true);
            //        _moduleInserted = true;

            //        break;
            //}
        }


        private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return false;
        }
    }
}
