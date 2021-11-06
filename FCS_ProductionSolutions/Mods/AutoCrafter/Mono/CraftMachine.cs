using System;
using System.Collections.Generic;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.Mods.AutoCrafter.Models;
using FCS_ProductionSolutions.Mods.AutoCrafter.Models.StateMachine.States;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.AutoCrafter.Mono
{
    internal class CraftMachine : MonoBehaviour
    {
        internal bool IsOccupied { get; private set; }
        internal Action<CraftingOperation> OnComplete { get; set; }
        public Action OnItemCrafted { get; set; }
        public Func<bool> OnNeededItemFound { get; set; }
        public AutoCrafterController Crafter;

        private CraftingOperation _operation;
        
        private void DistributeLoad(CraftingOperation operation)
        {
            if (_operation.Amount > 1 || _operation.IsRecursive)
            {
                Crafter.DistributeLoad(operation);
            }
        }

        /// <summary>
        /// Starts one craft operation
        /// </summary>
        /// <param name="craftingItem">The operation to handle for crafting</param>
        public void StartCrafting(CraftingOperation craftingItem)
        {
            QuickLogger.Debug("Start Crafting");

            //Check if crafter and operation is null
            if (craftingItem == null || Crafter == null) return;

            //Set Operation
            _operation = craftingItem;

            Mod.AddCraftingOperation(_operation);
            
            QuickLogger.Debug("Start Crafting changing state to: CrafterCheckForItemsState",true);

            //Tell the state machine to Check for all the items needed
            Crafter.StateMachine.SwitchToNewState(typeof(CrafterCraftingState));
            
            DistributeLoad(craftingItem);
            
            IsOccupied = true;
        }

        public CraftingOperation GetOperation()
        {
            // this is not filled in on load from save resulting in UI not updating 
            return _operation;
        }
        
        internal bool IsCrafting()
        {
            return Crafter.StateMachine.CurrentState is CrafterCraftingState;
        }

        public IEnumerable<TechType> GetPendingItems()
        {
            return Crafter.StateMachine.QueuedItems;
        }

        public Dictionary<TechType, int> GetNeededItems()
        {
            //return Crafter.StateMachine.NeededItems.Keys;
            return Crafter.StateMachine.CurrentState.GetType() == typeof(CrafterCraftingState) ? ((CrafterCraftingState)Crafter.StateMachine.CurrentState).GetConsumables() : new Dictionary<TechType, int>();
        }

        public void CancelOperation()
        {
            Crafter.StateMachine.NeededItems.Clear();
            Crafter.StateMachine.QueuedItems.Clear();
            
            if (_operation != null)
            {
                Mod.RemoveCraftingOperation(_operation.ParentMachineUnitID);
            }
            
            foreach (InventoryItem inventoryItem in Crafter.Storage.container)
            {
                Crafter.AddItemToStorage(inventoryItem.item.GetTechType());
            }
            
            Crafter.Storage.container.Clear();
            Crafter.StateMachine.SwitchToNewState(typeof(CrafterIdleState));

            OnComplete?.Invoke(_operation);
            OnItemCrafted?.Invoke();

            _operation = null;
        }

        public void SetOperation(CraftingOperation operation)
        {
            _operation = operation;
        }
    }
}