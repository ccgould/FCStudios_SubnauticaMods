using System;
using System.IO;
using System.Reflection;
using AlterraGen.Buildables;
using AlterraGen.Configuration;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Spawnables;
using FCSCommon.Utilities;
using QModManager.API.ModLoading;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace AlterraGen
{
    [QModCore]
    public class QPatch
    {
        internal static ConfigFile Configuration { get; set; }
        internal static string Version { get; set; }
        internal static AssetBundle GlobalBundle { get; set; }

        [QModPatch]
        public static void Patch()
        {
            Version = QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly());
            QuickLogger.Info($"Started patching. Version: {Version}");
            QuickLogger.ModName = Mod.ModFriendlyName;


#if DEBUG
            QuickLogger.DebugLogsEnabled = true;
            QuickLogger.Debug("Debug logs enabled");
#endif

            try
            {
                GlobalBundle = FCSAssetBundlesService.PublicAPI.GetAssetBundleByName(FCSAssetBundlesService.PublicAPI
                        .GlobalBundleName);

                if (GlobalBundle == null)
                {
                    QuickLogger.Error("Global Bundle has returned null stopping patching");
                    throw new FileNotFoundException("Bundle failed to load");
                }

                Configuration = Mod.LoadConfiguration();

                var alterraGen = new AlterraGenBuildable();
                alterraGen.Patch();
                
                QuickLogger.Info("Finished patching");
            }
            catch (Exception ex)
            {
                QuickLogger.Error(ex);
            }
        }
    }
}

