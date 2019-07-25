using FCS_DeepDriller.Buildable;
using FCS_DeepDriller.Configuration;
using FCS_DeepDriller.Enumerators;
using FCS_DeepDriller.Helpers;
using FCS_DeepDriller.Managers;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using FCSTechFabricator.Mono;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FCS_DeepDriller.Mono.Handlers
{
    internal class FCSDeepDrillerModuleContainer
    {
        private FCSDeepDrillerController _mono;
        private Func<bool> _isConstructed;
        private Equipment _equipment;

        internal static readonly string[] SlotIDs = new string[2]
        {
            "PowerCellCharger1",
            "PowerCellCharger2",
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
            _equipment.isAllowedToRemove = IsAllowedToRemove;
            _equipment.compatibleSlotDelegate = CompatibleSlotDelegate;
            _equipment.onEquip += OnEquipmentAdded;
            _equipment.onUnequip += OnEquipmentRemoved;
            _equipment.AddSlot(SlotIDs[0]);
            _equipment.AddSlot(SlotIDs[1]);
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
            _mono.RemoveAttachment(TechTypeHelpers.GetDeepModule(item.item.GetTechType()));
        }

        private void OnEquipmentAdded(string slot, InventoryItem item)
        {
            QuickLogger.Debug($"ModuleContainerOnAddItem Item Name {item.item.name}");

            if (item.item.GetTechType() == TechTypeHelpers.BatteryAttachmentTechType())
            {
                _mono.AddAttachment(DeepDrillModules.Battery);
            }

            if (item.item.GetTechType() == TechTypeHelpers.SolarAttachmentTechType())
            {
                _mono.AddAttachment(DeepDrillModules.Solar);
            }

            if (item.item.GetTechType() == TechTypeHelpers.FocusAttachmentTechType())
            {
                _mono.AddAttachment(DeepDrillModules.Focus);
            }
        }

        internal bool HasPowerModule(out DeepDrillModules module)
        {
            bool result = false;
            module = DeepDrillModules.None;

            for (int i = 0; i < SlotIDs.Length; i++)
            {
                if (_equipment.GetTechTypeInSlot(SlotIDs[i]) == TechTypeHelpers.BatteryAttachmentTechType())
                {
                    module = DeepDrillModules.Battery;
                    result = true;
                    break;
                }

                if (_equipment.GetTechTypeInSlot(SlotIDs[i]) == TechTypeHelpers.SolarAttachmentTechType())
                {
                    module = DeepDrillModules.Solar;
                    result = true;
                    break;
                }
            }

            if (!result)
            {
                DeepDrillerComponentManager.HideAllPowerAttachments();
            }
            return result;
        }

        private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            if (pickupable.gameObject.GetComponent<FCSTechFabricatorTag>() != null)
            {
                return true;
            }

            QuickLogger.Message(FCSDeepDrillerBuildable.DDAttachmentsOnly(), true);
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

        internal List<SlotData> GetCurrentModules()
        {
            var data = new List<SlotData>();

            var slot1 = _equipment.GetTechTypeInSlot(SlotIDs[0]);
            var slot2 = _equipment.GetTechTypeInSlot(SlotIDs[1]);

            data.Add(new SlotData { Module = slot1, Slot = SlotIDs[0] });

            data.Add(new SlotData { Module = slot2, Slot = SlotIDs[1] });

            return data;
        }

        internal void SetModules(List<SlotData> modules)
        {
            foreach (SlotData module in modules)
            {
                if (module.Module == TechType.None) continue;

                _equipment.AddItem(module.Slot, new InventoryItem(module.Module.ToPickupable()));
            }
        }
    }
}
