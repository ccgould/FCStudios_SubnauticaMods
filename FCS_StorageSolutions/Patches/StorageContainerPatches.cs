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
            if (fcsdevice != null && fcsdevice.StorageType == StorageType.OtherStorage) return;

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
    }


    [HarmonyPatch(typeof(StorageContainer))]
    [HarmonyPatch("OnConstructedChanged")]
    internal class StorageContainerOnConstructedChangedPatcher
    {

        [HarmonyPostfix]
        internal static void Postfix(bool constructed, StorageContainer __instance)
        {
            if (__instance == null || !constructed /*|| __instance.gameObject.GetComponentInParent<FcsDevice>()*/)
            {
                return;
            }

            var fcsdevice = __instance.gameObject.GetComponentInChildren<FcsDevice>();
            if(fcsdevice != null && fcsdevice.StorageType == StorageType.OtherStorage) return;

            QuickLogger.Debug($"OnConstructedChanged: FCSDevice: {fcsdevice} || Is Bypassing: {fcsdevice?.BypassFCSDeviceCheck} || {fcsdevice?.StorageType}", true);

            var manager = BaseManager.FindManager(__instance.prefabRoot);

            if (manager == null)
            {
                QuickLogger.Debug($"[StorageContainerPatch] Base Manager is null canceling operation");
                return;
            }

            manager.AlertNewStorageContainerPlaced(__instance);
        }
    }
}