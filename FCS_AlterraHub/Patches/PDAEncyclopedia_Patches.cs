using System;
using System.Collections;
using System.Linq;
using FCS_AlterraHub.Mods.FCSPDA.Mono;
using FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems;
using FCS_AlterraHub.Registration;
using FCSCommon.Utilities;
using HarmonyLib;
using UnityEngine;
using UWE;

namespace FCS_AlterraHub.Patches
{
    [HarmonyPatch]
    internal class PDAEncyclopedia_Patches
    {
        [HarmonyPatch(typeof(PDAEncyclopedia), nameof(PDAEncyclopedia.Initialize))]
        [HarmonyPostfix]
        public static void Initialize_Postfix(ref PDAData pdaData)
        {
            QuickLogger.Info("PDAEncyclopedia Initialize Post",true);
            CraftNode node = new CraftNode("Root");
            CraftNode.Copy(PDAEncyclopedia.tree,node);
            EncyclopediaTabController.Tree = node;

            FCSAlterraHubService.InternalAPI.RegisterEncyclopediaEntries(FCSAlterraHubService.InternalAPI.EncyclopediaEntries);

            QuickLogger.Info("PDAEncyclopedia Initialize Post Complete", true);
        }

        [HarmonyPatch(typeof(PDAEncyclopedia), nameof(PDAEncyclopedia.tree), MethodType.Getter)]
        [HarmonyPostfix]
        public static void Getter_Postfix(ref CraftNode __result)
        {
            if (FCSPDAController.Instance?.IsOpen ?? false || FCSAlterraHubService.PublicAPI.IsRegisteringEncyclopedia)
            {
                __result = EncyclopediaTabController.Tree;
            }
        }

        [HarmonyPatch(typeof(PDAEncyclopedia), nameof(PDAEncyclopedia.NotifyAdd))]
        [HarmonyPrefix]
        public static bool NotifyAdd_Prefix(ref CraftNode node,ref bool verbose)
        {
            var craftNode = node;

            if (!FCSAlterraHubService.InternalAPI.EncyclopediaEntries.Any(x=>x.ContainsKey(craftNode.id))) return true;

            if (FCSPDAController.Instance?.EncyclopediaTabController != null)
                FCSPDAController.Instance.EncyclopediaTabController.OnAddEntry(node, verbose);
            else
                CoroutineHost.StartCoroutine(HoldNotifications(node, verbose));
            
            return false;
        }

        private static IEnumerator HoldNotifications(CraftNode node, bool verbose)
        {
            yield return new WaitUntil(() => FCSPDAController.Instance?.EncyclopediaTabController != null);
            
            FCSPDAController.Instance.EncyclopediaTabController.OnAddEntry(node, verbose);
            yield break; 
        }  
        
        [HarmonyPatch(typeof(PDAEncyclopedia), nameof(PDAEncyclopedia.Add), new Type[]{typeof(string), typeof(PDAEncyclopedia.Entry), typeof(bool)})]
        [HarmonyPrefix]
        public static void Add_Prefix(string key, bool verbose)
        {
            if (FCSAlterraHubService.InternalAPI.EncyclopediaEntries.Any(x=>x.ContainsKey(key)))
                FCSAlterraHubService.PublicAPI.IsRegisteringEncyclopedia = true;
        }
        
        [HarmonyPatch(typeof(PDAEncyclopedia), nameof(PDAEncyclopedia.Add),new Type[]{typeof(string), typeof(PDAEncyclopedia.Entry), typeof(bool)})]
        [HarmonyPostfix]
        public static void Add_Postfix(string key, bool verbose)
        {
            if(FCSAlterraHubService.InternalAPI.EncyclopediaEntries.Any(x => x.ContainsKey(key)))
                FCSAlterraHubService.PublicAPI.IsRegisteringEncyclopedia = true;
        }
    }
}
