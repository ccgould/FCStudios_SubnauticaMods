using AE.MiniFountainFilter.Buildable;
using AE.MiniFountainFilter.Configuration;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using Harmony;
using Oculus.Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace AE.MiniFountainFilter
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

                var configBottleTechType = QPatch.Configuration.Config.BottleTechType;
                
                if (configBottleTechType == null)
                {
                    QuickLogger.Error("Bottle TechType is null setting to default");
                    BottleTechType = TechType.DisinfectedWater;
                }
                else
                {
                    BottleTechType = configBottleTechType.ToTechType();
                }

                if (BottleTechType == TechType.None)
                {
                    QuickLogger.Error("TechType returned None");
                }
                
                MiniFountainFilterBuildable.PatchSMLHelper();

                var harmony = HarmonyInstance.Create("com.minifountainfilter.fcstudios");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                QuickLogger.Info("Finished patching");
            }
            catch (Exception ex)
            {
                QuickLogger.Error(ex);
            }
        }

        public static TechType BottleTechType { get; set; }

        private static void LoadConfiguration()
        {
            // == Load Configuration == //
            string configJson = File.ReadAllText(Mod.ConfigurationFile().Trim());

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.MissingMemberHandling = MissingMemberHandling.Ignore;

            //LoadData
            Configuration = JsonConvert.DeserializeObject<ModConfiguration>(configJson, settings);
        }

        public static ModConfiguration Configuration { get; set; }

        public static string Version { get; set; }
    }
}
