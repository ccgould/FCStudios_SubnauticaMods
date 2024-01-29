using FCS_AlterraHub.Core.Services;
using FCSCommon.Utilities;
using HarmonyLib;
using System;
using System.Collections;
using UnityEngine;

namespace FCS_StorageSolutions.Patches;
internal class VehicleDockingBayPatcher
{
    [HarmonyPatch(typeof(VehicleDockingBay))]
    [HarmonyPatch("Start")]
    internal class VehicleDockingBayPatcher_Start
    {
        internal static IEnumerator __coroutine;

        [HarmonyPostfix]
        public static void Postfix(ref VehicleDockingBay __instance)
        {
            if (!Plugin.Configuration.IsDataStorageSolutionsEnabled) return;

            try
            {
                QuickLogger.Debug($"[VehicleDockingBayPatcher_Start] [{__instance.gameObject.name}]");
                var manager = HabitatService.main.GetBaseManager(__instance.gameObject);

                if (manager is not null)
                {
                    manager.RegisterDockingBay(__instance);
                    //CoroutineHost.StartCoroutine(TryRegisterDockingBay(__instance));
                }
                else
                {
                    QuickLogger.DebugError($"[VehicleDockingBayPatcher_Start] No Manager Found {__instance.gameObject.name}");
                }
            }
            catch (Exception e)
            {
                QuickLogger.Error($"Message: {e.Message} | StackTrace: {e.StackTrace}");
            }
        }

        private static IEnumerator TryRegisterDockingBay(VehicleDockingBay instance)
        {

            int i = 1;
            var manager = HabitatService.main.GetBaseManager(instance.gameObject);


            while (manager is null)
            {
                yield return new WaitForSeconds(1);
                QuickLogger.Debug($"[TryRegisterDockingBay {instance.gameObject.name}] {i++}");
                yield return null;
            }

            manager.RegisterDockingBay(instance);
            __coroutine = null;
            yield break;
        }

    }

    [HarmonyPatch(typeof(VehicleDockingBay))]
    [HarmonyPatch("OnDestroy")]
    internal class VehicleDockingBayPatcher_OnDestroy
    {
        [HarmonyPostfix]
        public static void Postfix(ref VehicleDockingBay __instance)
        {
            QuickLogger.Debug($"[VehicleDockingBayPatcher_OnDestroy] {__instance.gameObject.name}");

            if (!Plugin.Configuration.IsDataStorageSolutionsEnabled) return;

            try
            {
                if (__instance.subRoot == null || HabitatService.main is null) return;
                var manager = HabitatService.main.GetBaseManager(__instance.gameObject);

                manager?.UnRegisterDockingBay(__instance);
            }
            catch (Exception e)
            {
                QuickLogger.Error($"Message: {e.Message} | StackTrace: {e.StackTrace}");
            }
        }
    }
}
