using System;
using System.IO;
using System.Reflection;
using AE.BaseTeleporter.Buildables;
using AE.BaseTeleporter.Configuration;
using FCSCommon.Utilities;
using Harmony;
using Oculus.Newtonsoft.Json;
using UnityEngine;


namespace AE.BaseTeleporter
{
    public class QPatch
    {
        public static AssetBundle GlobalBundle { get; set; }
        public static void Patch()
        {
            Version = QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly());
            QuickLogger.Info($"Started patching. Version: {Version}");


#if DEBUG
            QuickLogger.DebugLogsEnabled = true;
            QuickLogger.Debug("Debug logs enabled");
#endif

            try
            {
                GlobalBundle = FCSTechFabricator.QPatch.Bundle;

                if (GlobalBundle == null)
                {
                    QuickLogger.Error("Global Bundle has returned null stopping patching");
                    throw new FileNotFoundException("Bundle failed to load");
                }

                LoadConfiguration();

                BaseTeleporterBuildable.PatchSMLHelper();

                var harmony = HarmonyInstance.Create("com.baseteleporter.fcstudios");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                QuickLogger.Info("Finished patching");
            }
            catch (Exception ex)
            {
                QuickLogger.Error(ex);
            }
        }

        private static void LoadConfiguration()
        {
            // == Load Configuration == //
            string configJson = File.ReadAllText(Mod.GetModInfoPath().Trim());

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.MissingMemberHandling = MissingMemberHandling.Ignore;

            //LoadData
            Configuration = JsonConvert.DeserializeObject<ModConfiguration>(configJson, settings);
        }

        internal static ModConfiguration Configuration { get; set; }
        internal static string Version { get; set; }
    }
}
