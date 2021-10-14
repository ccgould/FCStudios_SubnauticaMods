using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem.StatesMachine.States;
using UnityEngine;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem.StatesMachine
{
    public class StateMachine : MonoBehaviour
    {
        private Dictionary<Type, BaseState> _avaliableStates;

        internal BaseState CurrentState { get; private set; }
        internal event Action<BaseState> OnStateChanged;

        internal void SetStates(Dictionary<Type, BaseState> states)
        {
            _avaliableStates = states;
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

            var nextState = CurrentState?.Tick();

            if (nextState != null && nextState != CurrentState?.GetType())
            {
                SwitchToNewState(nextState);
            }
        }

        public void SwitchToNewState(Type nextState)
        {
            CurrentState = _avaliableStates[nextState];
            OnStateChanged?.Invoke(CurrentState);
        }
    }
}
