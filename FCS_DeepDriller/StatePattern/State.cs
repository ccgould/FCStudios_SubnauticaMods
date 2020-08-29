using System.Collections;
using FCS_DeepDriller.Mono.MK2;

namespace FCS_DeepDriller.StatePattern
{
    internal abstract class State
    {
        protected FCSDeepDrillerController Controller;

        public State(FCSDeepDrillerController controller)
        {
            Controller = controller;
        }

        internal virtual IEnumerator Start()
        {
            yield break;
        }

        internal virtual IEnumerator PowerOn()
        {
            yield break;
        }

        internal virtual IEnumerator PoweredOff()
        {
            yield break;
        }

        internal virtual IEnumerator Tripped()
        {
            yield break;
        }
    }
}
