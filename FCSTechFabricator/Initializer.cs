using System.Reflection;
using FCSCommon.Utilities;
using FCSTechFabricator.Configuration;
using HarmonyLib;
using QModManager.API.ModLoading;

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

            var harmony = new Harmony("com.fcstechfabricator.fcstudios");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            FcTechFabricatorService.InternalAPI.PatchFabricator();
        }
    }
}
