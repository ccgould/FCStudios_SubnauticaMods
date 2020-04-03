using System.Linq;
using FCSCommon.Utilities;
using Harmony;
using MAC.OxStation.Mono;

namespace MAC.OxStation.Patches
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
                OxStationController[] array = UnityEngine.Object.FindObjectsOfType<OxStationController>();
                if (array.Any(t => t.TryAttachBeacon(__instance)))
                {
                    QuickLogger.Message("Beacon attached",true);
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
                OxStationController.TryDetachBeacon(__instance);
            }
        }
    }
}
