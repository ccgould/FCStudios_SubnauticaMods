using System;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.AutoCrafter
{
    internal class CraftMachine : MonoBehaviour
    {
        internal bool IsOccupied { get; private set; }
        public int CrafterID { get; set; }
        internal Action<CraftingItem> OnComplete { get; set; }
        private int _goal;
        private CraftingItem _craftingItem;
        private float _startBuffer;
        private DSSAutoCrafterController _mono;
        private const float MAXTIME = 5f;

        public void StartCrafting(CraftingItem craftingItem,DSSAutoCrafterController mono)
        {
            _mono = mono;
            _craftingItem = craftingItem;
            craftingItem.StartTime = DayNightCycle.main.timePassedAsFloat;
            _craftingItem.IsBeingCrafted = true;
            _goal = craftingItem.Amount;
            _startBuffer = MAXTIME;
            IsOccupied = true;
        }

        public int GetGoal()
        {
            return _goal;
        }

        public int GetComplete()
        {
            return _craftingItem.AmountCompleted;
        }

        private void Update()
        {

            if (_craftingItem == null || _mono?.Manager == null || !IsOccupied) return;

            if (!_mono.Manager.IsAllowedToAdd(_craftingItem.TechType, false)) return;

            if (_startBuffer > 0)
            {
                _startBuffer -= DayNightCycle.main.deltaTime;
                return;
            }

            if (_mono.Manager.HasIngredientsFor(_craftingItem.TechType))
            {
                _craftingItem.AmountCompleted += 1;
                _mono.Manager.ConsumeIngredientsFor(_craftingItem.TechType);
                _mono.Manager.AddItemToContainer(_craftingItem.FixCustomTechType().ToInventoryItemLegacy());
                _mono.CraftManager.SpawnItem(_craftingItem.TechType, CrafterID);
                _startBuffer = MAXTIME;
            }
            else
            {
                _startBuffer = MAXTIME;
                return;
            }

            if (_craftingItem.IsComplete)
            {
                QuickLogger.Debug($"Is Complete",true);
                OnComplete?.Invoke(_craftingItem);
                
                if(!_craftingItem.IsRecurring)
                    IsOccupied = false;
            }

        }

        public void Reset(bool bypass)
        {
            if (_craftingItem == null || _mono == null) return;
            if (!_craftingItem.IsRecurring || bypass)
            {
                _mono.DisplayManager.RemoveCraftingItem(_craftingItem);
                _craftingItem = null;
                IsOccupied = false;
                _goal = 0;
            }
            _startBuffer = 0;
        }
    }
}