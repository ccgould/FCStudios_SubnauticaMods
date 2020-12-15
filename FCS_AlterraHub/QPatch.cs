using System.Reflection;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Craftables;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Patch;
using FCS_AlterraHub.Patches;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Spawnables;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using HarmonyLib;
using QModManager.API;
using QModManager.API.ModLoading;
using SMLHelper.V2.Handlers;
using UnityEngine;

namespace FCS_AlterraHub
{
    /*
     * AlterraHub is a mod that allows you to purchase or craft FCStudios object in the Subnautica world.
     * This mod acts as a hub for the base and also allows other mods to connect to one another
     */
    [QModCore]
    public class QPatch
    {
        internal static Config Configuration { get;} = OptionsPanelHandler.Main.RegisterModOptions<Config>();
        public static AssetBundle GlobalBundle { get; set; }

        [QModPatch]
        public static void Patch()
        {
            QuickLogger.Info($"Started patching. Version: {QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly())}");
            GlobalBundle = FCSAssetBundlesService.PublicAPI.GetAssetBundleByName(Mod.AssetBundleName);
            
            QuickLogger.DebugLogsEnabled = Configuration.EnableDebugLogs;
            //Load Prefabs
            AlterraHub.GetPrefabs();
            
            //Patch all the buildables
            PatchBuildables();
            
            //Patch all spawnables
            PatchSpawnables();

            //CreatKitEntries
            CreateKits();

            //Run Harmony Patches
            var harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), "com.alterrahub.fcstudios");

            if (QModServices.Main.ModPresent("EasyCraft"))
                EasyCraft_API.Init(harmony);

            //Register debug commands
            ConsoleCommandsHandler.Main.RegisterConsoleCommands(typeof(DebugCommands));
        }

        public static FCSHUD HUD { get; set; }

        private static void CreateKits()
        {
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(Mod.OreConsumerTechType, Mod.OreConsumerKitClassID.ToTechType(), 30000, StoreCategory.Production);
        }

        private static void PatchSpawnables()
        {
            //Patch Bio Fuel
            var bioFuelSpawnable = new BioFuelSpawnable();
            bioFuelSpawnable.Patch();

            //Patch Debit Card
            var debitCardSpawnable = new DebitCardSpawnable();
            debitCardSpawnable.Patch();
            Mod.DebitCardTechType = debitCardSpawnable.TechType;
        }

        private static void PatchBuildables()
        {

            //Patch AlterraHub
            var alterraHub = new AlterraHub();
            alterraHub.Patch();

            //Patch OreConsumer
            var oreConsumer = new OreConsumer();
            oreConsumer.Patch();

            //PDATabPatcher.AddPDATab("FSColor");

        }
    }
}
