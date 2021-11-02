using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCS_ProductionSolutions.Mods.AutoCrafter.Models.StateMachine.States;
using FCS_ProductionSolutions.Mods.AutoCrafter.Mono;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.AutoCrafter.Models.StateMachine
{
    internal class CrafterStateManager : MonoBehaviour
    {
        private Dictionary<Type, CrafterBaseState> _avaliableStates;
        public AutoCrafterController Crafter;
        public CrafterBaseState CurrentState { get; private set; }
        public Dictionary<TechType, int> NeededItems { get; set; } = new();
        public Queue<TechType> QueuedItems { get; set; } = new();
        public bool BypassCraftingQueue { get; set; }

        public event Action<CrafterBaseState> OnStateChanged;

        public void SetStates()
        {
            if (_avaliableStates != null) return;

            _avaliableStates = new Dictionary<Type, CrafterBaseState>()
            {
                {typeof(CrafterIdleState), new CrafterIdleState(this)},
                {typeof(CrafterCraftingState), new CrafterCraftingState(this)}
            };
        }

        public void LoadFromSave(CrafterCraftingState craftingData)
        {
            SetStates();
            if (_avaliableStates[typeof(CrafterCraftingState)] is CrafterCraftingState t)
            {
                t._timeLeft = craftingData._timeLeft;
                t._consumable = craftingData._consumable;
                t._crafted = craftingData._crafted;
                t._operation = craftingData._operation;
                CurrentState = t;
                Crafter.CraftMachine.SetOperation(craftingData._operation);
            }

            QuickLogger.Debug($"Current After Load: {CurrentState}");
        }

        private void Start()
        {
            SetStates();
        }

        private void Update()
        {
            if (WorldHelpers.CheckIfPaused())
            {
                return;
            }

            if (CurrentState == null)
            {
                CurrentState = _avaliableStates.Values.First();
            }

            var nextState = CurrentState?.UpdateState();

            if (nextState != null && nextState != CurrentState?.GetType())
            {
                QuickLogger.Debug($"Switching to new state {nextState} : {CurrentState.GetType()}",true);
                SwitchToNewState(nextState);
            }
        }

        public void SwitchToNewState(Type nextState)
        {
            QuickLogger.Debug("Switch to new state");
            CurrentState = _avaliableStates[nextState];
            CurrentState.EnterState();
            OnStateChanged?.Invoke(CurrentState);
        }

        public CraftingOperation GetOperation()
        {
            return Crafter.CraftMachine.GetOperation();
        }

        public void Reset()
        {
            NeededItems.Clear();
            QueuedItems.Clear();
            BypassCraftingQueue = false;
        }
    }
}
