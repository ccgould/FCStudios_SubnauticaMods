using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCSCommon.Utilities;
using GasPodCollector.Mono;
using Harmony;

namespace GasPodCollector.Patches
{
    internal class Beacon_Patches
    {
        // Token: 0x0200001E RID: 30
        [HarmonyPatch(typeof(Beacon), "Throw")]
        internal static class Beacon_Throw_Patch
        {
            // Token: 0x06000089 RID: 137 RVA: 0x00003A04 File Offset: 0x00001C04
            private static void Postfix(Beacon __instance)
            {
                GaspodCollectorController[] array = UnityEngine.Object.FindObjectsOfType<GaspodCollectorController>();
                if (array.Any(t => t.TryAttachBeacon(__instance)))
                {
                    QuickLogger.Message("Beacon attached", true);
                    return;
                }
            }
        }

        [HarmonyPatch(typeof(Beacon), "OnPickedUp")]
        internal static class Beacon_OnPickedUp_Patch
        {
            // Token: 0x06000088 RID: 136 RVA: 0x000039FA File Offset: 0x00001BFA
            private static void Prefix(Beacon __instance)
            {
                GaspodCollectorController.TryDetachBeacon(__instance);
            }
        }
    }
}
