using FCS_DeepDriller.Buildable;
using FCS_DeepDriller.Configuration;
using FCS_DeepDriller.Managers;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FCS_DeepDriller.Mono
{
    internal class FCSDeepDrillerBatteryController : HandTarget, IHandTarget
    {
        private FCSDeepDrillerController _mono;
        private Func<bool> _isConstructed;
        private Equipment _equipment;
        internal Action<Pickupable, string> OnBatteryAdded;
        internal Action<Pickupable> OnBatteryRemoved;


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
            equipmentRoot.transform.SetParent(gameObject.transform, false);
            equipmentRoot.AddComponent<ChildObjectIdentifier>();
            equipmentRoot.SetActive(false);

            _equipment = new Equipment(gameObject, equipmentRoot.transform);
            _equipment.SetLabel(FCSDeepDrillerBuildable.BEquipmentContainerLabel());
            _equipment.isAllowedToAdd = IsAllowedToAdd;
            _equipment.isAllowedToRemove = IsAllowedToRemove;
            _equipment.onEquip += OnEquipmentAdded;
            _equipment.onUnequip += OnEquipmentRemoved;

            AddMoreSlots();

            if (_equipment == null)
            {
                QuickLogger.Error("Equipment is null on creation");
            }
        }

        internal void AddMoreSlots()
        {
            _equipment.AddSlots(EquipmentConfiguration.SlotIDs);
            QuickLogger.Debug($"Added slots");
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
            if (slot == EquipmentConfiguration.SlotIDs[0])
            {
                DeepDrillerComponentManager.GetBatteryCellModel(1).SetActive(false);
            }
            else if (slot == EquipmentConfiguration.SlotIDs[1])
            {
                DeepDrillerComponentManager.GetBatteryCellModel(2).SetActive(false);
            }
            else if (slot == EquipmentConfiguration.SlotIDs[2])
            {
                DeepDrillerComponentManager.GetBatteryCellModel(3).SetActive(false);
            }
            else if (slot == EquipmentConfiguration.SlotIDs[3])
            {
                DeepDrillerComponentManager.GetBatteryCellModel(4).SetActive(false);
            }

            _mono.DisplayHandler.EmptyBatteryVisual(slot);
            OnBatteryRemoved?.Invoke(item.item);
        }

        private void OnEquipmentAdded(string slot, InventoryItem item)
        {
            if (slot == EquipmentConfiguration.SlotIDs[0])
                DeepDrillerComponentManager.GetBatteryCellModel(1).SetActive(true);
            else if (slot == EquipmentConfiguration.SlotIDs[1])
                DeepDrillerComponentManager.GetBatteryCellModel(2).SetActive(true);
            else if (slot == EquipmentConfiguration.SlotIDs[2])
                DeepDrillerComponentManager.GetBatteryCellModel(3).SetActive(true);
            else if (slot == EquipmentConfiguration.SlotIDs[3])
                DeepDrillerComponentManager.GetBatteryCellModel(4).SetActive(true);

            OnBatteryAdded?.Invoke(item.item, slot);
        }

        public void OnHandHover(GUIHand hand)
        {
            HandReticle main = HandReticle.main;
            main.SetInteractText(FCSDeepDrillerBuildable.OnBatteryHoverText());
            main.SetIcon(HandReticle.IconType.Hand, 1f);
        }

        public void OnHandClick(GUIHand hand)
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



            var f = Equipment.slotMapping.Where(x => x.Value == EquipmentType.PowerCellCharger);

            foreach (var VARIABLE in f)
            {
                QuickLogger.Debug($"Found slot {VARIABLE}");
            }
        }

        public bool HasBatteries()
        {
            for (int i = 0; i < EquipmentConfiguration.SlotIDs.Length; i++)
            {
                QuickLogger.Debug($" Checking battery {i + 1}");
                if (DeepDrillerComponentManager.GetBatteryCellModel(i + 1).activeSelf)
                {
                    return true;
                }
            }

            return false;
        }

        internal void LoadData(DeepDrillerPowerData data)
        {
            foreach (PowerUnitData powercellData in data.Batteries)
            {
                QuickLogger.Debug($"Adding entity {powercellData.TechType}");

                var prefab = GameObject.Instantiate(CraftData.GetPrefabForTechType(powercellData.TechType));
                prefab.gameObject.GetComponent<PrefabIdentifier>().Id = powercellData.PrefabID;

                var battery = prefab.gameObject.GetComponent<Battery>();
                battery._charge = powercellData.Charge;

                var item = new InventoryItem(prefab.gameObject.GetComponent<Pickupable>().Pickup(false));

                _equipment.AddItem(powercellData.Slot, item);
                QuickLogger.Debug($"Load Item {item.item.name} to slot {powercellData.Slot}");
            }
        }
    }
}
