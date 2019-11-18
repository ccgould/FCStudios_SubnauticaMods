using AE.BaseTeleporter.Managers;
using Harmony;

namespace AE.BaseTeleporter.Patchers
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
