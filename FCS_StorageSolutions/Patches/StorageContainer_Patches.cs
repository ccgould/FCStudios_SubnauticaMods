using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Mono;
using FCS_StorageSolutions.Models;
using FCS_StorageSolutions.Models.Enumerator;
using FCS_StorageSolutions.Services;
using FCSCommon.Utilities;
using HarmonyLib;
using System;
using System.Collections;
using UnityEngine;
using UWE;
using static FCS_StorageSolutions.Patches.StorageContainer_Patches;
using static VFXParticlesPool;

namespace FCS_StorageSolutions.Patches;
internal class StorageContainer_Patches
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

            //var manager = HabitatService.main.GetBaseManager(__instance.prefabRoot);

            //if (manager is null)
            //{
            //    QuickLogger.DebugError($"[StorageContainerPatch_Awake] Habitat Manager is null canceling operation");
            //}
            //else
            //{
            //    QuickLogger.Debug($"StorageContainer_Awake: 1 {__instance.gameObject.name}");

            //    if (__instance == null || InvalidComponent(__instance.gameObject))
            //    {
            //        return;
            //    }

            //    QuickLogger.Debug($"StorageContainer_Awake: 2 {__instance.gameObject.name}");

            //    var fcsdevice = __instance.gameObject.GetComponentInChildren<FCSDevice>();
            //    var fcsStorage = __instance.gameObject.GetComponentInChildren<FCSStorage>();

            //    QuickLogger.Debug($"StorageContainer_Awake: 3 {__instance.gameObject.name}");


            //    if (fcsdevice is not null && (fcsStorage is not null && !fcsStorage.IsVisibleInNetwork)) return;

            //    QuickLogger.Debug($"Awake: FCSDevice: {fcsdevice} || FCSStorage is null: {fcsStorage is null} || Is Visible in Network {fcsStorage?.IsVisibleInNetwork}", true);

            //    QuickLogger.Debug($"StorageContainer_Awake: 4 {__instance.gameObject.name}");

            //    QuickLogger.Debug($"StorageContainer_Awake: 5 {__instance.gameObject.name}");


            //    var dssManager = manager.gameObject.GetComponentInChildren<DSSManager>();//DSSService.main.GetDSSManager(manager);

            //    QuickLogger.Debug($"StorageContainer_Awake: 6 {__instance.gameObject.name}");

            //    if (dssManager is null)
            //    {
            //        QuickLogger.DebugError($"[StorageContainerPatch_Awake] DSS Manager is null canceling operation");
            //        return;
            //    }

            //    var techTag = __instance.gameObject.GetComponentInParent<TechTag>();

            //    if (techTag is not null)
            //    {
            //        dssManager.RegisterStorage(techTag.type, __instance);
            //    }
            //    else
            //    {
            //        QuickLogger.DebugError($"StorageContainer_Awake: TechTag was not found on {__instance.gameObject.name}");
            //    }



            //    QuickLogger.Debug($"StorageContainer_Awake: 7 {__instance.gameObject.name}");
            }            
        }


        //internal static bool InvalidComponent(GameObject __instance)
        //{
        //    if (__instance.GetComponent<Planter>() ||
        //        __instance.GetComponent<Aquarium>())
        //    {
        //        return true;
        //    }
        //    return false;
        //}

        //internal static bool IsInvalidStorageType(StorageType storageType)
        //{
        //    return storageType == StorageType.RemoteStorage || storageType == StorageType.Replicator ||
        //           storageType == StorageType.Harvester || storageType == StorageType.SeaBreeze;
        //}
    }


    [HarmonyPatch(typeof(Constructable))]
    [HarmonyPatch(nameof(Constructable.OnConstructedChanged))]
    internal class StorageContainerOnConstructedChangedPatcher
    {

        [HarmonyPostfix]
        internal static void Postfix(bool constructed, Constructable __instance)
        {

        var storageContainer = __instance.gameObject.GetComponent<StorageContainer>();

        if (!constructed || storageContainer is null || !LargeWorldStreamer.main.IsWorldSettled()) return;

        var manager = HabitatService.main.GetBaseManager(__instance.gameObject);

        if (manager == null) return;

        var dssManager = DSSService.main.GetDSSManager(manager);
        var techTag = __instance.gameObject.GetComponentInParent<TechTag>();
        dssManager.RegisterStorage(techTag.type, storageContainer);
        

        //QuickLogger.Debug("[Constructable.OnConstructedChanged.PostFix] : 1");


        //if (__instance == null || !constructed || StorageContainerAwakePatcher.InvalidComponent(__instance.gameObject))
        //{
        //    return;
        //}

        //var manager = HabitatService.main.GetBaseManager(__instance.gameObject);

        //if (manager is not null)
        //{
        //    var fcsdevice = __instance.gameObject.GetComponentInChildren<FCSDevice>();

        //    var fcsStorage = __instance.gameObject.GetComponentInChildren<FCSStorage>();


        //    if (fcsdevice is not null && (fcsStorage is not null && !fcsStorage.IsVisibleInNetwork) || storageContainer is null) return;

        //    if (manager == null)
        //    {
        //        QuickLogger.DebugError($"[StorageContainerPatch_OnConstructedChanged] DSS Manager is null canceling operation");
        //        return;
        //    }

        //    var dssManager = DSSService.main.GetDSSManager(manager);

        //    var techTag = __instance.gameObject.GetComponentInParent<TechTag>();

        //    if (dssManager is null)
        //    {
        //        QuickLogger.DebugError($"StorageContainer_ConstructionChanged: DSSManager was not found.");
        //        return;
        //    }

        //    if (techTag is not null)
        //    {
        //        dssManager.RegisterStorage(techTag.type, storageContainer);
        //    }
        //    else
        //    {
        //        QuickLogger.DebugError($"StorageContainer_ConstructionChanged: TechTag was not found on {__instance.gameObject.name}");
        //    }
        //}
        //}
    }
}