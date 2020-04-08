using System.IO;
using System.Reflection;
using FCSCommon.Utilities;
using FCSTechFabricator.Components;
using FCSTechFabricator.Configuration;
using Harmony;
using QModManager.API.ModLoading;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCSTechFabricator
{
    [QModCore]
    public static class Initializer
    {
        [QModPostPatch]
        public static void PatchFabricator()
        {
            var version = QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly());
            QuickLogger.Info($"Started patching. Version: {version}");


#if DEBUG
            QuickLogger.DebugLogsEnabled = true;
            QuickLogger.Debug("Debug logs enabled");
#endif

            var harmony = HarmonyInstance.Create("com.fcstechfabricator.fcstudios");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            
            FcTechFabricatorService.InternalAPI.PatchFabricator();
        }
    }
}
