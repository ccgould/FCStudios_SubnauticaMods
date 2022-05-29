using System;
using System.Reflection;
using CyclopsUpgradeConsole.Buildables;
using CyclopsUpgradeConsole.Configuration;
using FCSCommon.Utilities;
using QModManager.API.ModLoading;
using SMLHelper.V2.Handlers;

namespace CyclopsUpgradeConsole
{
    [QModCore]
    public class QPatch
    {
        internal static Config Configuration { get; } = OptionsPanelHandler.Main.RegisterModOptions<Config>();

        internal static string Version { get; set; }


        [QModPatch]
        public static void Patch()
        {
            Version = QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly());
            QuickLogger.Info($"Started patching. Version: {Version}");

            try
            {
                var alterraGen = new CUCBuildable();
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
