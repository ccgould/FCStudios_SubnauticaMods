using FCS_AlterraHub.Mono.FCSPDA.Mono;
using FCS_AlterraHub.Mono.FCSPDA.Mono.ScreenItems;
using FCS_AlterraHub.Registration;
using FCSCommon.Utilities;
using HarmonyLib;

namespace FCS_AlterraHub.Patches
{
    [HarmonyPatch]
    internal class PDAEncyclopedia_Patches
    {
        [HarmonyPatch(typeof(PDAEncyclopedia), nameof(PDAEncyclopedia.Initialize))]
        [HarmonyPostfix]
        public static void Postfix(ref PDAData pdaData)
        {
            QuickLogger.Info("PDAEncyclopedia Initialize Post",true);
            EncyclopediaTabController.Tree = new CraftNode("Root");
            CraftNode.Copy(PDAEncyclopedia.tree,EncyclopediaTabController.Tree);
            //FCSAlterraHubService.PublicAPI.AddEncyclopediaEntries();
            QuickLogger.Info("PDAEncyclopedia Initialize Post Complete", true);
        }

        [HarmonyPatch(typeof(PDAEncyclopedia), nameof(PDAEncyclopedia.tree), MethodType.Getter)]
        [HarmonyPostfix]
        public static void Postfix(ref CraftNode __result)
        {
            if (FCSPDAController.Instance?.IsOpen ?? false || FCSAlterraHubService.PublicAPI.IsRegisteringEncyclopedia)
            {
                __result = EncyclopediaTabController.Tree;
            }
        }

        [HarmonyPatch(typeof(PDAEncyclopedia), nameof(PDAEncyclopedia.NotifyAdd))]
        [HarmonyPostfix]
        public static void Postfix(ref CraftNode node, ref bool verbose)
        {
            if(FCSAlterraHubService.PublicAPI.IsRegisteringEncyclopedia)
                FCSPDAController.Instance.EncyclopediaTabController.OnAddEntry(node, verbose);
        }
    }
}
