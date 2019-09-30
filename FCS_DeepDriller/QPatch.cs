using FCS_DeepDriller.Buildable;
using FCS_DeepDriller.Ores;
using FCS_DeepDriller.Patchers;
using FCSCommon.Utilities;
using Harmony;
using System;
using System.IO;
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
#if USE_ExStorageDepot
            QuickLogger.Info("Ex-Storage Version");
#endif
            try
            {
                GlobalBundle = FCSTechFabricator.QPatch.Bundle;

                if (GlobalBundle == null)
                {
                    QuickLogger.Error("Global Bundle has returned null stopping patching");
                    throw new FileNotFoundException("Bundle failed to load");
                }

                FCSDeepDrillerBuildable.PatchHelper();

                SandSpawnable.PatchHelper();

                var harmony = HarmonyInstance.Create("com.fcsdeepdriller.fcstudios");

                harmony.Patch(typeof(Equipment).GetMethod("GetSlotType"),
                    new HarmonyMethod(typeof(Equipment_GetSlotType_Patch), "Prefix"), null);

                harmony.Patch(typeof(uGUI_Equipment).GetMethod("Awake",
                        BindingFlags.NonPublic |
                        BindingFlags.Instance |
                        BindingFlags.SetField),
                    new HarmonyMethod(typeof(uGUI_Equipment_Awake_Patch), "Prefix"),
                    new HarmonyMethod(typeof(uGUI_Equipment_Awake_Patch), "Postfix"));

                //harmony.PatchAll(Assembly.GetExecutingAssembly());

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
