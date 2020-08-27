using System.Linq;
using CyclopsUpgradeConsole.Buildables;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using MoreCyclopsUpgrades.API.Buildables;
using UnityEngine;

namespace CyclopsUpgradeConsole.Mono
{
    internal class CUCController : AuxiliaryUpgradeConsole, IConstructable
    {
        private readonly GameObject[] _slots = new GameObject[6];
        private bool _slotsFound;
        private bool _runStartUpOnEnable;
        internal bool IsInitialized { get; set; }
        internal bool IsConstructed { get; set; }
        internal CUCDisplayManager DisplayManager { get; private set; }

        private void OnEnable()
        {
            if (!_runStartUpOnEnable) return;

            if (!IsInitialized)
            {
                Initialize();
            }

            RefreshSlots();

            _runStartUpOnEnable = false;
        }

        private void RefreshSlots()
        {
            foreach (var upgradeSlot in UpgradeSlots)
            {
                if (upgradeSlot.HasItemInSlot())
                {
                    ToggleSlot(upgradeSlot.slotName, true);
                }
            }
        }

        private void Initialize()
        {
            if (!_slotsFound)
            {
                FindSlots();
            }

            IsInitialized = true;
        }

        private void FindSlots()
        {
            for (int i = 0; i < 6; i++)
            {
                _slots[i] = GameObjectHelpers.FindGameObject(gameObject, $"module_{i + 1}");
            }

            if (_slots.Count(x => x != null) == 6)
            {
                _slotsFound = true;
            }
        }
        
        private void ToggleSlot(string slot, bool value)
        {
            if (!IsInitialized || !CheckDisplay()) return;

            switch (slot)
            {
                case "Module1":
                    _slots[0]?.SetActive(value);
                    break;
                case "Module2":
                    _slots[1]?.SetActive(value);
                    break;
                case "Module3":
                    _slots[2]?.SetActive(value);
                    break;
                case "Module4":
                    _slots[3]?.SetActive(value);
                    break;
                case "Module5":
                    _slots[4]?.SetActive(value);
                    break;
                case "Module6":
                    _slots[5]?.SetActive(value);
                    break;
            }

            if (value)
            {
                DisplayManager.SetIcon(UpgradeSlots.Single(x => x.slotName == slot));
            }
            else
            {
                DisplayManager.RemoveIcon(UpgradeSlots.Single(x => x.slotName == slot));
            }
        }

        private bool CheckDisplay()
        {
            if (DisplayManager == null)
            {
                DisplayManager = gameObject.GetComponent<CUCDisplayManager>();
                DisplayManager.Setup();
            }

            return DisplayManager != null;
        }

        public override void OnSlotEquipped(string slot, InventoryItem item)
        {
            QuickLogger.Debug($"Added to {item.item.GetTechName()} slot {slot}", true);
            ToggleSlot(slot, true);
        }
        
        public override void OnSlotUnequipped(string slot, InventoryItem item)
        {
            QuickLogger.Debug($"Removed {item.item.GetTechName()} from slot {slot}", true);
            ToggleSlot(slot, false);
        }

        public bool CanDeconstruct(out string reason)
        {
            if (_slots.Any(slot => slot != null && slot.activeSelf))
            {
                reason = CUCBuildable.NotEmpty();
                return false;
            }

            reason = "";
            return true;
        }

        public void OnConstructedChanged(bool constructed)
        {
            IsConstructed = constructed;

            if (constructed)
            {
                if (isActiveAndEnabled)
                {
                    if (!IsInitialized)
                    {
                        Initialize();
                    }

                    IsInitialized = true;
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        protected override string OnHoverText => CUCBuildable.HoverText();
    }
}
