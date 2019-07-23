using FCS_DeepDriller.Buildable;
using FCS_DeepDriller.Enumerators;
using FCSCommon.Utilities;
using FCSTechFabricator.Mono;
using SMLHelper.V2.Handlers;
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
            "PowerCellCharger1",
            "PowerCellCharger2",
        };

        private bool _batteryModuleFound;
        private TechType _batteryModuleTechType;
        private bool _solarPanelFound;
        private TechType _solarPanelTechType;


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
            _equipment.isAllowedToRemove = IsAllowedToRemove;
            _equipment.compatibleSlotDelegate = CompatibleSlotDelegate;
            _equipment.onEquip += OnEquipmentAdded;
            _equipment.onUnequip += OnEquipmentRemoved;
            _equipment.AddSlot(SlotIDs[0]);
            _equipment.AddSlot(SlotIDs[1]);

            _batteryModuleFound = TechTypeHandler.TryGetModdedTechType("BatteryAttachment_DD", out TechType batteryModuleTechType);

            if (!_batteryModuleFound)
            {
                QuickLogger.Error("Deep Driller Battery Attachment TechType not found");
            }
            else
            {
                _batteryModuleTechType = batteryModuleTechType;
            }

            _solarPanelFound = TechTypeHandler.TryGetModdedTechType("SolarPanelAttachment_DD", out TechType solarPanelTechType);

            if (!_solarPanelFound)
            {
                QuickLogger.Error("Deep Driller Solar Panel TechType not found");
            }
            else
            {
                _solarPanelTechType = solarPanelTechType;
            }

        }

        private bool IsAllowedToRemove(Pickupable pickupable, bool verbose)
        {
            return _mono.IsModuleRemovable();
        }

        private bool CompatibleSlotDelegate(EquipmentType itemtype, out string slot)
        {
            QuickLogger.Debug($"ItemType = {itemtype}");

            slot = String.Empty;

            return true;
        }

        private void OnEquipmentRemoved(string slot, InventoryItem item)
        {
            if (item.item.GetTechType() == _batteryModuleTechType)
            {
                _mono.RemoveAttachment();
            }

            if (item.item.GetTechType() == _solarPanelTechType)
            {
                //_modulePower = item.item.GetComponent<PowerSource>();
                //var module = item.item.GetComponent<AIDeepDrillerSolarController>();
                //module.DeepDrillerObject = gameObject;
                //_battery_Module.SetActive(false);
                //_solar_Panel_Module.SetActive(true);
                //_moduleInserted = true;
            }
        }

        private void OnEquipmentAdded(string slot, InventoryItem item)
        {
            QuickLogger.Debug($"ModuleContainerOnAddItem Item Name {item.item.name}");

            QuickLogger.Debug($"GTP {item.item.GetTechType()} || BTP {_batteryModuleTechType}");

            if (item.item.GetTechType() == _batteryModuleTechType)
            {
                _mono.AddAttachment(DeepDrillModules.Battery);
            }

            if (item.item.GetTechType() == _solarPanelTechType)
            {
                //_mono.AddAttachement(DeepDrillModules.Solar);
                //_modulePower = item.item.GetComponent<PowerSource>();
                //var module = item.item.GetComponent<AIDeepDrillerSolarController>();
                //module.DeepDrillerObject = gameObject;
                //_battery_Module.SetActive(false);
                //_solar_Panel_Module.SetActive(true);
                //_moduleInserted = true;
            }
        }


        private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            if (pickupable.gameObject.GetComponent<FCSTechFabricatorTag>() != null)
            {
                return true;
            }

            QuickLogger.Message("Only FCS Deep Driller Attachments allowed!", true);
            return false;
        }

        internal void OpenModulesDoor()
        {
            QuickLogger.Debug("Modules Door Opened", true);
            PDA pda = Player.main.GetPDA();
            if (!pda.isInUse)
            {
                Inventory.main.SetUsedStorage(_equipment, false);
                pda.Open(PDATab.Inventory, _mono.transform, null, 4f);
            }
        }
    }
}
