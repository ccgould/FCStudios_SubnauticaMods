using Harmony;
using MAC.OxStation.Managers;

namespace MAC.OxStation.Patches
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
}
