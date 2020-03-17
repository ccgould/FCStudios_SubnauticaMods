using Harmony;
using MAC.FireExtinguisherHolder.Buildable;
using MAC.FireExtinguisherHolder.Config;
using Oculus.Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using FCSCommon.Utilities;


namespace MAC.FireExtinguisherHolder
{

    public static class QPatch
    {
        internal static Configuration Configuration { get; set; }

        public static void Patch()
        {
            QuickLogger.Info("Started patching. Version: " + QuickLogger.GetAssemblyVersion());

#if DEBUG
            QuickLogger.DebugLogsEnabled = true;
            QuickLogger.Debug("Debug logs enabled");
#endif

            try
            {
                LoadConfiguration();

                FEHolderBuildable.PatchHelper();

                var harmony = HarmonyInstance.Create("com.fireextinguishergholder.MAC");

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
            Configuration = JsonConvert.DeserializeObject<Configuration>(configJson, settings);
        }
    }
}
