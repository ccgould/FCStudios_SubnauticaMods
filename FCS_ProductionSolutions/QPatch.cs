using System.Reflection;
using FCS_HomeSolutions.Mods.Replicator.Buildables;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.DeepDriller.Buildable;
using FCS_ProductionSolutions.DeepDriller.Craftable;
using FCS_ProductionSolutions.DeepDriller.Ores;
using FCS_ProductionSolutions.HydroponicHarvester.Buildable;
using FCS_ProductionSolutions.MatterAnalyzer.Buildable;
using FCS_StorageSolutions.Mods.DataStorageSolutions.Buildable;
using FCSCommon.Utilities;
using HarmonyLib;
using QModManager.API.ModLoading;
using SMLHelper.V2.Handlers;

namespace FCS_ProductionSolutions
{
    [QModCore]
    public class QPatch
    {
        internal static Config Configuration { get; } = OptionsPanelHandler.Main.RegisterModOptions<Config>();

        [QModPatch]
        public void Patch()
        {
            QuickLogger.Info($"Started patching. Version: {QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly())}");
            
            ModelPrefab.Initialize();

            AuxPatchers.AdditionalPatching();

            if (Configuration.IsHydroponicHarvesterEnabled)
            {
                var hydroHarvester = new HydroponicHarvesterPatch();
                hydroHarvester.Patch();
            }

            if (Configuration.IsReplicatorEnabled)
            {
                var replicator = new ReplicatorBuildable();
                replicator.Patch();
            }
            
            if (Configuration.IsReplicatorEnabled || Configuration.IsHydroponicHarvesterEnabled)
            {
                var matterAnalyzer = new MatterAnalyzerPatch();
                matterAnalyzer.Patch();
            }

            if (Configuration.IsDeepDrillerEnabled)
            {
                var sand = new SandSpawnable();
                sand.Patch();

                var glass = new FcsGlassCraftable();
                glass.Patch();

                var deepDriller = new FCSDeepDrillerBuildable();
                deepDriller.Patch();
            }

            if (Configuration.IsAutocrafterEnabled)
            {
                var dssAutoCrafter = new DSSAutoCrafterPatch();
                dssAutoCrafter.Patch();
            }

            //Register debug commands
            ConsoleCommandsHandler.Main.RegisterConsoleCommands(typeof(DebugCommands));

            //Harmony
            var harmony = new Harmony("com.productionsolutions.fcstudios");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            QuickLogger.Info($"Finished Patching");
        }
    }
}
