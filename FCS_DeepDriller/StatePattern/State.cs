using System.Collections;

namespace FCS_DeepDriller.StatePattern
{
    internal abstract class State
    {
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
