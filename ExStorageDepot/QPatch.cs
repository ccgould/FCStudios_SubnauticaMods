using ExStorageDepot.Buildable;
using ExStorageDepot.Configuration;
using FCSCommon.Utilities;
using Harmony;
using Oculus.Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;


namespace ExStorageDepot
{
    public static class QPatch
    {
        internal static Config Config { get; private set; }

        public static void Patch()
        {
            QuickLogger.Info("Started patching. Version: " + QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly()));

#if DEBUG
            QuickLogger.DebugLogsEnabled = true;
            QuickLogger.Debug("Debug logs enabled");
#endif

            try
            {
                LoadConfiguration();

                ExStorageDepotBuildable.PatchHelper();

                var harmony = HarmonyInstance.Create("com.exstoragedepot.fcstudios");
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
            Config = JsonConvert.DeserializeObject<Config>(configJson, settings);
        }
    }
}
