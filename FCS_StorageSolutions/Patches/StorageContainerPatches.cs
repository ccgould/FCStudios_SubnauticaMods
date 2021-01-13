using FCS_AlterraHub.Mono;
using FCSCommon.Utilities;
using HarmonyLib;

namespace FCS_StorageSolutions.Patches
{
    /**
     * Patches into StorageContainer::Awake method to alert our Resource monitor about the newly placed storage container
     */
    [HarmonyPatch(typeof(StorageContainer))]
    [HarmonyPatch("Awake")]
    internal class StorageContainerAwakePatcher
    {

        [HarmonyPostfix]
        internal static void Postfix(StorageContainer __instance)
        {
            if (__instance == null)
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


    [HarmonyPatch(typeof(StorageContainer))]
    [HarmonyPatch("OnConstructedChanged")]
    internal class StorageContainerOnConstructedChangedPatcher
    {

        [HarmonyPostfix]
        internal static void Postfix(bool constructed, StorageContainer __instance)
        {
            if (__instance == null)
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