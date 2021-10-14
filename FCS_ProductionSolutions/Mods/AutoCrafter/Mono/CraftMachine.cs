using System;
using System.Collections.Generic;
using FCS_AlterraHub.Mono;
using FCS_ProductionSolutions.Mods.AutoCrafter.Models.StateMachine.States;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.AutoCrafter.Mono
{
    internal class CraftMachine : MonoBehaviour
    {
        internal bool IsOccupied { get; private set; }
        public int CrafterID { get; set; }
        internal Action<CraftingOperation> OnComplete { get; set; }
        public Action OnItemCrafted { get; set; }
        public Func<bool> OnNeededItemFound { get; set; }
        public AutoCrafterController Crafter;

 

        private int _testTechType = 34; //11371;
        //private Dictionary<TechType,bool> _ingredients = new();

        private float MAXTIME = 7f;

        private CraftingOperation _operation;
        private bool _isInitialized;

        private float _checkWaitPeriod = 1f;
        private bool _isCrafting;
        private bool _bypassCraftingQueue;

        private void Start()
        {
            Initialize();
        }

        private void DistributeLoad(CraftingOperation operation)
        {
            if (_operation.Amount > 1 || _operation.IsRecursive)
            {
                Crafter.DistributeLoad(operation);
            }
        }

        public void Initialize()
        {
            if (_isInitialized) return;

            //OnComplete += item =>
            //{
            //    if (item == null) return;

            //    if (!item.IsRecursive)
            //    {
            //        _mono.Manager.RemoveCraftingOperation(item);
            //        item.UnMount(_mono);
            //        _neededItems.Clear();
            //        _isCrafting = false;
            //        _mono.Manager.NotifyByID("DTC", "RefreshCraftingGrid");
            //    }
            //};

            //    InvokeRepeating(nameof(CollectItems), 1f, 1f);

            _isInitialized = true;
        }


        /// <summary>
        /// Starts one craft operation
        /// </summary>
        /// <param name="craftingItem">The operation to handle for crafting</param>
        public void StartCrafting(CraftingOperation craftingItem)
        {
            //Check if crafter and operation is null
            if (craftingItem == null || Crafter == null) return;

            //Set Operation
            _operation = craftingItem;
            
            QuickLogger.Debug("Start Crafting changing state to: CrafterCheckForItemsState",true);
            //Tell the state machine to Check for all the items needed
            Crafter.StateMachine.SwitchToNewState(typeof(CrafterCheckForItemsState));
            
            //DistributeLoad(craftingItem);

            //GetIngredients(craftingItem.TechType);
            //GetRequiredItems(craftingItem.TechType);
            //CollectItems();
            //Now that we got the items try craft the missing Items

            //_mono.CraftManager.GetCraftingOperation().IsBeingCrafted = true;

            //StartCoroutine(TryCraftRequiredItems());

            IsOccupied = true;
        }

        public CraftingOperation GetOperation()
        {
            return _operation;
        }





        internal bool IsCrafting()
        {
            //return _neededItems.Any() || _craftingQueue.Any();
            return Crafter.StateMachine.CurrentState is CrafterCraftingState;
        }

        //public int GetGoal()
        //{
        //    return _goal;
        //}

        //private void Update()
        //{
        //    //if (WorldHelpers.CheckIfPaused()) return;



        //    //if (_mono?.CraftManager?.GetCraftingOperation() == null || Mod.Craftables == null || _mono?.Manager == null || !IsOccupied) return;

        //    //var techType = _mono.CraftManager.GetCraftingOperation().TechType;
        //    //var amount = _mono.CraftManager.GetCraftingOperation().ReturnAmount;

        //    //if (!Mod.Craftables.Contains(techType)) return;

        //    //if (!_mono.Manager.IsAllowedToAdd(techType, amount, true, false))
        //    //{
        //    //    _mono.ShowMessage($"{Language.main.Get(techType)} is not allowed in your system. Please add a server that can store this item or add an unformatted server.");
        //    //    _mono.CancelOperation();
        //    //    return;
        //    //}

        //    //if (_startBuffer > 0)
        //    //{
        //    //    _startBuffer -= DayNightCycle.main.deltaTime;
        //    //    return;
        //    //}

        //    //if (!Mod.Craftables.Contains(techType)) return;

        //    //var hasIngredients = _mono.Manager.HasIngredientsFor(techType);

        //    //if (hasIngredients && !_mono.CraftManager.GetCraftingOperation().IsComplete)
        //    //{
        //    //    _mono.CraftManager.GetCraftingOperation().AppendCompletion();
        //    //    _mono.Manager.ConsumeIngredientsFor(techType);
        //    //    var craftingTechType = _mono.CraftManager.GetCraftingOperation().FixCustomTechType();

        //    //    for (int i = 0; i < amount; i++)
        //    //    {
        //    //        AttemptToAddToNetwork(craftingTechType);
        //    //    }

        //    //    _startBuffer = MAXTIME;
        //    //    _mono.CraftManager.SpawnItem(_mono.CraftManager.GetCraftingOperation().TechType);
        //    //}
        //    //else
        //    //{
        //    //    _startBuffer = 1;
        //    //}

        //    //if (!hasIngredients)
        //    //{
        //    //    if (NotMetIngredients.Any())
        //    //    {
        //    //        _mono.AskForCraftingAssistance(NotMetIngredients.ElementAt(0).Key);
        //    //    }
        //    //}

        //    //if (_mono.CraftManager.GetCraftingOperation().IsComplete)
        //    //{
        //    //    QuickLogger.Debug($"Is Complete", true);
        //    //    OnComplete?.Invoke(_mono?.CraftManager?.GetCraftingOperation());

        //    //    if (_mono?.CraftManager?.GetCraftingOperation() == null)
        //    //    {
        //    //        IsOccupied = false;
        //    //    }
        //    //    else if (!_mono.CraftManager.GetCraftingOperation().IsRecursive)
        //    //    {
        //    //        IsOccupied = false;
        //    //    }
        //    //}
        //}



        ////private void GetIngredients(TechType techType)
        ////{
        ////    _ingredients.Clear();

        ////    foreach (IIngredient ingredient in TechDataHelpers.GetIngredients(techType))
        ////    {
        ////        _ingredients.Add(ingredient.techType, _mono.Manager.GetItemCount(ingredient.techType) >= ingredient.amount);
        ////    }
        ////}

        //public void Reset(bool bypass)
        //{
        //    //if (_mono?.CraftManager?.GetCraftingOperation() == null) return;
        //    //if (!_mono.CraftManager.GetCraftingOperation().IsRecursive || bypass)
        //    //{
        //    //    _mono.CraftManager.StopOperation();
        //    //    IsOccupied = false;
        //    //    _goal = 0;
        //    //}
        //    //_startBuffer = 0;
        //}

        public IEnumerable<TechType> GetPendingItems()
        {
            return Crafter.StateMachine.QueuedItems;
        }

        public IEnumerable<TechType> GetNeededItems()
        {
            return Crafter.StateMachine.NeededItems.Keys;
        }

        public void CancelOperation()
        {
            Crafter.StateMachine.NeededItems.Clear();
            Crafter.StateMachine.QueuedItems.Clear();
            Crafter.Manager.RemoveCraftingOperation(_operation);
            OnComplete?.Invoke(_operation);
            OnItemCrafted?.Invoke();
            OnNeededItemFound?.Invoke();
            _operation = null;
            _isCrafting = false;
            foreach (InventoryItem inventoryItem in Crafter.Storage.container)
            {
                Crafter.AddItemToStorage(inventoryItem.item.GetTechType());
            }
            Crafter.Storage.container.Clear();
        }
    }
}