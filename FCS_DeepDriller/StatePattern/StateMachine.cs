using UnityEngine;

namespace FCS_DeepDriller.StatePattern
{
    internal abstract class StateMachine : MonoBehaviour
    {
        protected State State;

        internal void SetState(State state)
        {
            State = state;
            StartCoroutine(State.Start());
        }
    }
}
