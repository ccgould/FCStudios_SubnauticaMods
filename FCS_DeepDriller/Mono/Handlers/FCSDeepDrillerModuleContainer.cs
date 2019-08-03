using FCS_DeepDriller.Buildable;
using FCS_DeepDriller.Configuration;
using FCS_DeepDriller.Enumerators;
using FCS_DeepDriller.Helpers;
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
        private bool _powerModuleAttached;

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
            _equipment.AddSlot(EquipmentConfiguration.SlotIDs[4]);
            _equipment.AddSlot(EquipmentConfiguration.SlotIDs[5]);
        }

        private bool IsAllowedToRemove(Pickupable pickupable, bool verbose)
        {
            if (pickupable.GetTechType() == TechTypeHelper.BatteryAttachmentTechType())
            {
                if (_mono.BatteryController.GetController().HasBatteries())
                {
                    QuickLogger.Message(FCSDeepDrillerBuildable.BatteryAttachmentHasBatteries(), true);
                    return false;
                }
            }

            return true;
        }

        private bool CompatibleSlotDelegate(EquipmentType itemtype, out string slot)
        {
            QuickLogger.Debug($"ItemType = {itemtype}");

            slot = String.Empty;

            return true;
        }

        private void OnEquipmentRemoved(string slot, InventoryItem item)
        {
            if (item.item.GetTechType() == TechTypeHelper.BatteryAttachmentTechType())
            {
                _powerModuleAttached = false;
            }
            else if (item.item.GetTechType() == TechTypeHelper.SolarAttachmentTechType())
            {
                _powerModuleAttached = false;
            }

            _mono.RemoveAttachment(TechTypeHelper.GetDeepModule(item.item.GetTechType()));
        }

        private void OnEquipmentAdded(string slot, InventoryItem item)
        {
            QuickLogger.Debug($"ModuleContainerOnAddItem Item Name {item.item.name}");

            if (item.item.GetTechType() == TechTypeHelper.BatteryAttachmentTechType())
            {
                _mono.AddAttachment(DeepDrillModules.Battery);
                _powerModuleAttached = true;
            }
            else if (item.item.GetTechType() == TechTypeHelper.SolarAttachmentTechType())
            {
                _mono.AddAttachment(DeepDrillModules.Solar);
                _powerModuleAttached = true;
            }
            else if (item.item.GetTechType() == TechTypeHelper.FocusAttachmentTechType())
            {
                _mono.AddAttachment(DeepDrillModules.Focus);
            }
        }

        internal bool IsPowerModuleAttached() => _powerModuleAttached;
        internal bool HasPowerModule(out DeepDrillModules module)
        {
            bool result = false;
            module = DeepDrillModules.None;

            for (int i = 0; i < EquipmentConfiguration.SlotIDs.Length; i++)
            {
                if (_equipment.GetTechTypeInSlot(EquipmentConfiguration.SlotIDs[i]) == TechTypeHelper.BatteryAttachmentTechType())
                {
                    module = DeepDrillModules.Battery;
                    result = true;
                    break;
                }

                if (_equipment.GetTechTypeInSlot(EquipmentConfiguration.SlotIDs[i]) == TechTypeHelper.SolarAttachmentTechType())
                {
                    module = DeepDrillModules.Solar;
                    result = true;
                    break;
                }
            }

            if (!result)
            {
                _mono.ComponentManager.HideAllPowerAttachments();
            }
            return result;
        }

        private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            if (pickupable.gameObject.GetComponent<FCSTechFabricatorTag>() != null)
            {
                if (pickupable.GetTechType() == TechTypeHelper.BatteryAttachmentTechType())
                {
                    if (_powerModuleAttached)
                    {
                        QuickLogger.Message(FCSDeepDrillerBuildable.OnePowerAttachmentAllowed(), true);
                        return false;
                    }
                }
                else if (pickupable.GetTechType() == TechTypeHelper.SolarAttachmentTechType())
                {
                    if (_powerModuleAttached)
                    {
                        QuickLogger.Message(FCSDeepDrillerBuildable.OnePowerAttachmentAllowed(), true);
                        return false;
                    }
                }

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

            var slot1 = _equipment.GetTechTypeInSlot(EquipmentConfiguration.SlotIDs[4]);
            var slot2 = _equipment.GetTechTypeInSlot(EquipmentConfiguration.SlotIDs[5]);

            data.Add(new SlotData { Module = slot1, Slot = EquipmentConfiguration.SlotIDs[4] });

            data.Add(new SlotData { Module = slot2, Slot = EquipmentConfiguration.SlotIDs[5] });

            return data;
        }

        internal void SetModules(List<SlotData> modules)
        {
            foreach (SlotData module in modules)
            {
                if (module.Module == TechType.None) continue;

                _equipment.AddItem(module.Slot, new InventoryItem(module.Module.ToPickupable().Pickup(false)));
            }
        }

        internal bool HasSolarModule()
        {
            bool result = false;

            for (int i = 0; i < EquipmentConfiguration.SlotIDs.Length; i++)
            {
                if (_equipment.GetTechTypeInSlot(EquipmentConfiguration.SlotIDs[i]) != TechTypeHelper.SolarAttachmentTechType()) continue;
                result = true;
                break;
            }

            return result;
        }

        internal bool IsEmpty()
        {
            bool result = true;

            for (int i = 0; i < EquipmentConfiguration.SlotIDs.Length; i++)
            {
                var slot = EquipmentConfiguration.SlotIDs[i];

                if (!slot.StartsWith("HDD")) continue;

                var techType = _equipment.GetTechTypeInSlot(slot);

                if (techType != TechType.None)
                {
                    result = false;
                }
            }

            return result;
        }

        internal DeepDrillModules GetPowerModule()
        {
            var module = DeepDrillModules.None;

            for (int i = 0; i < EquipmentConfiguration.SlotIDs.Length; i++)
            {
                if (_equipment.GetTechTypeInSlot(EquipmentConfiguration.SlotIDs[i]) == TechTypeHelper.BatteryAttachmentTechType())
                {
                    module = DeepDrillModules.Battery;
                    break;
                }

                if (_equipment.GetTechTypeInSlot(EquipmentConfiguration.SlotIDs[i]) == TechTypeHelper.SolarAttachmentTechType())
                {
                    module = DeepDrillModules.Solar;
                    break;
                }
            }

            return module;
        }
    }
}
