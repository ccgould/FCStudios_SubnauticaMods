using System;
using System.Collections;
using UnityEngine;

namespace FCSCommon.Utilities
{
    public class CouroutineManager
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