using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem.StatesMachine
{
    public class StateMachine : MonoBehaviour
    {
        private Dictionary<Type, BaseState> _avaliableStates;

        public BaseState CurrentState { get; private set; }
        public event Action<BaseState> OnStateChanged;

        public void SetStates(Dictionary<Type, BaseState> states)
        {
            _avaliableStates = states;
        }

        private void Update()
        {
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
