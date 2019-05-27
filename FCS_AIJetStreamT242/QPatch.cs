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
            QuickLogger.Info("Started patching. Version: " + QuickLogger.GetAssemblyVersion());

#if DEBUG
            QuickLogger.DebugLogsEnabled = true;
            QuickLogger.Debug("Debug logs enabled");
#endif

            try
            {
                LoadAssetBundle();
                //Set Rotation
                AISolutionsData.ChangeRotation();
                AIJetStreamT242Patcher.PatchSMLHelper();
                AIMarineMonitorPatcher.PatchSMLHelper();

                var harmony = HarmonyInstance.Create("com.aijetstreamt242.fcstudios");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                QuickLogger.Info("Finished patching");
            }
            catch (Exception ex)
            {
                QuickLogger.Error(ex);
            }
        }

        private static void LoadAssetBundle()
        {
            QuickLogger.Debug("GetPrefabs");
            AssetBundle assetBundle = AssetHelper.Asset($"FCSAIMarineTurbine", "aimarineturbinemodbundle");

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
