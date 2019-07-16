using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechWorkBench.Mono;
using FCSTechWorkBench.Mono.PowerStorage;
using Harmony;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace FCSTechWorkBench
{
    public class QPatch
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
                GetPrefabs();

                var freon = new FreonBuildable();
                freon.Register();

                var psKit = new PowerStorageKitBuildable();
                psKit.Register();

                FCSTechWorkBenchBuildable.ItemsList.Add(freon);
                FCSTechWorkBenchBuildable.ItemsList.Add(psKit);

                FCSTechWorkBenchBuildable.PatchHelper();

                var harmony = HarmonyInstance.Create("com.fcstechworkbench.fcstudios");

                harmony.PatchAll(Assembly.GetExecutingAssembly());

                QuickLogger.Info("Finished patching");
            }
            catch (Exception ex)
            {
                QuickLogger.Error(ex);
            }
        }

        public static bool GetPrefabs()
        {
            QuickLogger.Debug("GetPrefabs");

            AssetBundle assetBundle = AssetHelper.Asset("FCSTechWorkBench", "fcstechworkbenchmodbundle");

            //If the result is null return false.
            if (assetBundle == null)
            {
                QuickLogger.Error($"AssetBundle is Null!");
                throw new FileLoadException();
            }

            Bundle = assetBundle;


            //We have found the asset bundle and now we are going to continue by looking for the model.
            Kit = Bundle.LoadAsset<GameObject>("UnitContainerKit");

            //If the prefab isn't null lets add the shader to the materials
            if (Kit != null)
            {
                QuickLogger.Debug($"UnitContainerKit Prefab Found!");
            }
            else
            {
                QuickLogger.Error($"UnitContainerKit Prefab Not Found!");
                return false;
            }

            return true;
        }

        public static GameObject Kit { get; set; }

        public static AssetBundle Bundle { get; set; }
    }
}
