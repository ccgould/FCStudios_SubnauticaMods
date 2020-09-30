using DataStorageSolutions.Mono;
using HarmonyLib;

namespace DataStorageSolutions.Patches
{
    [HarmonyPatch(typeof(IngameMenu))]
    [HarmonyPatch("QuitGame")]
    public class InGameMenuQuitPatcher
    {

        [HarmonyPostfix]
        public static void Postfix()
        {
            BaseManager.SetAllowedToNotify(false);
        }
    }
}
