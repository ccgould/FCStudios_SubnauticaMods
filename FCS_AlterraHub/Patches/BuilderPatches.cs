using FCSCommon.Utilities;
using HarmonyLib;


namespace FCS_AlterraHub.Patches
{
    [HarmonyPatch(typeof(Constructable), nameof(Constructable.Construct))]
    public class Constructable_Construct_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Constructable __instance)
        {
            if (__instance.constructed)
            {
                QuickLogger.Debug($"Constructed: {Language.main.Get(__instance.techType)}", true);
                QPatch.QuestManagerGM.NotifyTechTypeConstructed(__instance.techType);
            }
        }
    }
}