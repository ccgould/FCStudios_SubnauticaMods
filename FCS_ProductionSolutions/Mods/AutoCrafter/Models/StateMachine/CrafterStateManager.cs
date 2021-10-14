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

        public void SetStates(Dictionary<Type, CrafterBaseState> states)
        {
            _avaliableStates = states;
        }

        private void Start()
        {
            _avaliableStates = new Dictionary<Type, CrafterBaseState>()
            {
                {typeof(CrafterIdleState), new CrafterIdleState(this)},
                {typeof(CrafterCraftingState), new CrafterCraftingState(this)},
                {typeof(CrafterCheckForItemsState), new CrafterCheckForItemsState(this)},
            };
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
            CurrentState = _avaliableStates[nextState];
            CurrentState.EnterState();
            OnStateChanged?.Invoke(CurrentState);
        }

        public CraftingOperation GetOperation()
        {
            return Crafter.CraftMachine.GetOperation();
        }
    }
}
