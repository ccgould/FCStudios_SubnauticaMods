using FCS_AlterraHub.Registration;
using HarmonyLib;


namespace FCS_AlterraHub.Patches
{

    [HarmonyPatch(typeof(Constructable))]
    [HarmonyPatch("NotifyConstructedChanged")]
    internal static class Constructable_NotifyConstructedChanged_Patch
    {
        private static void Postfix(Constructable __instance, bool constructed)
        {
            if (constructed)
            {
                FCSAlterraHubService.PublicAPI.AddBuiltTech(__instance.techType);
            }
        }
    }

    [HarmonyPatch(typeof(Constructable))]
    [HarmonyPatch("DeconstructAsync")]
    internal static class Constructable_Deconstruct_Patch
    {
        private static void Postfix(Constructable __instance)
        {
            if (__instance.constructedAmount <= 0f)
            {
                FCSAlterraHubService.PublicAPI.RemoveBuiltTech(__instance.techType);
            }
        }
    }
}