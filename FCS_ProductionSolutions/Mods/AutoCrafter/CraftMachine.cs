using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCS_ProductionSolutions.Configuration;
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
            IsOccupied = true;
        }

        public int GetGoal()
        {
            return _goal;
        }

        private void Update()
        {
            if (_mono?.CraftManager?.GetCraftingOperation() == null || Mod.Craftables == null || _mono?.Manager == null || !IsOccupied) return;

            var techType = _mono.CraftManager.GetCraftingOperation().TechType;
            var amount = _mono.CraftManager.GetCraftingOperation().ReturnAmount;

            if(!Mod.Craftables.Contains(techType)) return;

            if (!_mono.Manager.IsAllowedToAdd(techType, amount, true,false))
            {
                _mono.ShowMessage($"{Language.main.Get(techType)} is not allowed in your system. Please add a server that can store this item or add an unformatted server.");
                _mono.CancelOperation();
                return;
            }
            
            if (_startBuffer > 0)
            {
                _startBuffer -= DayNightCycle.main.deltaTime;
                return;
            }

            GetMissingItems(techType);

            var hasIngredients = _mono.Manager.HasIngredientsFor(techType);

            if (hasIngredients && !_mono.CraftManager.GetCraftingOperation().IsComplete)
            {
                _mono.CraftManager.GetCraftingOperation().AppendCompletion();
                _mono.Manager.ConsumeIngredientsFor(techType);
                var craftingTechType = _mono.CraftManager.GetCraftingOperation().FixCustomTechType();
                
                for (int i = 0; i < amount; i++)
                {
                    StartCoroutine(AttemptToAddToNetwork(craftingTechType, techType));
                }
                
                _startBuffer = MAXTIME;
                _mono.CraftManager.SpawnItem(_mono.CraftManager.GetCraftingOperation().TechType);
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
                OnComplete?.Invoke(_mono?.CraftManager?.GetCraftingOperation());

                if (_mono?.CraftManager?.GetCraftingOperation() == null)
                {
                    IsOccupied = false;
                }
                else if(!_mono.CraftManager.GetCraftingOperation().IsRecursive)
                {
                    IsOccupied = false;
                }
            }
        }

        private IEnumerator AttemptToAddToNetwork(TechType craftingTechType, TechType techType)
        {
            TaskResult<InventoryItem> taskResult = new TaskResult<InventoryItem>();
            yield return AsyncExtensions.ToInventoryItemLegacyAsync(techType, taskResult);
            var inventoryItem = taskResult.Get();

            var result = BaseManager.AddItemToNetwork(_mono.Manager, inventoryItem, true);

            if (!result)
            {
                _mono.ShowMessage($"Failed to add {Language.main.Get(techType)} to storage. Please build a locker, remote storage or add more space to your data storage system. Your item will be added to the autocrafter storage/");
                _mono.AddItemToStorage(techType);
                Destroy(inventoryItem.item.gameObject);
            }
            yield break;
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
            if (_mono?.CraftManager?.GetCraftingOperation() == null) return;
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