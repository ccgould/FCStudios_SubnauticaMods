using FCS_DeepDriller.Buildable;
using FCS_DeepDriller.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FCS_DeepDriller.Mono
{
    internal class FCSDeepDrillerBatteryController : MonoBehaviour
    {


        internal static readonly string[] SlotIDs = new string[4]
        {
            "PowerCellCharger1",
            "PowerCellCharger2",
            "PowerCellCharger3",
            "PowerCellCharger4",
        };

        private FCSDeepDrillerController _mono;
        private Func<bool> _isConstructed;
        private Equipment _equipment;

        internal HashSet<TechType> CompatibleTech = new HashSet<TechType>()
        {
            TechType.PowerCell,
            TechType.PrecursorIonPowerCell
        };

        internal void Setup(FCSDeepDrillerController mono)
        {
            _mono = mono;

            _isConstructed = () => mono.IsConstructed;

            var equipmentRoot = new GameObject("BEquipmentRoot");
            equipmentRoot.transform.SetParent(_mono.transform, false);
            equipmentRoot.AddComponent<ChildObjectIdentifier>();
            equipmentRoot.SetActive(false);

            _equipment = new Equipment(mono.gameObject, equipmentRoot.transform);
            _equipment.SetLabel(FCSDeepDrillerBuildable.BEquipmentContainerLabel());
            _equipment.isAllowedToAdd = IsAllowedToAdd;
            _equipment.isAllowedToRemove = IsAllowedToRemove;
            _equipment.onEquip += OnEquipmentAdded;
            _equipment.onUnequip += OnEquipmentRemoved;
            _equipment.AddSlot(SlotIDs[0]);
            _equipment.AddSlot(SlotIDs[1]);
            _equipment.AddSlot(SlotIDs[2]);
            _equipment.AddSlot(SlotIDs[3]);


        }

        private bool IsAllowedToRemove(Pickupable pickupable, bool verbose)
        {
            return true;
        }

        private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            bool flag = false;

            if (pickupable != null && CompatibleTech.Contains(pickupable.GetTechType()))
            {
                flag = true;
            }
            else
            {
                ErrorMessage.AddMessage(FCSDeepDrillerBuildable.OnlyPowercellsAllowed());
            }

            return flag;
        }

        private void OnEquipmentRemoved(string slot, InventoryItem item)
        {
            //TODO Update battery info

            switch (slot)
            {
                case "PowerCellCharger1":
                    DeepDrillerComponentManager.GetBatteryCellModel(1).SetActive(false);
                    break;
                case "PowerCellCharger2":
                    DeepDrillerComponentManager.GetBatteryCellModel(2).SetActive(false);
                    break;
                case "PowerCellCharger3":
                    DeepDrillerComponentManager.GetBatteryCellModel(3).SetActive(false);
                    break;
                case "PowerCellCharger4":
                    DeepDrillerComponentManager.GetBatteryCellModel(4).SetActive(false);
                    break;
            }
        }

        private void OnEquipmentAdded(string slot, InventoryItem item)
        {
            switch (slot)
            {
                case "PowerCellCharger1":
                    DeepDrillerComponentManager.GetBatteryCellModel(1).SetActive(true);
                    break;
                case "PowerCellCharger2":
                    DeepDrillerComponentManager.GetBatteryCellModel(2).SetActive(true);
                    break;
                case "PowerCellCharger3":
                    DeepDrillerComponentManager.GetBatteryCellModel(3).SetActive(true);
                    break;
                case "PowerCellCharger4":
                    DeepDrillerComponentManager.GetBatteryCellModel(4).SetActive(true);
                    break;
            }
        }
    }
}
