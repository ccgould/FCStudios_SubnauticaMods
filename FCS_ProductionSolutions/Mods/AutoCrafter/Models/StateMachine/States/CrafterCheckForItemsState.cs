using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCSCommon.Utilities;
using UnityEngine;


namespace FCS_ProductionSolutions.Mods.AutoCrafter.Models.StateMachine.States
{
    class CrafterCheckForItemsState : CrafterBaseState
    {
        public override string Name { get; }
        private CraftingOperation _operation;
        private float _timeLeft;

        public CrafterCheckForItemsState()
        {
            
        }

        public CrafterCheckForItemsState(CrafterStateManager manager) : base(manager)
        {

        }

        public override void EnterState()
        {
            _operation = _manager.GetOperation();

            if (_operation == null)
            {
                QuickLogger.DebugError("Operation is null",true);
                return;
            }
            QuickLogger.Debug("Enter State Crafter Check For Items",true);
            for (int i = 0; i < _operation.Amount; i++)
            {
                GetRequiredItems(_operation.TechType);
            }
        }

        public override Type UpdateState()
        {
             //Once we have an operation check for items

            if (_operation != null)
            {
                _timeLeft -= Time.deltaTime;
                if (_timeLeft < 0)
                {
                    if (_manager.NeededItems.Any(x => x.Value > 0))
                    {
                        CollectItems();
                    }
                    _timeLeft = 1f;
                }
            }

            return CheckIfAllItemsAvailable() ? typeof(CrafterCraftingState) : typeof(CrafterCheckForItemsState);
        }

        /// <summary>
        /// Gets all required items for the craft and adds to it the needed Items list or queue
        /// </summary>
        /// <param name="craftingItemTechType"></param>
        private void GetRequiredItems(TechType craftingItemTechType)
        {
            QuickLogger.Debug("GetRequiredItems",true);
            var ingredients = new List<IIngredient>(TechDataHelpers.GetIngredients(craftingItemTechType));
            QuickLogger.Debug($"Ingredients Count: {ingredients.Count}", true);
            foreach (IIngredient ingredient in ingredients)
            {
                var amount = _manager.NeededItems.ContainsKey(ingredient.techType) ? ingredient.amount + _manager.NeededItems[ingredient.techType] : ingredient.amount;

                QuickLogger.Debug($"Item count in base: {_manager.Crafter.Manager.GetItemCount(ingredient.techType)}");

                if (_manager.Crafter.Manager.GetItemCount(ingredient.techType) >= amount)
                {
                    AppendIngredient(ingredient.techType, ingredient.amount);
                    continue;
                }

                if (TechDataHelpers.GetIngredientCount(ingredient.techType) > 0)
                {
                    _manager.QueuedItems.Enqueue(ingredient.techType);
                    GetRequiredItems(ingredient.techType);
                }
                else
                {
                    AppendIngredient(ingredient.techType, ingredient.amount);
                }
            }

            if (!_manager.QueuedItems.Any() && _manager.NeededItems.Any())
            {
                _manager.BypassCraftingQueue = true;
            }
        }

        /// <summary>
        /// Appends Items to the needed items list
        /// </summary>
        /// <param name="techType"></param>
        /// <param name="amount"></param>
        private void AppendIngredient(TechType techType, int amount)
        {
            QuickLogger.Debug("===================================", true);
            QuickLogger.Debug("AppendIngredient", true);
            if (_manager.NeededItems.ContainsKey(techType))
            {
                _manager.NeededItems[techType] += amount;
                QuickLogger.Debug($"Added amount to {techType}", true);
            }
            else
            {
                _manager.NeededItems.Add(techType, amount);
                QuickLogger.Debug($"Added {techType}", true);
            }
            QuickLogger.Debug("===================================", true);
        }

        /// <summary>
        /// Collects items from the base and adds it to the storage of the crafter
        /// </summary>
        private void CollectItems()
        {
            if (!_manager.NeededItems.Any()) return;

            var crafter = _manager.Crafter;
            
            for (var i = _manager.NeededItems.Count - 1; i >= 0; i--)
            {
                var currentItem = _manager.NeededItems.ElementAt(i);

                if (crafter.Manager.HasItem(currentItem.Key))
                {
                    var amount = crafter.Manager.GetItemCount(currentItem.Key);

                    var takeAmount = amount >= currentItem.Value ? currentItem.Value : amount;

                    for (int j = 0; j < takeAmount; j++)
                    {
                        var item = crafter.Manager.TakeItem(currentItem.Key);

                        QuickLogger.Debug($"Is Item Null: {item is null}", true);

                        if (item != null)
                        {
                            QuickLogger.Debug("Removing item from NeededItems",true);
                            QuickLogger.Debug($"InventoryItem Found: {item?.GetTechType()}",true);

                            crafter.Storage.container.UnsafeAdd(item.ToInventoryItem());
                            QuickLogger.Debug($"Container Amount: {crafter.Storage.container.count}", true);
                            RemoveNeededItemFromList(currentItem.Key);
                        }
                    }
                }
            }
        }

        private void RemoveNeededItemFromList(TechType techType)
        {
            QuickLogger.Debug($"Removing {techType} from NeededItems", true);
            _manager.NeededItems[techType] -= 1;
            QuickLogger.Debug($"{techType} amount : { _manager.NeededItems[techType]}", true);
            if (_manager.NeededItems[techType] <= 0)
            {
                _manager.NeededItems.Remove(techType);
                QuickLogger.Debug($"Removed {techType} from NeededItems list", true);
            }

            _manager.Crafter.CraftMachine.OnNeededItemFound?.Invoke();
        }

        private bool CheckIfAllItemsAvailable()
        {
            foreach (KeyValuePair<TechType, int> item in _manager.NeededItems)
            {
                if (_manager.Crafter.Storage.container.GetCount(item.Key) < item.Value)
                {
                    return false;
                }
            }

            return true;
        }


    }
}
