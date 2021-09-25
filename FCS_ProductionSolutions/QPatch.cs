using System;
using System.IO;
using System.Reflection;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Mods.Replicator.Buildables;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.Mods.AutoCrafter.Buildable;
using FCS_ProductionSolutions.Mods.DeepDriller.Buildable;
using FCS_ProductionSolutions.Mods.DeepDriller.Craftable;
using FCS_ProductionSolutions.Mods.DeepDriller.Ores;
using FCS_ProductionSolutions.Mods.HydroponicHarvester.Buildable;
using FCS_ProductionSolutions.Mods.MatterAnalyzer.Buildable;
using FCSCommon.Utilities;
using HarmonyLib;
using QModManager.API.ModLoading;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCS_ProductionSolutions
{
    [QModCore]
    public class QPatch
    {
        internal static Config Configuration { get; } = OptionsPanelHandler.Main.RegisterModOptions<Config>();

        [QModPatch]
        public void Patch()
        {
            FCSAlterraHubService.PublicAPI.RegisterModPack(Mod.ModPackID, Mod.ModBundleName, Assembly.GetExecutingAssembly());
            FCSAlterraHubService.PublicAPI.RegisterEncyclopediaEntry(Mod.ModPackID);
            FCSAlterraHubService.PublicAPI.OnPurge += Mod.Purge;
            ModelPrefab.Initialize();

            AuxPatchers.AdditionalPatching();
            
            //Harmony
            var harmony = new Harmony("com.productionsolutions.fcstudios");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

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

                var pingSprite = ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetFolder(), "DeepDriller_ping.png"));
                DeepDrillerPingType = WorldHelpers.CreatePingType("Deep Driller","Deep Driller",pingSprite);
                
                var deepDriller = new FCSDeepDrillerBuildable();
                deepDriller.Patch();
            }

            if (Configuration.IsAutocrafterEnabled)
            {
                var dssAutoCrafter = new AutoCrafterPatch();
                dssAutoCrafter.Patch();
            }

            //Register debug commands
            ConsoleCommandsHandler.Main.RegisterConsoleCommands(typeof(DebugCommands));

            QuickLogger.Info($"Finished Patching");
        }

        public static PingType DeepDrillerPingType { get; set; }
    }
}
