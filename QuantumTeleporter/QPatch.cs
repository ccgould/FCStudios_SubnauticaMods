using System;
using System.IO;
using System.Reflection;
using FCSCommon.Utilities;
using Harmony;
using Oculus.Newtonsoft.Json;
using QuantumTeleporter.Buildable;
using QuantumTeleporter.Configuration;
using UnityEngine;

namespace QuantumTeleporter
{
    public class QPatch
    {
        internal static AssetBundle GlobalBundle { get; set; }
        
        internal static ModConfiguration Configuration { get; set; }
        
        public static void Patch()
        {
            var version = QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly());
            QuickLogger.Info($"Started patching. Version: {version}");


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

                QuantumTeleporterBuildable.PatchSMLHelper();

                var harmony = HarmonyInstance.Create("com.quantumteleporter.fcstudios");
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
            string configJson = File.ReadAllText(Mod.ConfigurationFile().Trim());

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.MissingMemberHandling = MissingMemberHandling.Ignore;

            //LoadData
            Configuration = JsonConvert.DeserializeObject<ModConfiguration>(configJson, settings);
        }
    }
}
