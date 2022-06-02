using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Registration;
//using FCS_HomeSolutions.Mods.Replicator.Buildables;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
//using FCS_ProductionSolutions.Mods.AutoCrafter.Buildable;
using FCS_ProductionSolutions.Mods.DeepDriller.Craftable;
using FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Buildable;
using FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Ores;
using FCS_ProductionSolutions.Mods.DeepDriller.LightDuty.Buildable;
using FCS_ProductionSolutions.Mods.DeepDriller.Managers;
using FCS_ProductionSolutions.Mods.HydroponicHarvester.Buildable;
using FCS_ProductionSolutions.Mods.MatterAnalyzer.Buildable;
using FCSCommon.Utilities;
using HarmonyLib;
using QModManager.API.ModLoading;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

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
                //var hydroHarvester = new HydroponicHarvesterPatch();
                //hydroHarvester.Patch();
            }

            if (Configuration.IsReplicatorEnabled)
            {
                //var replicator = new ReplicatorBuildable();
                //replicator.Patch();
            }
            
            if (Configuration.IsReplicatorEnabled || Configuration.IsHydroponicHarvesterEnabled)
            {
                //var matterAnalyzer = new MatterAnalyzerPatch();
                //matterAnalyzer.Patch();
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

                var deepDrillerL = new DeepDrillerLightDutyBuildable();
                deepDrillerL.Patch();

                if (TechTypeHandler.ModdedTechTypeExists("Cobalt"))
                {
                    QuickLogger.Info("Trying to Add ROTA Ores and Resources");
                    if(AddROTAOre("Cobalt", out var cobalt))
                    {
                        AddROTAOre("DrillableCobalt", out var ore, new List<TechType>(){ cobalt});
                    }

                    if (AddROTAOre("Emerald", out var emerald))
                    {
                        AddROTAOre("DrillableEmerald", out var ore, new List<TechType>() { emerald });
                    }

                    if (AddROTAOre("Morganite", out var morganite))
                    {
                        AddROTAOre("DrillableMorganite", out var ore, new List<TechType>() { morganite });
                    }

                    if (AddROTAOre("RedBeryl", out var redBeryl))
                    {
                        AddROTAOre("DrillableRedBeryl", out var ore,new List<TechType>() { redBeryl });

                    }

                    if (AddROTAOre("Sapphire", out var sapphire))
                    {
                        AddROTAOre("DrillableSapphire", out var ore, new List<TechType>() { sapphire });

                    }
                }
                else
                {
                    QuickLogger.Info("Return Of The Ancients was not Found");
                }
            }

            if (Configuration.IsAutocrafterEnabled)
            {
                //var dssAutoCrafter = new AutoCrafterPatch();
                //dssAutoCrafter.Patch();
            }

            //Register debug commands
            ConsoleCommandsHandler.Main.RegisterConsoleCommands(typeof(DebugCommands));

            QuickLogger.Info($"Finished Patching");
        }

        private bool AddROTAOre(string oreName, out TechType techType, List<TechType> techTypes = null)
        {
            techType = TechType.None;
            if(TechTypeHandler.TryGetModdedTechType(oreName, out TechType ore))
            {
                if (techTypes != null&& !BiomeManager.AdditionalSpecialResources.ContainsKey(ore))
                {
                    BiomeManager.AdditionalSpecialResources.Add(ore,techTypes);
                }
                techType = ore;
                BiomeManager.Resources.Add(ore);
                QuickLogger.Info($"Added ROTA Ore/Resource {oreName}");
                return true;
            }

            return false;
        }

        public TechType Cobalt { get; set; }

        public static PingType DeepDrillerPingType { get; set; }
    }
}
