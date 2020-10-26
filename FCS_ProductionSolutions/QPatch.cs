using System.Reflection;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.HydroponicHarvester.Buildable;
using FCSCommon.Utilities;
using QModManager.API.ModLoading;
using SMLHelper.V2.Handlers;

namespace FCS_ProductionSolutions
{
    [QModCore]
    public class QPatch
    {
        internal static Config Configuration { get; } = OptionsPanelHandler.Main.RegisterModOptions<Config>();
        internal static HarvesterConfig HarvesterConfiguration { get; } = OptionsPanelHandler.Main.RegisterModOptions<HarvesterConfig>();

        [QModPatch]
        public void Patch()
        {
            QuickLogger.Info($"Started patching. Version: {QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly())}");
            
            ModelPrefab.Initialize();

            var hydroHarvester = new HydroponicHarvesterPatch();
            hydroHarvester.Patch();

            //Register debug commands
            ConsoleCommandsHandler.Main.RegisterConsoleCommands(typeof(DebugCommands));

            QuickLogger.Info($"Finished Patching");


        }
    }
}
