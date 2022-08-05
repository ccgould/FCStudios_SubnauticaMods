using System;
using System.Collections;
using UnityEngine;

namespace FCSCommon.Utilities
{
    internal class CoroutineManager
    {
        public static void WaitCoroutine(IEnumerator func)
        {
            QuickLogger.Debug("In WaitCoroutine");
            while (func.MoveNext())
            {
                QuickLogger.Debug("In While");
                if (func.Current is IEnumerator num)
                {
                    WaitCoroutine(num);
                }
            }
            QuickLogger.Debug("Out WaitCoroutine");
        }
    }
}