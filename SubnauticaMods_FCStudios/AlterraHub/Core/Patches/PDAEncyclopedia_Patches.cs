using FCS_AlterraHub.ModItems.FCSPDA.Mono.Dialogs;
using FCS_AlterraHub.ModItems.FCSPDA.Mono;
using FCSCommon.Utilities;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UWE;
using FCS_AlterraHub.Core.Services;

namespace FCS_AlterraHub.Core.Patches
{
#if SUBNAUTICA
    [HarmonyPatch]
    internal class PDAEncyclopedia_Patches
    {
        [HarmonyPatch(typeof(PDAEncyclopedia), nameof(PDAEncyclopedia.Initialize))]
        [HarmonyPostfix]
        public static void Initialize_Postfix(ref PDAData pdaData)
        {
            QuickLogger.Debug("PDAEncyclopedia Initialize Post", true);
            CraftNode node = new CraftNode("Root");
            CraftNode.Copy(PDAEncyclopedia.tree, node);
            EncyclopediaTabController.Tree = node;
            EncyclopediaService.RegisterEncyclopediaEntries(EncyclopediaService.EncyclopediaEntries);

            QuickLogger.Debug("PDAEncyclopedia Initialize Post Complete", true);
        }

        [HarmonyPatch(typeof(PDAEncyclopedia), nameof(PDAEncyclopedia.tree), MethodType.Getter)]
        [HarmonyPostfix]
        public static void Getter_Postfix(ref CraftNode __result)
        {
            if (FCSPDAController.Main?.IsOpen ?? false || EncyclopediaService.IsRegisteringEncyclopedia)
            {
                __result = EncyclopediaTabController.Tree;
            }
        }

        [HarmonyPatch(typeof(PDAEncyclopedia), nameof(PDAEncyclopedia.NotifyAdd))]
        [HarmonyPrefix]
        public static bool NotifyAdd_Prefix(ref CraftNode node, ref bool verbose)
        {
            var craftNode = node;

            if (!EncyclopediaService.EncyclopediaEntries.Any(x => x.ContainsKey(craftNode.id))) return true;

            if (FCSPDAController.Main?.Screen.EncyclopediaTabController != null)
                FCSPDAController.Main.Screen.EncyclopediaTabController.OnAddEntry(node, verbose);
            else
                CoroutineHost.StartCoroutine(HoldNotifications(node, verbose));

            return false;
        }

        private static IEnumerator HoldNotifications(CraftNode node, bool verbose)
        {
            yield return new WaitUntil(() => FCSPDAController.Main?.Screen.EncyclopediaTabController != null);

            FCSPDAController.Main.Screen.EncyclopediaTabController.OnAddEntry(node, verbose);
            yield break;
        }

        [HarmonyPatch(typeof(PDAEncyclopedia), nameof(PDAEncyclopedia.GetParent))]
        [HarmonyPrefix]
        public static bool GetParent_Prefix(ref CraftNode __result, PDAEncyclopedia.EntryData entryData, bool create)
        {
            if (!EncyclopediaService.EncyclopediaEntries.Any(x => x.ContainsKey(entryData.key))) return true;

            __result = FCSPDAController.Main.Screen.EncyclopediaTabController.GetParent(entryData, create);
            return false;
        }


        [HarmonyPatch(typeof(PDAEncyclopedia), nameof(PDAEncyclopedia.Add), typeof(string), typeof(PDAEncyclopedia.Entry), typeof(bool))]
        [HarmonyPrefix]
        public static bool Add_Prefix(ref PDAEncyclopedia.EntryData __result, string key, PDAEncyclopedia.Entry entry, bool verbose)
        {
            if (string.IsNullOrEmpty(key) || !EncyclopediaService.EncyclopediaEntries.Any(x => x.ContainsKey(key))) return true;

            __result = FCSPDAController.Main.Screen.EncyclopediaTabController.Add(key, entry, verbose);
            return false;
        }

    }
#endif
}
