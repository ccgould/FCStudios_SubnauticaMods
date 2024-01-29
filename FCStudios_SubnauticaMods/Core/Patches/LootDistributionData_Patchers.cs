using FCS_AlterraHub.Core.Services;
using FCSCommon.Utilities;
using HarmonyLib;

namespace FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Plugins;

internal class LootDistributionData_Patchers
{
    [HarmonyPatch(typeof(LootDistributionData))]
    [HarmonyPatch("Initialize")]
    public static class LootDistributionDataPatch
    {
        [HarmonyPostfix]
        public static void Postfix(LootDistributionData __instance)
        {
            QuickLogger.Debug($"Initialize Loot Distribution Data: {__instance}");
            BiomeManager.LootDistributionData = __instance;
        }
    }
}
