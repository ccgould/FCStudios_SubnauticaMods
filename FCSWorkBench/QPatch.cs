using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechWorkBench.Mono;
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

                var longTermFilter = new LongTermFilterBuildable();
                longTermFilter.Register();

                var shortTermFilter = new ShortTermFilterBuildable();
                shortTermFilter.Register();

                FCSTechWorkBenchBuildable.ItemsList.Add(shortTermFilter);
                FCSTechWorkBenchBuildable.ItemsList.Add(longTermFilter);

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

            return true;
        }

        public static AssetBundle Bundle { get; set; }
    }
}
