using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using SMLHelper.V2.Handlers;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.AutoCrafter
{
    internal class CraftMachine : MonoBehaviour
    {
        internal bool IsOccupied { get; private set; }
        public int CrafterID { get; set; }
        internal Action<CraftingOperation> OnComplete { get; set; }
        private int _goal;
        private CraftingOperation _craftingItem;
        private float _startBuffer;
        private DSSAutoCrafterController _mono;
        private const float MAXTIME = 5f;

        public Dictionary<TechType, int> NotMetIngredients { get; set; } = new Dictionary<TechType, int>();

        public void StartCrafting(CraftingOperation craftingItem,DSSAutoCrafterController mono)
        {
            _mono = mono;

            NotMetIngredients.Clear();

            if (craftingItem == null)return;

            QuickLogger.Debug($"[StartCraft] Crafting:{craftingItem.TechType}",true);

            _craftingItem = craftingItem;
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

            if (!_mono.Manager.IsAllowedToAdd(_craftingItem.TechType, false))
            {
                _mono.ShowMessage($"{Language.main.Get(_craftingItem.TechType)} is not allowed in your system. Please add a server that can store this item or add an unformatted server.");
                return;
            }

            if (_startBuffer > 0)
            {
                _startBuffer -= DayNightCycle.main.deltaTime;
                return;
            }

            GetMissingItems(_craftingItem.TechType);

            if (_mono.Manager.HasIngredientsFor(_craftingItem.TechType))
            {
                _craftingItem.AmountCompleted += 1;
                _mono.Manager.ConsumeIngredientsFor(_craftingItem.TechType);
                _mono.Manager.AddItemToContainer(_craftingItem.FixCustomTechType().ToInventoryItemLegacy());
                _mono.CraftManager.SpawnItem(_craftingItem.TechType);
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
                
                if(!_craftingItem.IsRecursive)
                    IsOccupied = false;
            }

        }

        private void GetMissingItems(TechType craftingItemTechType)
        {
            _mono.ClearMissingItems();
            var missingItems = TechDataHelpers.GetIngredients(craftingItemTechType);
            foreach (IIngredient ingredient in missingItems)
            {
                if (_mono.Manager.HasItem(ingredient.techType)) continue;
                _mono.AddMissingItem(Language.main.Get(ingredient.techType), ingredient.amount);
            }
        }

        public void Reset(bool bypass)
        {
            if (_craftingItem == null || _mono == null) return;
            if (!_craftingItem.IsRecursive || bypass)
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