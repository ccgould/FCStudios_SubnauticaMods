using System.Reflection;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.DeepDriller.Buildable;
using FCS_ProductionSolutions.DeepDriller.Craftable;
using FCS_ProductionSolutions.DeepDriller.Ores;
using FCS_ProductionSolutions.HydroponicHarvester.Buildable;
using FCS_ProductionSolutions.MatterAnalyzer.Buildable;
using FCSCommon.Utilities;
using QModManager.API.ModLoading;
using SMLHelper.V2.Handlers;

namespace FCS_ProductionSolutions
{
    [QModCore]
    public class QPatch
    {
        internal static Config Configuration { get; } = OptionsPanelHandler.Main.RegisterModOptions<Config>();
        internal static DeepDrillerMk2Config DeepDrillerMk2Configuration { get; } = OptionsPanelHandler.Main.RegisterModOptions<DeepDrillerMk2Config>();
        internal static HarvesterConfig HarvesterConfiguration { get; } = OptionsPanelHandler.Main.RegisterModOptions<HarvesterConfig>();

        [QModPatch]
        public void Patch()
        {
            QuickLogger.Info($"Started patching. Version: {QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly())}");
            
            ModelPrefab.Initialize();

            AuxPatchers.AdditionalPatching();

            var hydroHarvester = new HydroponicHarvesterPatch();
            hydroHarvester.Patch();

            var matterAnalyzer = new MatterAnalyzerPatch();
            matterAnalyzer.Patch();

            var sand = new SandSpawnable();
            sand.Patch();

            var glass = new FcsGlassCraftable();
            glass.Patch();

            var deepDriller = new FCSDeepDrillerBuildable();
            deepDriller.Patch();

            //Register debug commands
            ConsoleCommandsHandler.Main.RegisterConsoleCommands(typeof(DebugCommands));

            QuickLogger.Info($"Finished Patching");


        }
    }
}
