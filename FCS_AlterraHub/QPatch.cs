using System.Collections.Generic;
using System.Reflection;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Craftables;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono.AlterraHubDepot.Buildable;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Spawnables;
using FCS_AlterraHub.Structs;
using FCS_AlterraHub.Systems;
using FCSCommon.Utilities;
using HarmonyLib;
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
        public static TechType OreConsumerFragTechType;
        public static Config Configuration { get;} = OptionsPanelHandler.Main.RegisterModOptions<Config>();
        public static EncyclopediaConfig EncyclopediaConfig { get;} = OptionsPanelHandler.Main.RegisterModOptions<EncyclopediaConfig>();
        public static AssetBundle GlobalBundle { get; set; }

        [QModPatch]
        public static void Patch()
        {
            //QModServices.Main.AddCriticalMessage($"Power Loss Over Distance Result: {MathHelpers.PowerLossOverDistance(379)}");

            Mod.CollectKnownDevices();

            QuickLogger.Info($"Started patching. Version: {QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly())}");
            GlobalBundle = FCSAssetBundlesService.PublicAPI.GetAssetBundleByName(Mod.AssetBundleName);

            //Mod.OnGamePlaySettingsLoaded += settings =>
            //{
            //    SpawnFrag();
            //};

            Mod.LoadAudioFiles();
            
            QuickLogger.DebugLogsEnabled = Configuration.EnableDebugLogs;
            
            //Load Prefabs
            AlterraHub.GetPrefabs();
            
            //Patch all spawnables
            PatchSpawnables();

            //Patch all the buildables
            PatchBuildables();

            //Patch all Additional Store Items
            PatchAdditionalStoreItems();


            //Run Harmony Patches
            var harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), "com.alterrahub.fcstudios");

            //if (QModServices.Main.ModPresent("EasyCraft"))
            //    EasyCraft_API.Init(harmony);
            
            FCSAlterraHubService.PublicAPI.RegisterEncyclopediaEntry(EncyclopediaConfig.EncyclopediaEntries);

            //Register debug commands
            ConsoleCommandsHandler.Main.RegisterConsoleCommands(typeof(DebugCommands));
        }

        private static void PatchAdditionalStoreItems()
        {
            //Electronics
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.ReactorRod, TechType.ReactorRod, 5, 18000, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.ReactorRod);
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.AdvancedWiringKit, TechType.AdvancedWiringKit, 5, 50000, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.AdvancedWiringKit);
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.WiringKit, TechType.WiringKit, 5,50000, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.WiringKit);
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.PrecursorIonPowerCell, TechType.PrecursorIonPowerCell, 5,50000, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.PrecursorIonPowerCell);
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.PowerCell, TechType.PowerCell, 5,50000, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.PowerCell);
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.PrecursorIonBattery, TechType.PrecursorIonBattery, 5,50000, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.PrecursorIonBattery);
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.Battery, TechType.Battery, 5,10200, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.Battery);
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.CopperWire, TechType.CopperWire, 5,50000, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.CopperWire);

            //Advanced Materials
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.Polyaniline, TechType.Polyaniline, 5,98340, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.Polyaniline);
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.Aerogel, TechType.Aerogel, 5,153600, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.Aerogel);
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.AramidFibers, TechType.AramidFibers, 5,85800, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.AramidFibers);
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.Benzene, TechType.Benzene, 5,67500, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.Benzene);
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.HydrochloricAcid, TechType.HydrochloricAcid, 5,4800, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.HydrochloricAcid);
        }

        public static FCSHUD HUD { get; set; }
        public static bool IsDockedVehicleStorageAccessInstalled { get; set; }

        private static void PatchSpawnables()
        {
            ////Patch Bio Fuel
            //var bioFuelSpawnable = new BioFuelSpawnable();
            //bioFuelSpawnable.Patch();
            
            //Patch Debit Card
            var debitCardSpawnable = new DebitCardSpawnable();
            debitCardSpawnable.Patch();
            Mod.DebitCardTechType = debitCardSpawnable.TechType;

            var oreConsumerFragment = new OreConsumerFragment();
            oreConsumerFragment.Patch();
            OreConsumerFragTechType = oreConsumerFragment.TechType;
            
            var alterraHubDepotFragment = new AlterraHubDepotFragment();
            alterraHubDepotFragment.Patch();
            Mod.AlterraHubDepotFragmentTechType = alterraHubDepotFragment.TechType;

            var alterraHubDepotPatcher = new AlterraHubDepotPatcher();
            alterraHubDepotPatcher.Patch();
        }
        
        private static void PatchBuildables()
        {
            //Patch OreConsumer
            var oreConsumer = new OreConsumer();
            oreConsumer.Patch();
        }
    }
}
