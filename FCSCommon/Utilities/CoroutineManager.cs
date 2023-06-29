using System.Collections;

namespace FCSCommon.Utilities
{
    public class CoroutineManager
    {
        public static void WaitCoroutine(IEnumerator func)
        {
            while (func.MoveNext())
            {
                if (func.Current is IEnumerator num)
                {
                    WaitCoroutine(num);
                }
            }
        }
    }
}