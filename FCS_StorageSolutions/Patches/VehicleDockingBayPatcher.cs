using System;
using FCS_AlterraHub.Mono;
using FCSCommon.Utilities;
using HarmonyLib;

namespace FCS_StorageSolutions.Patches
{
    internal class VehicleDockingBayPatcher
    {
        [HarmonyPatch(typeof(VehicleDockingBay))]
        [HarmonyPatch("Start")]
        internal class VehicleDockingBayPatcher_Start
        {
            [HarmonyPostfix]
            public static void Postfix(ref VehicleDockingBay __instance)
            {
                try
                {
                    var manager = BaseManager.FindManager(__instance.subRoot);
                    if (manager != null)
                    {
                        manager.DockingManager.RegisterDockingBay(__instance);
                    }
                }
                catch (Exception e)
                {
                    QuickLogger.Error($"Message: {e.Message} | StackTrace: {e.StackTrace}");
                }
            }
        }

        [HarmonyPatch(typeof(VehicleDockingBay))]
        [HarmonyPatch("OnDestroy")]
        internal class VehicleDockingBayPatcher_OnDestroy
        {
            [HarmonyPostfix]
            public static void Postfix(ref VehicleDockingBay __instance)
            {
                try
                {
                    var manager = BaseManager.FindManager(__instance.subRoot);
                    if (manager != null)
                    {
                        manager.DockingManager.UnRegisterDockingBay(__instance);
                    }
                }
                catch (Exception e)
                {
                    QuickLogger.Error($"Message: {e.Message} | StackTrace: {e.StackTrace}");
                }
            }
        }
    }
}
