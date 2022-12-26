using System;
using System.IO;
using System.Reflection;
using BepInEx;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.AlterraHubConstructor.Buildable;
using FCS_AlterraHub.Mods.AlterraHubDepot.Buildable;
using FCS_AlterraHub.Mods.AlterraHubDepot.Spawnable;
using FCS_AlterraHub.Mods.AlterraHubPod.Spawnable;
using FCS_AlterraHub.Mods.Common.DroneSystem;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Mods.OreConsumer.Buildable;
using FCS_AlterraHub.Mods.PatreonStatue.Buildable;
using FCS_AlterraHub.Patches;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Systems;
using FCSCommon.Utilities;
using FMOD;
using HarmonyLib;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCS_AlterraHub
{
    /*
     * AlterraHub is a mod that allows you to purchase or craft FCStudios object in the Subnautica world.
     * This mod acts as a hub for the base and also allows other mods to connect to one another
     */
    [BepInPlugin(GUID,MODNAME, VERSION)]
    public class Main : BaseUnityPlugin
    {
        #region [Declarations]

        public const string
            MODNAME = "AlterraHub",
            AUTHOR = "FieldCreatorsStudios",
            GUID = AUTHOR + "_" + MODNAME,
            VERSION = "1.0.0.0";

        internal static FMODAsset BoxOpenSoundAsset;
        private static string PdaEntryMessage => $"Please open your AlterraHub PDA to read this data entry ({Configuration.FCSPDAKeyCode}). Make sure you have completed the Alterra Hub Station mission to do so.";
        public static Config Configuration { get; } = OptionsPanelHandler.Main.RegisterModOptions<Config>();
        public static AssetBundle GlobalBundle { get; set; }

        #endregion



        private void Awake()
        {
            QuickLogger.Info($"Started patching. Version: {QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly())}");

            FCSAlterraHubService.PublicAPI.RegisterModPack(Mod.ModPackID, Mod.AssetBundleName, Assembly.GetExecutingAssembly());
            FCSAlterraHubService.PublicAPI.RegisterEncyclopediaEntry(Mod.ModPackID);

            LanguageHandler.SetLanguageLine($"Ency_{Mod.AlterraHubDepotClassID}", Mod.AlterraHubDepotFriendly);
            LanguageHandler.SetLanguageLine($"EncyDesc_{Mod.AlterraHubDepotClassID}", PdaEntryMessage);
            LanguageHandler.SetLanguageLine($"Ency_{OreConsumerPatcher.OreConsumerClassID}", OreConsumerPatcher.OreConsumerFriendly);
            LanguageHandler.SetLanguageLine($"EncyDesc_{OreConsumerPatcher.OreConsumerClassID}", PdaEntryMessage);


           Mod.CollectKnownDevices();

            

            GlobalBundle = FCSAssetBundlesService.PublicAPI.GetAssetBundleByName(Mod.AssetBundleName);
            //Mod.LoadAudioFiles();


            //QuickLogger.InfoLogsEnabled = Configuration.EnableInfoLogs;
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
            CreatePingType(harmony);
            //BoosterSound = AudioUtils.CreateSound(Path.Combine(Mod.GetAssetPath(), "Audio", "booster.mp3"));
            FCSAlterraHubService.PublicAPI.FcsUnlockTechType = TechTypeHandler.Main.AddTechType("FCSUnlocker", "FCS Access Key", "TechType to unlock FCSItems", false);
            RegisterCommands();
            AlterraHub.AdditionalPatching();
            Mod.RegisterVoices();

            QuickLogger.Info($"Finished patching. Version: {QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly())}");

        }

        private static void RegisterCommands()
        {
            //Register Info commands
            ConsoleCommandsHandler.Main.RegisterConsoleCommands(typeof(DebugCommands));
        }

        private static void CreatePingType(Harmony harmony)
        {
            //Create AlterraHubStation PingType
            var type = Type.GetType("SubnauticaMap.PingMapIcon, SubnauticaMap", false, false);
            if (type != null)
            {
                var pingOriginal = AccessTools.Method(type, "Refresh");
                var pingPrefix = new HarmonyMethod(AccessTools.Method(typeof(PingMapIcon_Patch), "Prefix"));
                harmony.Patch(pingOriginal, pingPrefix);
            }

            Mod.AlterraHubStationPingType = WorldHelpers.CreatePingType("Station", "Station",
                ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetPath(), "AlterraHubPing.png")));

            Mod.AlterraTransportDronePingType = WorldHelpers.CreatePingType("AlterraTransportDrone",
                "AlterraTransportDrone", ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetPath(), "TransportDronePing.png")));


            var pingMapIconType = Type.GetType("SubnauticaMap.PingMapIcon, SubnauticaMap", false, false);
            if (pingMapIconType != null)
            {
                var pingOriginal = AccessTools.Method(pingMapIconType, "Refresh");
                var pingPrefix = new HarmonyMethod(AccessTools.Method(typeof(PingMapIcon_Patch), "Prefix"));
                harmony.Patch(pingOriginal, pingPrefix);
            }
        }

        private static void PatchAdditionalStoreItems()
        {
            //Electronics
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.ReactorRod, TechType.ReactorRod, 5, 445000, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.ReactorRod);
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.AdvancedWiringKit, TechType.AdvancedWiringKit, 5, 372500, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.AdvancedWiringKit);
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.WiringKit, TechType.WiringKit, 5, 115000, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.WiringKit);
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.PrecursorIonPowerCell, TechType.PrecursorIonPowerCell, 5, 425000, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.PrecursorIonPowerCell);
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.PowerCell, TechType.PowerCell, 5, 35000, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.PowerCell);
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.PrecursorIonBattery, TechType.PrecursorIonBattery, 5, 210000, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.PrecursorIonBattery);
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.Battery, TechType.Battery, 5, 16250, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.Battery);
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.CopperWire, TechType.CopperWire, 5, 23750, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.CopperWire);

            //Advanced Materials
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.Polyaniline, TechType.Polyaniline, 5, 88750, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.Polyaniline);
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.Aerogel, TechType.Aerogel, 5, 133000, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.Aerogel);
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.AramidFibers, TechType.AramidFibers, 5, 95000, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.AramidFibers);
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.Benzene, TechType.Benzene, 5, 75000, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.Benzene);
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.HydrochloricAcid, TechType.HydrochloricAcid, 5, 10000, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.HydrochloricAcid);
        }

        private static void PatchSpawnables()
        {
            //Data Box
            var databox = new FCSDataBoxSpawnable();
            databox.Patch();

            //Patch Debit Card
            var debitCardSpawnable = new DebitCardSpawnable();
            debitCardSpawnable.Patch();
            Mod.DebitCardTechType = debitCardSpawnable.TechType;

            var transportDrone = new AlterraTransportDroneSpawnable();
            transportDrone.Patch();

            BoxOpenSoundAsset = FModHelpers.CreateFmodAsset("box_open", "event:/loot/databox/box_open");

            var sropPod = new AlterraHubPatch();
            sropPod.Patch();
        }

        private static void PatchBuildables()
        {
            //Patch OreConsumer
            var oreConsumer = new OreConsumerPatcher();
            oreConsumer.Patch();

            var alterraHubDepotPatcher = new AlterraHubDepotPatcher();
            alterraHubDepotPatcher.Patch();

            var dronePortBuilable = new DronePortPadHubNewPatcher();
            dronePortBuilable.Patch();

            var alterraHubConstructor = new AlterraHubFabricatorPatcher();
            alterraHubConstructor.Patch();

            var patreonStatueBuilable = new PatreonStatuePatcher();
            patreonStatueBuilable.Patch();
        }
    }
}
