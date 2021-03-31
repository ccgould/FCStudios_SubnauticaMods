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

            _mono.GetCraftingItem().IsBeingCrafted = true;
            _goal = craftingItem.Amount;
            _startBuffer = MAXTIME;
            IsOccupied = true;
        }

        public int GetGoal()
        {
            return _goal;
        }



        private void Update()
        {

            if (_mono?.GetCraftingItem() == null || _mono?.Manager == null || !IsOccupied) return;

            if (!_mono.Manager.IsAllowedToAdd(_mono.GetCraftingItem().TechType, false))
            {
                _mono.ShowMessage($"{Language.main.Get(_mono.GetCraftingItem().TechType)} is not allowed in your system. Please add a server that can store this item or add an unformatted server.");
                return;
            }

            if (_startBuffer > 0)
            {
                _startBuffer -= DayNightCycle.main.deltaTime;
                return;
            }

            GetMissingItems(_mono.GetCraftingItem().TechType);

            if (_mono.Manager.HasIngredientsFor(_mono.GetCraftingItem().TechType) && !_mono.GetCraftingItem().IsComplete)
            {
                _mono.GetCraftingItem().AppendCompletion();
                _mono.Manager.ConsumeIngredientsFor(_mono.GetCraftingItem().TechType);
                _mono.Manager.AddItemToContainer(_mono.GetCraftingItem().FixCustomTechType().ToInventoryItemLegacy());
                _mono.CraftManager.SpawnItem(_mono.GetCraftingItem().TechType);
                _startBuffer = MAXTIME;
            }
            else
            {
                _startBuffer = MAXTIME;
            }

            if (_mono.GetCraftingItem().IsComplete)
            {
                QuickLogger.Debug($"Is Complete",true);
                OnComplete?.Invoke(_mono.GetCraftingItem());

                if (!_mono.GetCraftingItem().IsRecursive)
                {
                    IsOccupied = false;
                }
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
            if (_mono.GetCraftingItem() == null || _mono == null) return;
            if (!_mono.GetCraftingItem().IsRecursive || bypass)
            {
                _mono.ClearCraftingItem();
                IsOccupied = false;
                _goal = 0;
            }
            _startBuffer = 0;
        }
    }
}