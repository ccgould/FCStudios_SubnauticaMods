using DataStorageSolutions.Model;
using FCSCommon.Utilities;
using HarmonyLib;

namespace DataStorageSolutions.Patches
{
    [HarmonyPatch(typeof(SubRoot))]
    [HarmonyPatch("Awake")]
    internal class SubRoot_Awake
    {
        [HarmonyPostfix]
        public static void Postfix(ref SubRoot __instance)
        {
            var manager = BaseManager.FindManager(__instance);
        }
    }

    [HarmonyPatch(typeof(SubRoot))]
    [HarmonyPatch("OnKill")]
    internal class SubRoot_OnKill
    {
        [HarmonyPostfix]
        public static void Postfix(ref SubRoot __instance)
        {
            QuickLogger.Debug("On Kill",true);
            //var manager = BaseManager.FindManager(__instance);
        }
    }
}
