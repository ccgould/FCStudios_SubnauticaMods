using FCS_DeepDriller.Buildable;
using FCS_DeepDriller.Configuration;
using FCSCommon.Utilities;
using System;
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
            foreach (var slotID in EquipmentConfiguration.SlotIDs)
            {
                if (!slotID.StartsWith("DD")) continue;
                _equipment.AddSlot(slotID);
                QuickLogger.Debug($"Added slot {slotID}");
            }
        }

        private bool IsAllowedToRemove(Pickupable pickupable, bool verbose)
        {
            return true;
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
                ErrorMessage.AddMessage(FCSDeepDrillerBuildable.OnlyPowercellsAllowed());
            }

            return flag;
        }

        private void OnEquipmentRemoved(string slot, InventoryItem item)
        {
            if (slot == EquipmentConfiguration.SlotIDs[0])
            {
                _mono.ComponentManager.GetBatteryCellModel(1).SetActive(false);
            }
            else if (slot == EquipmentConfiguration.SlotIDs[1])
            {
                _mono.ComponentManager.GetBatteryCellModel(2).SetActive(false);
            }
            else if (slot == EquipmentConfiguration.SlotIDs[2])
            {
                _mono.ComponentManager.GetBatteryCellModel(3).SetActive(false);
            }
            else if (slot == EquipmentConfiguration.SlotIDs[3])
            {
                _mono.ComponentManager.GetBatteryCellModel(4).SetActive(false);
            }

            _mono.DisplayHandler.EmptyBatteryVisual(slot);
            OnBatteryRemoved?.Invoke(item.item);
        }

        private void OnEquipmentAdded(string slot, InventoryItem item)
        {
            if (slot == EquipmentConfiguration.SlotIDs[0])
                _mono.ComponentManager.GetBatteryCellModel(1).SetActive(true);
            else if (slot == EquipmentConfiguration.SlotIDs[1])
                _mono.ComponentManager.GetBatteryCellModel(2).SetActive(true);
            else if (slot == EquipmentConfiguration.SlotIDs[2])
                _mono.ComponentManager.GetBatteryCellModel(3).SetActive(true);
            else if (slot == EquipmentConfiguration.SlotIDs[3])
                _mono.ComponentManager.GetBatteryCellModel(4).SetActive(true);

            OnBatteryAdded?.Invoke(item.item, slot);
        }

        public void OnHandHover(GUIHand hand)
        {
            HandReticle main = HandReticle.main;
#if SUBNAUTICA
            main.SetInteractText(FCSDeepDrillerBuildable.OnBatteryHoverText());
#elif BELOWZERO
            main.SetText(HandReticle.TextType.Hand, FCSDeepDrillerBuildable.OnBatteryHoverText(), false);
#endif
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
                if (_mono.ComponentManager.GetBatteryCellModel(i + 1) != null)
                {
                    if (_mono.ComponentManager.GetBatteryCellModel(i + 1).activeSelf)
                    {
                        return true;
                    }
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

#if SUBNAUTICA
                var item = new InventoryItem(prefab.gameObject.GetComponent<Pickupable>().Pickup(false));
#elif BELOWZERO
                Pickupable pickupable = prefab.gameObject.GetComponent<Pickupable>();
                pickupable.Pickup(false);
                var item = new InventoryItem(pickupable);
#endif

                _equipment.AddItem(powercellData.Slot, item);
                QuickLogger.Debug($"Load Item {item.item.name} to slot {powercellData.Slot}");
            }
        }
    }
}
