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

            _mono.CraftManager.GetCraftingOperation().IsBeingCrafted = true;
            _goal = craftingItem.Amount;
            _startBuffer = 1;
            _mono.UpdateTotal(new Vector2(craftingItem.AmountCompleted,craftingItem.Amount));
            IsOccupied = true;
        }

        public int GetGoal()
        {
            return _goal;
        }

        private void Update()
        {

            if (_mono?.CraftManager?.GetCraftingOperation() == null || _mono?.Manager == null || !IsOccupied) return;

            //TODO Find a way to add to other storage systems

            if (!_mono.Manager.IsAllowedToAdd(_mono.CraftManager.GetCraftingOperation().TechType, false))
            {
                _mono.ShowMessage($"{Language.main.Get(_mono.CraftManager.GetCraftingOperation().TechType)} is not allowed in your system. Please add a server that can store this item or add an unformatted server.");
                return;
            }
            
            if (_startBuffer > 0)
            {
                _startBuffer -= DayNightCycle.main.deltaTime;
                return;
            }

            GetMissingItems(_mono.CraftManager.GetCraftingOperation().TechType);

            var hasIngredients = _mono.Manager.HasIngredientsFor(_mono.CraftManager.GetCraftingOperation().TechType);

            if (hasIngredients && !_mono.CraftManager.GetCraftingOperation().IsComplete)
            {
                _mono.CraftManager.GetCraftingOperation().AppendCompletion();
                _mono.Manager.ConsumeIngredientsFor(_mono.CraftManager.GetCraftingOperation().TechType);
                _mono.Manager.AddItemToContainer(_mono.CraftManager.GetCraftingOperation().FixCustomTechType().ToInventoryItemLegacy());
                _mono.CraftManager.SpawnItem(_mono.CraftManager.GetCraftingOperation().TechType);
                _mono.UpdateTotal(new Vector2(_mono.CraftManager.GetCraftingOperation().AmountCompleted, _mono.CraftManager.GetCraftingOperation().Amount));
                _startBuffer = MAXTIME;
            }
            else
            {
                _startBuffer = 1;
            }

            if (!hasIngredients)
            {
                if (NotMetIngredients.Any())
                {
                    _mono.AskForCraftingAssistance(NotMetIngredients.ElementAt(0).Key);
                }
            }

            if (_mono.CraftManager.GetCraftingOperation().IsComplete)
            {
                QuickLogger.Debug($"Is Complete",true);
                _mono.UpdateTotal(new Vector2(0, 0));
                OnComplete?.Invoke(_mono.CraftManager.GetCraftingOperation());
                if (!_mono.CraftManager.GetCraftingOperation().IsRecursive)
                {
                    IsOccupied = false;
                }
            }
        }

        private void GetMissingItems(TechType craftingItemTechType)
        {
            _mono.ClearMissingItems();
            NotMetIngredients.Clear();
            var missingItems = TechDataHelpers.GetIngredients(craftingItemTechType);
            foreach (IIngredient ingredient in missingItems)
            {
                if (_mono.Manager.GetItemCount(ingredient.techType) >= ingredient.amount) continue;
                NotMetIngredients.Add(ingredient.techType,ingredient.amount);
                _mono.AddMissingItem(Language.main.Get(ingredient.techType), ingredient.amount);
            }
        }

        public void Reset(bool bypass)
        {
            if (_mono.CraftManager.GetCraftingOperation() == null || _mono == null) return;
            if (!_mono.CraftManager.GetCraftingOperation().IsRecursive || bypass)
            {
                _mono.CraftManager.StopOperation();
                IsOccupied = false;
                _goal = 0;
            }
            _startBuffer = 0;
        }
    }
}