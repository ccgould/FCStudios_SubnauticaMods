using System;
using System.Collections.Generic;
using System.Globalization;
using FCS_AlterraHub.Mono;
using FCS_StorageSolutions.Configuration;
using FCSCommon.Utilities;
using HarmonyLib;
using UnityEngine;

namespace FCS_StorageSolutions.Patches
{
    /**
     * Patches into StorageContainer::Awake method to alert our Resource monitor about the newly placed storage container
     */
    [HarmonyPatch(typeof(StorageContainer))]
    [HarmonyPatch("OnConstructedChanged")]
    internal class StorageContainerAwakePatcher
    {

        [HarmonyPostfix]
        internal static void Postfix(bool constructed, StorageContainer __instance)
        {
            if (__instance == null || !constructed)
            {
                return;
            }

            var manager = BaseManager.FindManager(__instance.prefabRoot);

            if (manager == null)
            {
#if DEBUG
                QuickLogger.Debug($"[StorageContainerPatch] GameObject Name Returned: {__instance.prefabRoot.name}");
#endif
                QuickLogger.Debug($"[StorageContainerPatch] Base Manager is null canceling operation");
                return;
            }

            manager.AlertNewStorageContainerPlaced(__instance);
        }
    }
}
