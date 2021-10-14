using System;
using System.Collections;
using System.Linq;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.AutoCrafter.Models.StateMachine.States
{
    internal class CrafterCraftingState : CrafterBaseState
    {
        private CraftingOperation _operation;
        private float _timeLeft;
        private const float MAXTIME = 5f;
        public override string Name { get; }

        public CrafterCraftingState()
        {
            
        }

        public CrafterCraftingState(CrafterStateManager manager) : base(manager)
        {
            
        }

        public override void EnterState()
        {
            QuickLogger.Debug("======================================",true);
            QuickLogger.Debug("Entering Crafter Crafting State",true);
            _operation = _manager.GetOperation();
        }

        public override Type UpdateState()
        {
            if (_operation != null)
            {
                //_timeLeft -= Time.deltaTime;
                //if (_timeLeft < 0)
                //{
                //    if (TryCraftRequiredItems())
                //    {
                //        _timeLeft = 0;
                //        return typeof(CrafterIdleState);
                //    }
                //    _timeLeft = MAXTIME;
                //}
            }

            return typeof(CrafterCraftingState);
        }

        private bool TryCraftRequiredItems()
        {
            if (_manager.BypassCraftingQueue)
            {
                CraftItem(_operation.TechType, true);

                if (_operation.IsComplete)
                {
                    CompleteOperation(_operation);
                    return true;
                }
                //else
                //{
                //    StartCrafting(_operation);
                //}
            }
            else if (_manager.QueuedItems.Any())
            {

                for (int i = 0; i < _manager.QueuedItems.Count; i++)
                {
                    CraftItem(_manager.QueuedItems.Dequeue(), false);
                }

                if (_manager.QueuedItems.Any())
                {
                    TryCraftRequiredItems();
                    return false;
                }
                else
                {
                    CraftItem(_operation.TechType, true);

                    if (_operation.IsComplete)
                    {
                        CompleteOperation(_manager.GetOperation());
                        return true;
                    }
                    else
                    {
                        //StartCrafting(_operation);
                    }
                }
            }

            return false;
        }

        private void CompleteOperation(CraftingOperation craftingOperation)
        {
            _manager.Crafter.CraftMachine.OnComplete?.Invoke(craftingOperation);
            _manager.Crafter.CrafterBelt.SpawnBeltItem(craftingOperation.FixCustomTechType());
        }

        private void CraftItem(TechType techType, bool isComplete)
        {

            QuickLogger.Debug($"CraftItem: TechType:{Language.main.Get(techType)} | IsComplete: {isComplete}", true);

            var result = UWEHelpers.ConsumeIngredientsFor(_manager.Crafter.Storage.container, techType); // turn into coroutine

            QuickLogger.Debug($"Result:  {result}", true);

            if (result)
            {
                var techData = CraftDataHandler.GetTechData(techType);

                foreach (Ingredient ingredient in techData.Ingredients)
                {
                    if (_manager.NeededItems.ContainsKey(ingredient.techType))
                    {
                        _manager.NeededItems[ingredient.techType] -= ingredient.amount;
                        if (_manager.NeededItems[ingredient.techType] <= 0)
                        {
                            _manager.NeededItems.Remove(ingredient.techType);
                        }
                    }
                }
            }
            else
            {
                QuickLogger.Debug($"All items not crafted for {Language.main.Get(techType)}. Re-adding to the bottom of the queue and crafting next in queue.", true);
                _manager.QueuedItems.Enqueue(techType);
                if (_manager.QueuedItems.Count > 1)
                    CraftItem(_manager.QueuedItems.Dequeue(), false);
                return;
            }


            if (isComplete)
            {
                for (int i = 0; i < _operation.ReturnAmount; i++)
                {
                    if (_manager.Crafter.Manager.IsAllowedToAdd(techType, 1, true))
                    {
                        AttemptToAddToNetwork(techType);
                    }
                    else
                    {
                        _manager.Crafter.AddItemToStorage(techType);
                    }
                }
                _operation.AppendCompletion();

            }
            else
            {
                _manager.Crafter.Storage.container.UnsafeAdd(techType.ToInventoryItem());
            }

            _manager.Crafter.CrafterBelt.SpawnBeltItem(techType);
            _manager.Crafter.CraftMachine.OnItemCrafted?.Invoke();
        }

        private bool AttemptToAddToNetwork(TechType techType)
        {
            var inventoryItem = techType.ToInventoryItem();

            QuickLogger.Debug($"InventoryItemLegacy returned: {Language.main.Get(inventoryItem.item.GetTechType())}");

            var result = BaseManager.AddItemToNetwork(_manager.Crafter.Manager, inventoryItem, true);

            if (!result)
            {
                _manager.Crafter.ShowMessage($"Failed to add {Language.main.Get(techType)} to storage. Please build a locker, remote storage or add more space to your data storage system. Your item will be added to the autocrafter storage/");
                _manager.Crafter.AddItemToStorage(techType);
                GameObject.Destroy(inventoryItem.item.gameObject);
            }

            return result;
        }

    }
}
