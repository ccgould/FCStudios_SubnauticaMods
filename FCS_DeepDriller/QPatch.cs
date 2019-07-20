using FCS_DeepDriller.Buildable;
using FCSCommon.Utilities;
using Harmony;
using System;
using System.Reflection;
using UnityEngine;

namespace FCS_DeepDriller
{
    public static class QPatch
    {
        public static void Patch()
        {
            QuickLogger.Info("Started patching. Version: " + QuickLogger.GetAssemblyVersion());

#if DEBUG
            QuickLogger.DebugLogsEnabled = true;
            QuickLogger.Debug("Debug logs enabled");
#endif

            try
            {
                GlobalBundle = FCSTechFabricator.QPatch.Bundle;

                FCSDeepDrillerBuildable.PatchHelper();

                var harmony = HarmonyInstance.Create("com.fcsdeepdriller.fcstudios");

                harmony.PatchAll(Assembly.GetExecutingAssembly());

                QuickLogger.Info("Finished patching");
            }
            catch (Exception ex)
            {
                QuickLogger.Error(ex);
            }
        }

        public static AssetBundle GlobalBundle { get; set; }
    }
}
