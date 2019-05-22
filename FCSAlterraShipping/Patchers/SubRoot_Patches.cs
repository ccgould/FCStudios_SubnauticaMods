using FCSAlterraShipping.Models;
using Harmony;

namespace FCSAlterraShipping.Patchers
{
    [HarmonyPatch(typeof(SubRoot))]
    [HarmonyPatch("Awake")]
    internal class SubRoot_Awake
    {
        [HarmonyPostfix]
        public static void Postfix(ref SubRoot __instance)
        {
            if (!__instance.isCyclops)
            {
                var manager = ShippingTargetManager.FindManager(__instance);
            }
        }
    }
}
