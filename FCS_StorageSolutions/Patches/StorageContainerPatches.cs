using FCS_AlterraHub.Enumerators;
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

            // TODO Find effective way to check this. (Trying to get the DSS C48 to display items in Wind Surfer Operator)
            var fcsdevice = __instance.gameObject.GetComponentInChildren<FcsDevice>();

            if (fcsdevice != null && IsInvalidStorageType(fcsdevice.StorageType)) return;

            QuickLogger.Debug($"Awake: FCSDevice: {fcsdevice} || Is Bypassing: {fcsdevice?.BypassFCSDeviceCheck} || {fcsdevice?.StorageType}", true);

            //if (fcsdevice != null && !fcsdevice.BypassFCSDeviceCheck)
            //{
            //    return;
            //}

            var manager = BaseManager.FindManager(__instance.prefabRoot);

            if (manager == null)
            {
                QuickLogger.Debug($"[StorageContainerPatch] Base Manager is null canceling operation");
                return;
            }

            manager.AlertNewStorageContainerPlaced(__instance);
        }

        internal static bool IsInvalidStorageType(StorageType storageType)
        {
            return storageType == StorageType.RemoteStorage || storageType == StorageType.Replicator ||
                   storageType == StorageType.Harvester || storageType == StorageType.SeaBreeze;
        }
    }


#if SUBNAUTICA_STABLE
    [HarmonyPatch(typeof(StorageContainer))]
    [HarmonyPatch(nameof(StorageContainer.OnConstructedChanged))]
#else
    [HarmonyPatch(typeof(Constructable))]
    [HarmonyPatch(nameof(Constructable.OnConstructedChanged))]
#endif
    internal class StorageContainerOnConstructedChangedPatcher
    {

        [HarmonyPostfix]
        internal static void Postfix(bool constructed, Constructable __instance)
        {
            if (__instance == null || !constructed /*|| __instance.gameObject.GetComponentInParent<FcsDevice>()*/)
            {
                return;
            }

            var fcsdevice = __instance.gameObject.GetComponentInChildren<FcsDevice>();
            var storageContainer = __instance.gameObject.GetComponentInChildren<StorageContainer>(true);
            if(fcsdevice != null && StorageContainerAwakePatcher.IsInvalidStorageType(fcsdevice.StorageType)) return;

            QuickLogger.Debug($"OnConstructedChanged: FCSDevice: {fcsdevice} || Is Bypassing: {fcsdevice?.BypassFCSDeviceCheck} || {fcsdevice?.StorageType}", true);

            var manager = BaseManager.FindManager(__instance.gameObject);

            if (manager == null)
            {
                QuickLogger.Debug($"[StorageContainerPatch] Base Manager is null canceling operation");
                return;
            }

            manager.AlertNewStorageContainerPlaced(storageContainer);
        }
    }
}