using FCS_AIMarineTurbine.Buildable;
using FCS_AIMarineTurbine.Model;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using Harmony;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace FCS_AIMarineTurbine
{
    public static class QPatch
    {
        public static AssetBundle Bundle { get; private set; }
        public static void Patch()
        {
            var assembly = Assembly.GetExecutingAssembly();
            QuickLogger.Info("Started patching. Version: " + QuickLogger.GetAssemblyVersion(assembly));

#if DEBUG
            QuickLogger.DebugLogsEnabled = true;
            QuickLogger.Debug("Debug logs enabled");
            //TODO remove on release
            //AIWindSurferBuildable.PatchSMLHelper(); 
#endif

            try
            {
                LoadAssetBundle();
                AISolutionsData.PatchHelper();
                AIJetStreamT242Buildable.PatchSMLHelper();
                AIMarineMonitorBuildable.PatchSMLHelper();
                AIWindSurferBuildable.PatchSMLHelper();

                var harmony = HarmonyInstance.Create("com.aijetstreamt242.fcstudios");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                QuickLogger.Info("Finished patching");
                
                QuickLogger.Debug("Unload Bundle");
                Bundle.Unload(false);
                QuickLogger.Debug("Bundle Unloaded");
            }
            catch (Exception ex)
            {
                QuickLogger.Error(ex.Message);
            }
        }

        private static void LoadAssetBundle()
        {
            QuickLogger.Debug("GetPrefabs");
            AssetBundle assetBundle = AssetHelper.Asset($"FCS_MarineTurbine", "aimarineturbinemodbundle");

            //If the result is null return false.
            if (assetBundle == null)
            {
                QuickLogger.Error($"AssetBundle is Null!");
                throw new FileLoadException();
            }

            Bundle = assetBundle;
        }
    }
}
