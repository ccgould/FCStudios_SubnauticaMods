using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Objects;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Buildables.OutDoorPlanters;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.AlienChief.Buildables;
using FCS_HomeSolutions.Mods.Cabinets.Buildable;
using FCS_HomeSolutions.Mods.Curtains.Buildable;
using FCS_HomeSolutions.Mods.Elevator.Buildable;
using FCS_HomeSolutions.Mods.FireExtinguisherRefueler.Buildable;
using FCS_HomeSolutions.Mods.HologramPoster.Buildable;
using FCS_HomeSolutions.Mods.JukeBox.Buildable;
using FCS_HomeSolutions.Mods.LedLights.Buildable;
using FCS_HomeSolutions.Mods.MiniFountainFilter.Buildables;
using FCS_HomeSolutions.Mods.PaintTool.Spawnable;
using FCS_HomeSolutions.Mods.PeeperLoungeBar.Buildable;
using FCS_HomeSolutions.Mods.QuantumTeleporter.Buildable;
using FCS_HomeSolutions.Mods.SeaBreeze.Buildable;
using FCS_HomeSolutions.Mods.TrashReceptacle.Buildable;
using FCS_HomeSolutions.Mods.TrashRecycler.Buildable;
using FCS_HomeSolutions.Mods.TV.Buildable;
using FCS_HomeSolutions.Spawnables;
using FCSCommon.Utilities;
using FMOD;
using HarmonyLib;
using QModManager.API.ModLoading;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;
using Settings = FCS_HomeSolutions.Buildables.Settings;

namespace FCS_HomeSolutions
{
    /*
     * Alterra Home Solutions mod pack adds objects to subnautica that deals with bases and decorations
    */

    [QModCore]
    public class QPatch
    {
        internal static Config Configuration { get; } = OptionsPanelHandler.Main.RegisterModOptions<Config>();

        internal static Dictionary<string, Texture2D> Patterns = new();

        internal static Dictionary<Texture2D, Atlas.Sprite> PatternsIcon = new();

        public static Dictionary<string, SoundEntry> AudioClips = new();

        [QModPatch]
        public void Patch()
        {
            FCSAlterraHubService.PublicAPI.RegisterModPack(Mod.ModPackID,Mod.ModBundleName, Assembly.GetExecutingAssembly());
            FCSAlterraHubService.PublicAPI.RegisterEncyclopediaEntry(Mod.ModPackID);

            ModelPrefab.Initialize();

            AuxPatchers.AdditionalPatching();
            
            //Load Additional Colors
            LoadAdditionalColors();

            if (Configuration.IsPeeperLoungeBarEnabled)
            {
                var ahsSweetWaterBar = new PeeperLoungeBarPatch("ahsSweetWaterBar", "Peeper Lounge Bar",
                    "All drinks on the house.", ModelPrefab.GetPrefab("PeeperLoungeBar",true), new Settings
                    {
                        KitClassID = "ahsSweetWaterBar_kit",
                        AllowedInBase = true,
                        AllowedOutside = true,
                        AllowedOnGround = true,
                        RotationEnabled = true,
                        Cost = 45000,
                        Size = new Vector3(2.020296f, 1.926336f, 2.067809f),
                        Center = new Vector3(0, 1.5901f, 0),
                        CategoryForPDA = TechCategory.Misc,
                        GroupForPDA = TechGroup.Miscellaneous,
                        IconName = "PeeperLoungeBar"
                    });
                ahsSweetWaterBar.Patch();

                LoadPeeperLoungeTracks();

                PatchFood();
            }

            if (Configuration.IsSmartOutDoorPlanterEnabled)
            {
                //Patch Smart Planter Pot
                var smartOutDoorPlanter = new OutDoorPlanterPatch(Mod.SmartPlanterPotClassID, Mod.SmartPlanterPotFriendly,
                    Mod.SmartPlanterPotDescription, ModelPrefab.SmallOutdoorPot, new Settings
                    {
                        KitClassID = Mod.SmartPlanterPotKitClassID,
                        Size = new Vector3(0.7929468f, 0.3463891f, 0.7625999f),
                        Center = new Vector3(0f, 0.2503334f, 0f)
                    });

                smartOutDoorPlanter.Patch();
            }

            if (Configuration.IsMiniFountainFilterEnabled)
            {
                //Patch Mini Fountain Filter
                var miniFountainFilter = new MiniFountainFilterBuildable();
                miniFountainFilter.Patch();
            }

            if (Configuration.IsSeaBreezeEnabled)
            {
                //Patch SeaBreeze
                var seaBreeze = new SeaBreezeBuildable();
                seaBreeze.Patch();
            }

            if (Configuration.IsPaintToolEnabled)
            {
                //Patch Paint Tool
                var paintToolSpawnable = new PaintToolSpawnable();
                paintToolSpawnable.Patch();

                //Patch Paint Can
                var paintCan = new PaintCanSpawnable();
                paintCan.Patch();
            }

            if (Configuration.IsFireExtinguisherRefuelerEnabled)
            {
                //Fire Extinguisher Refueler
                var fireExtinguisherRefueler = new FireExtinguisherRefuelerBuildable();
                fireExtinguisherRefueler.Patch();
            }

            if (Configuration.IsTrashReceptacleEnabled)
            {
                //Patch Trash Receptacle
                var trashReceptacle = new TrashReceptaclePatch();
                trashReceptacle.Patch();
            }

            if (Configuration.IsTrashRecyclerEnabled)
            {
                //Patch Trash Recycler
                var trashRecycler = new TrashRecyclerPatch();
                trashRecycler.Patch();
            }
            
            if (Configuration.IsCurtainEnabled)
            {
                //Patch Curtain
                var curtain = new CurtainPatch();
                curtain.Patch();
            }

            if (Configuration.IsQuantumTeleporterEnabled)
            {
                //Patch Quantum Teleporter
                var quantumTeleporter = new QuantumTeleporterBuildable();
                quantumTeleporter.Patch();
            }

            if (Configuration.IsAlienChiefEnabled)
            {
                //Patch Alien Chief
                var alienChief = new AlienChefBuildable();
                alienChief.Patch();
            }

            if (Configuration.IsObservationTankEnabled)
            {
                var observationTank = new ObservationTankBuildable();
                observationTank.Patch();
            }
            
            if (Configuration.IsAlterraMiniBathroomEnabled)
            {
                //Patch Alterra Mini Bathroom
                var alterraMiniBathroom = new AlterraMiniBathroomBuildable();
                alterraMiniBathroom.Patch();
            }

            if (Configuration.IsElevatorEnabled)
            {
                //Patch Elevator
                var elevator = new ElevatorBuildable();
                elevator.Patch();
                AudioUtils.CreateSound(Path.Combine(Mod.GetAssetPath(), "Audio", "LiftSoundEffect.mp3"));
            }

            var jukeBox = new JukeBoxBuildable();
            jukeBox.Patch();

            var jukeboxSpeaker = new JukeBoxSpeakerBuildable();
            jukeboxSpeaker.Patch();


            var hologramPoster = new HologramPosterBuildable();
            hologramPoster.Patch();

            PatchSigns();

            PatchSmartTvs();

            PatchRailings();

            PatchShelvesAndTables();

            PatchLights();

            LoadCurtainTemplates();

            PatchCabinets();
            
            var harmony = new Harmony("com.homesolutions.fstudios");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            
            //Register debug commands
            ConsoleCommandsHandler.Main.RegisterConsoleCommands(typeof(DebugCommands));
        }

        private void LoadPeeperLoungeTracks()
        {
            AudioClips.Add("PLB_AnnoyingFish", new SoundEntry
            {
                Sound = AudioUtils.CreateSound(Path.Combine(Mod.GetAssetPath(), "Audio", "PeeperLoungeBar_AnnoyingFish.mp3")),
                Message = "These fish are so annoying. Would you mind taking them out?"
            });
            AudioClips.Add("PLB_Hello", new SoundEntry
            {
                Sound = AudioUtils.CreateSound(Path.Combine(Mod.GetAssetPath(), "Audio", "PeeperLoungeBar_Hello.mp3")),
                Message = "Hello Ryley! would you like to purchase something?"
            });
            AudioClips.Add("PLB_ThankYou", new SoundEntry
            {
                Sound = AudioUtils.CreateSound(Path.Combine(Mod.GetAssetPath(), "Audio", "PeeperLoungeBar_ThankYou.mp3")),
                Message = "Thank You! Please come back again!"
            });            
            AudioClips.Add("PLB_Intro", new SoundEntry
            {
                Sound = AudioUtils.CreateSound(Path.Combine(Mod.GetAssetPath(), "Audio", "PeeperLoungeBar_Intro.mp3")),
                Message = "Hello, my name is Peeper Lounge, nice to meet you. My function is to make your leisure time fun and efficient with a great selection of drinks and snacks from the Alterra Corporation. I am able to speak to you through your FCS PDA, isn't that great? However, if you prefer, you can disable my voice in the settings for FCS products."
            });
            AudioClips.Add("PLB_FishRemoved", new SoundEntry
            {
                Sound = AudioUtils.CreateSound(Path.Combine(Mod.GetAssetPath(), "Audio", "PeeperLoungeBar_FishRemoved.mp3")),
                Message = "Thank you for taking those pesky fish out!"
            });            
            AudioClips.Add("PLB_NoCardDetected", new SoundEntry
            {
                Sound = AudioUtils.CreateSound(Path.Combine(Mod.GetAssetPath(), "Audio", "PeeperLoungeBar_NoCardDetected.mp3")),
                Message = "I maybe blind since I am just a robot, but I can't seem to locate your debit card on your body anywhere!"
            });
            AudioClips.Add("PLB_NotEnoughCredit", new SoundEntry
            {
                Sound = AudioUtils.CreateSound(Path.Combine(Mod.GetAssetPath(), "Audio", "PeeperLoungeBar_NotEnoughCredit.mp3")),
                Message = "TI'm sorry, but it seems you do not have enough credit for this purchase."
            });
        }
        
        private void LoadAdditionalColors()
        {
            foreach (AdditionalColor additionalColor in Configuration.PaintToolAdditionalPaintColors)
            {
                ColorList.AddColor(additionalColor.Color,additionalColor.ColorName);
            }
        }

        private void PatchLights()
        {
            if(!Configuration.IsLEDLightsEnabled) return;

            var longLight = new LedLightPatch(new LedLightData
            {
                classId = "LedLightStickLong",
                description = "A long LED light stick, suitable for interior and exterior use. (Change the color with the Paint Tool)",
                friendlyName = "Long Led Light Stick",
                allowedInBase = true,
                allowedInSub = false,
                allowedOnGround = true,
                allowedOnWall = false,
                allowedOutside = true,
                categoryForPDA = TechCategory.Misc,
                groupForPda = TechGroup.Miscellaneous,
                size = Vector3.zero,
                center = Vector3.zero,
                prefab = ModelPrefab.LedLightLongPrefab,
                TechData = Mod.LedLightStickLongIngredients,
            });
            longLight.Patch();

            var shortLight = new LedLightPatch(new LedLightData
            {
                classId = "LedLightStickShort",
                description = "A short LED light stick, suitable for interior and exterior use. (Change the color with the Paint Tool)",
                friendlyName = "Short Led Light Stick",
                allowedInBase = true,
                allowedInSub = true,
                allowedOnGround = true,
                allowedOnWall = false,
                allowedOutside = true,
                categoryForPDA = TechCategory.Misc,
                groupForPda = TechGroup.Miscellaneous,
                size = Vector3.zero,
                center = Vector3.zero,
                prefab = ModelPrefab.LedLightShortPrefab,
                TechData = Mod.LedLightStickShortIngredients
            });
            shortLight.Patch();

            var wallLight = new LedLightPatch(new LedLightData
            {
                classId = "LedLightStickWall",
                description = "A wall mountable LED light strip. (Change the color with the Paint Tool) (Interior use only)",
                friendlyName = "Wall Mountable Led Light Strip",
                allowedInBase = true,
                allowedInSub = true,
                allowedOnGround = false,
                allowedOnWall = true,
                allowedOutside = false,
                categoryForPDA = TechCategory.InteriorModule,
                groupForPda = TechGroup.InteriorModules,
                size = Vector3.zero,
                center = Vector3.zero,
                prefab = ModelPrefab.LedLightWallPrefab,
                TechData = Mod.LedLightStickWallIngredients
            });
            wallLight.Patch();
        }

        private void PatchShelvesAndTables()
        {
            if (!Configuration.IsShelvesAndTablesEnabled) return;

            var floorShelf01 = new DecorationEntryPatch("floorShelf01", "Floor Shelf 1", "Don’t put your things on the floor, put them on this lovely shelf!",
                ModelPrefab.GetPrefab("FloorShelf01"),
                new Settings
                {
                    KitClassID = "floorShelf01_kit",
                    AllowedInBase = true,
                    AllowedOutside = false,
                    AllowedOnGround = true,
                    AllowedInSub = true,
                    AllowedOnConstructables = true,
                    RotationEnabled = true,
                    Cost = 4500,
                    Center = new Vector3(-0.006723166f, 0.4907906f, 0f),
                    Size = new Vector3(0.6996989f, 0.8610544f, 0.6298063f),
                    CategoryForPDA = TechCategory.InteriorModule,
                    GroupForPDA = TechGroup.InteriorModules
                });
            floorShelf01.Patch();

            var floorShelf02 = new DecorationEntryPatch("floorShelf02", "Floor Shelf 2", "Don’t put your things on the floor, put them on this lovely shelf!",
                ModelPrefab.GetPrefab("FloorShelf02"),
                new Settings
                {
                    KitClassID = "floorShelf02_kit",
                    AllowedInBase = true,
                    AllowedOutside = false,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    AllowedInSub = true,
                    Cost = 4500,
                    Center = new Vector3(0f, 1.405873f, 0f),
                    Size = new Vector3(0.6692477f, 2.594568f, 0.6355314f),
                    CategoryForPDA = TechCategory.InteriorModule,
                    GroupForPDA = TechGroup.InteriorModules
                });
            floorShelf02.Patch();

            var floorShelf03 = new DecorationEntryPatch("floorShelf03", "Floor Shelf 3", "Don’t put your things on the floor, put them on this lovely shelf!",
                ModelPrefab.GetPrefab("FloorShelf03"),
                new Settings
                {
                    KitClassID = "floorShelf03_kit",
                    AllowedInBase = true,
                    AllowedOutside = false,
                    AllowedOnGround = true,
                    AllowedInSub = true,
                    AllowedOnConstructables = true,
                    RotationEnabled = true,
                    Cost = 4500,
                    Size = new Vector3(0.6614718f, 1.675184f, 0.6331196f),
                    Center = new Vector3(0f, 0.9526114f, 0f),
                    CategoryForPDA = TechCategory.InteriorModule,
                    GroupForPDA = TechGroup.InteriorModules
                });
            floorShelf03.Patch();


            var floorShelf04 = new DecorationEntryPatch("floorShelf04", "Floor Shelf 4", "Don’t put your things on the floor, put them on this lovely shelf!",
                ModelPrefab.GetPrefab("FloorShelf04"),
                new Settings
                {
                    KitClassID = "floorShelf04_kit",
                    AllowedInBase = true,
                    AllowedOutside = false,
                    AllowedOnGround = true,
                    AllowedInSub = true,
                    AllowedOnConstructables = true,
                    RotationEnabled = true,
                    Cost = 4500,
                    Size = new Vector3(2.743652f, 1.61677f, 0.6635007f),
                    Center = new Vector3(-0.1762199f, 0.8602326f, 0f),
                    CategoryForPDA = TechCategory.InteriorModule,
                    GroupForPDA = TechGroup.InteriorModules
                });
            floorShelf04.Patch();

            var floorShelf05 = new DecorationEntryPatch("floorShelf05", "Floor Shelf 5", "Don’t put your things on the floor, put them on this lovely shelf!",
                ModelPrefab.GetPrefab("FloorShelf05"),
                new Settings
                {
                    KitClassID = "floorShelf05_kit",
                    AllowedInBase = true,
                    AllowedOutside = false,
                    AllowedOnGround = true,
                    AllowedInSub = true,
                    AllowedOnConstructables = true,
                    RotationEnabled = true,
                    Cost = 4500,
                    Size = new Vector3(2.772398f, 1.636121f, 0.6347024f),
                    Center = new Vector3(-0.1675282f, 0.86191998f, 0f),
                    CategoryForPDA = TechCategory.InteriorModule,
                    GroupForPDA = TechGroup.InteriorModules
                });
            floorShelf05.Patch();

            var floorShelf06 = new DecorationEntryPatch("floorShelf06", "Floor Shelf 6", "Don’t put your things on the floor, put them on this lovely shelf!",
                ModelPrefab.GetPrefab("FloorShelf06"),
                new Settings
                {
                    KitClassID = "floorShelf06_kit",
                    AllowedInBase = true,
                    AllowedOutside = false,
                    AllowedOnGround = true,
                    AllowedOnConstructables = true,
                    AllowedInSub = true,
                    RotationEnabled = true,
                    Cost = 4500,
                    Size = new Vector3(2.759224f, 0.8717237f, 0.647239f),
                    Center = new Vector3(-0.1740894f, 0.4856191f, 0.002290621f),
                    CategoryForPDA = TechCategory.InteriorModule,
                    GroupForPDA = TechGroup.InteriorModules
                });
            floorShelf06.Patch();


            var floorShelf07 = new DecorationEntryPatch("floorShelf07", "Floor Shelf 7", "Don’t put your things on the floor, put them on this lovely shelf!",
                ModelPrefab.GetPrefab("FloorShelf07"),
                new Settings
                {
                    KitClassID = "floorShelf07_kit",
                    AllowedInBase = true,
                    AllowedOutside = false,
                    AllowedOnGround = true,
                    AllowedInSub = true,
                    AllowedOnConstructables = true,
                    RotationEnabled = true,
                    Cost = 4500,
                    Size = new Vector3(-0.1693296f, 0.4921141f, 0f),
                    Center = new Vector3(2.764177f, 0.8583584f, 0.6468056f),
                    CategoryForPDA = TechCategory.InteriorModule,
                    GroupForPDA = TechGroup.InteriorModules
                });
            floorShelf07.Patch();

            var neonShelf01 = new DecorationEntryPatch("neonShelf01", "Neon Shelf 01", "A shelf with neon lights. (Paint Tool Recommended)",
                ModelPrefab.GetPrefab("NeonShelf01"),
                new Settings
                {
                    KitClassID = "neonShelf01_kit",
                    AllowedInBase = true,
                    AllowedOutside = false,
                    AllowedOnGround = false,
                    AllowedInSub = true,
                    AllowedOnWall = true,
                    AllowedOnConstructables = true,
                    RotationEnabled = false,
                    Cost = 27000,
                    Size = new Vector3(1.997957f, 0.06401221f, 0.9870584f),
                    Center = new Vector3(-2.488494e-05f, -0.01308646f, 0.5065421f),
                    CategoryForPDA = TechCategory.InteriorModule,
                    GroupForPDA = TechGroup.InteriorModules
                });
            neonShelf01.Patch();


            var neonTable01 = new DecorationEntryPatch("neonTable01", "Neon Table 01", "A table with neon lights. (Paint Tool Recommended)",
                ModelPrefab.GetPrefab("NeonTable01"),
                new Settings
                {
                    KitClassID = "neonTable01_kit",
                    AllowedInBase = true,
                    AllowedOutside = false,
                    AllowedOnGround = true,
                    AllowedInSub = true,
                    AllowedOnConstructables = true,
                    RotationEnabled = true,
                    Cost = 45000,
                    Size = new Vector3(1.997957f, 0.8685947f, 2.000143f),
                    Center = new Vector3(-2.488494e-05f, 0.5808856f, 0f),
                    CategoryForPDA = TechCategory.InteriorModule,
                    GroupForPDA = TechGroup.InteriorModules
                });
            neonTable01.Patch();

            var neonTable02 = new DecorationEntryPatch("neonTable02", "Neon Table 02", "A table with neon lights. (Paint Tool Recommended)",
                ModelPrefab.GetPrefab("NeonTable02"),
                new Settings
                {
                    KitClassID = "neonTable02_kit",
                    AllowedInBase = true,
                    AllowedOutside = false,
                    AllowedOnGround = true,
                    AllowedInSub = true,
                    AllowedOnConstructables = true,
                    RotationEnabled = true,
                    Cost = 45000,
                    Size = new Vector3(1.997957f, 0.8685947f, 2.000143f),
                    Center = new Vector3(-2.488494e-05f, 0.5808856f, 0f),
                    CategoryForPDA = TechCategory.InteriorModule,
                    GroupForPDA = TechGroup.InteriorModules
                });
            neonTable02.Patch();
        }

        private static void PatchRailings()
        {
            if (!Configuration.IsRailingsEnabled) return;

            var ahsrailing = new DecorationEntryPatch("ahsrailing", "Railing", "A railing to create a barrior",
                ModelPrefab.GetPrefab("Railing_Normal"),
                new Settings
                {
                    KitClassID = "ahsrailing_kit",
                    AllowedInBase = true,
                    AllowedOutside = true,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    Cost = 20000,
                    Size = new Vector3(1.963638f, 1.020765f, 0.1433573f),
                    Center = new Vector3(0f, 0.6343491f, 0f),
                    CategoryForPDA = TechCategory.Misc,
                    GroupForPDA = TechGroup.Miscellaneous
                });
            ahsrailing.Patch();

            var ahsrailingglass = new DecorationEntryPatch("ahsrailingglass", "Railing With Glass",
                "A railing to create a barrior",
                ModelPrefab.GetPrefab("Railing_Normal_wGlass"),
                new Settings
                {
                    KitClassID = "ahsrailingglass_kit",
                    AllowedInBase = true,
                    AllowedOutside = true,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    Cost = 20000,
                    Size = new Vector3(1.963638f, 1.020765f, 0.1433573f),
                    Center = new Vector3(0f, 0.6343491f, 0f),
                    CategoryForPDA = TechCategory.Misc,
                    GroupForPDA = TechGroup.Miscellaneous
                });
            ahsrailingglass.Patch();

            var ahsLeftCornerRail = new DecorationEntryPatch("ahsLeftCornerRailing", "Railing Left Corner",
                "A railing to create a barrior",
                ModelPrefab.GetPrefab("Railing_LeftCorner"),
                new Settings
                {
                    KitClassID = "ahsleftcornerrailing_kit",
                    AllowedInBase = true,
                    AllowedOutside = true,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    Cost = 15000,
                    Size = new Vector3(1f, 0.822893f, 0.1791649f),
                    Center = new Vector3(-1.365597e-25f, 0.7452481f, -0.004088677f),
                    CategoryForPDA = TechCategory.Misc,
                    GroupForPDA = TechGroup.Miscellaneous
                });
            ahsLeftCornerRail.Patch();

            var ahsLeftCornerwGlassRail = new DecorationEntryPatch("ahsLeftCornerwGlassRailing", "Railing Left Corner wGlass",
                "A railing to create a barrior",
                ModelPrefab.GetPrefab("Railing_LeftCorner_wGlass"),
                new Settings
                {
                    KitClassID = "ahsleftcornerwGlassrailing_kit",
                    AllowedInBase = true,
                    AllowedOutside = true,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    Cost = 15000,
                    Size = new Vector3(1f, 0.822893f, 0.1791649f),
                    Center = new Vector3(-1.365597e-25f, 0.7452481f, -0.004088677f),
                    CategoryForPDA = TechCategory.Misc,
                    GroupForPDA = TechGroup.Miscellaneous
                });
            ahsLeftCornerwGlassRail.Patch();

            var ahsRightCornerRail = new DecorationEntryPatch("ahsRightCornerRailing", "Railing Right Corner",
                "A railing to create a barrior",
                ModelPrefab.GetPrefab("Railing_RightCorner"),
                new Settings
                {
                    KitClassID = "ahsrightcornerrailing_kit",
                    AllowedInBase = true,
                    AllowedOutside = true,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    Cost = 15000,
                    Size = new Vector3(1f, 0.822893f, 0.1791649f),
                    Center = new Vector3(-1.365597e-25f, 0.7452481f, -0.004088677f),
                    CategoryForPDA = TechCategory.Misc,
                    GroupForPDA = TechGroup.Miscellaneous
                });
            ahsRightCornerRail.Patch();


            var ahsRightCornerwGlassRail = new DecorationEntryPatch("ahsRightCornerwGlassRailing", "Railing Right Corner wGlass",
                "A railing to create a barrior",
                ModelPrefab.GetPrefab("Railing_RightCorner_wGlass"),
                new Settings
                {
                    KitClassID = "ahsrightcornerwGlassrailing_kit",
                    AllowedInBase = true,
                    AllowedOutside = true,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    Cost = 15000,
                    Size = new Vector3(1f, 0.822893f, 0.1791649f),
                    Center = new Vector3(-1.365597e-25f, 0.7452481f, -0.004088677f),
                    CategoryForPDA = TechCategory.Misc,
                    GroupForPDA = TechGroup.Miscellaneous
                });
            ahsRightCornerwGlassRail.Patch();
        }

        private static void PatchSmartTvs()
        {
            if (!Configuration.IsSmartTelevisionEnabled) return;

            var tableSmartTV = new TVPatch("tableSmartTV", "Smart TV With Table Mount",
                "Take a little break and watch some TV. Includes Table Mount.",
                ModelPrefab.GetPrefab("TableSmartTV"),
                new Settings
                {
                    KitClassID = "tableSmartTV_kit",
                    AllowedInBase = true,
                    AllowedOutside = false,
                    AllowedOnGround = true,
                    AllowedOnConstructables = true,
                    RotationEnabled = true,
                    Cost = 78750,
                    Size = new Vector3(1.820033f, 1.101903f, 0.08594985f),
                    Center = new Vector3(-0.003967494f, 0.5916209f, 0.001384925f)
                });
            tableSmartTV.Patch();

            var mountSmartTV = new TVPatch("mountSmartTV", "Smart TV With Wall Mount",
                "Take a little break and watch some TV. Includes Wall Mount.",
                ModelPrefab.GetPrefab("MountSmartTV"),
                new Settings
                {
                    KitClassID = "mountSmartTV_kit",
                    AllowedInBase = true,
                    AllowedOutside = false,
                    AllowedOnGround = false,
                    AllowedOnConstructables = true,
                    AllowedOnWall = true,
                    RotationEnabled = false,
                    Cost = 78750,
                    Size = new Vector3(1.818158f, 1.101903f, 0.08594985f),
                    Center = new Vector3(-0.003030092f, -0.00144583f, 0.05641174f)
                });
            mountSmartTV.Patch();
        }

        private void LoadCurtainTemplates()
        {

            var path = Path.Combine(Mod.GetAssetPath(), "CurtainTemplates");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var patterns = Directory.GetFiles(path, "*.png");

            foreach (string filePath in patterns)
            {
                if (File.Exists(filePath))
                {
                    var texture = ImageUtils.LoadTextureFromFile(filePath);
                    Patterns.Add(filePath, texture);
                    PatternsIcon.Add(texture, ImageUtils.LoadSpriteFromTexture(texture));
                }
            }

            QuickLogger.Debug($"Curtain templates loaded: {Patterns.Count}.");
        }

        private void PatchCabinets()
        {
            if (!Configuration.IsCabinetsEnabled) return;
            //Patch Cabinet 1
            var cabinet1 = new Cabinet1Buildable();
            cabinet1.Patch();

            //Patch Cabinet 2
            var cabinet2 = new Cabinet2Buildable();
            cabinet2.Patch();

            //Patch Cabinet 3
            var cabinet3 = new Cabinet3Buildable();
            cabinet3.Patch();
        }

        private void PatchSigns()
        {
            if (!Configuration.IsWallSignsEnabled) return;

            var wallSign = new SignEntryPatch("wallSign", "Wall Sign", "Wall-mounted sign, suitable for use indoors. Requires a wall.",
                ModelPrefab.GetPrefab("AlterraWallSign"),
                new Settings
                {
                    KitClassID = "wallSign_kit",
                    AllowedInBase = true,
                    AllowedOutside = false,
                    AllowedOnGround = false,
                    AllowedOnWall = true,
                    RotationEnabled = false,
                    Cost = 3750,
                    CategoryForPDA = TechCategory.InteriorModule,
                    GroupForPDA = TechGroup.InteriorModules,
                    Size = new Vector3(0.9294822f, 0.204877f, 0.04135694f),
                    Center = new Vector3(0f, 0f, 0.04976267f)
                });
            wallSign.Patch();

            var outsideSign = new SignEntryPatch("outsideSign", "Outside Sign", "Freestanding sign, suitable for outside use.",
                ModelPrefab.GetPrefab("AlterraOutsideSign"),
                new Settings
                {
                    KitClassID = "outsideSign_kit",
                    AllowedInBase = false,
                    AllowedOutside = true,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    Cost = 3750,
                    CategoryForPDA = TechCategory.ExteriorModule,
                    GroupForPDA = TechGroup.ExteriorModules,
                    Size = new Vector3(0.920086f, 0.8915547f, 0.07656322f),
                    Center = new Vector3(0f, 0.5162937f, 0f)
                });
            outsideSign.Patch();
        }

        private void PatchFood()
        {
            var mrSpicyChip = new FoodSpawnable(new PeeperBarFoodItemData
            {
                Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("SpicyChips",FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                ClassId = "SpicyChips", 
                Friendly = "MrSpicy Chips",
                Description = "Your taste buds will be screaming in delight from the very first bite of MrSpicy Chips. Get energized for the big project or running for your life on an alien planet, or celebrate your victories over even the most mundane tasks with MrSpicy Chips: an adventure in every bag.",
                Cost = 205,
                Food = 5,
                Water = -1
            });
            mrSpicyChip.Patch();
            Mod.PeeperBarFoods.Add(mrSpicyChip.TechType, mrSpicyChip.Cost);

            var z1Chips = new FoodSpawnable(new PeeperBarFoodItemData
            {
                Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("Z1Chips",FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                ClassId = "Z1Snacks",
                Friendly = "Z1 Snacks",
                Description = "A Countdown to flavor and adventuring, Z1 Snacks bring out your inner intrepid adventurer. Whether you’re running toward adventure or running away because you found it, Z1 Snacks are the ideal compliment. Perfect taste on Z3...Z2...Z1 Snacks!",
                Cost = 205,
                Food = 15,
                Water = -1
            });
            z1Chips.Patch();
            Mod.PeeperBarFoods.Add(z1Chips.TechType, z1Chips.Cost);

            var nutritionPackage = new FoodSpawnable(new PeeperBarFoodItemData
            {
                Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("NutritionPackage", FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                ClassId = "NutritionPackage",
                Friendly = "Alterra Nutrient Pouch",
                Description = "Packed with more nutrients than a momma marsupial could provide, you can feel confident of writing home of your good health and not getting in trouble later at the family reunion. It’s even tastier than the packaging it comes in!",
                Cost = 100,
                Food = 15,
                Water = 1
            });
            nutritionPackage.Patch();
            Mod.PeeperBarFoods.Add(nutritionPackage.TechType, nutritionPackage.Cost);
            
            var spicyPop = new FoodSpawnable(new PeeperBarFoodItemData
            {
                Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("fcs_Dirnk01",FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                ClassId = "SpicyPop",
                Friendly = "MrSpicy Pop",
                Description = "Wake up those taste buds and the rest of you too. A delicious compliment MrSpicy chips -- IF you’re brave enough -- Spicy Pop packs a crate full of flavor in every bottle. Made with zero caffeine because who could take a nap with all that flavor on your tongue!",
                Cost = 300,
                Food = 0,
                Water = 60
            });
            spicyPop.Patch();
            Mod.PeeperBarFoods.Add(spicyPop.TechType, spicyPop.Cost);

            var extraHotCoffee = new FoodSpawnable(new PeeperBarFoodItemData
            {
                Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("ExtraHotCoffee", FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                ClassId = "ExtraHotCoffee",
                Friendly = "Extra Hot Coffee",
                Description = "Whether you’re chilled to the bone and about to die from arctic winds or just keep getting distracted by co-workers, Alterra Extra Hot Coffee will keep you going with rich taste, deep flavor, and always the right amount of cream, sugar, salt, hot chili sauce, bacon, or...whatever makes coffee great:  Alterra knows and Alterra delivers.",
                Cost = 250,
                Food = 0,
                Water = 50
            });
            extraHotCoffee.Patch();
            Mod.PeeperBarFoods.Add(extraHotCoffee.TechType, extraHotCoffee.Cost);

            var fcsOrangeSoda = new FoodSpawnable(new PeeperBarFoodItemData
            {
                Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("fcsOrangeSoda", FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                ClassId = "FCSOrangeSoda",
                Friendly = "FCS Orange Soda",
                Description = "Ah, the simple, joyful flavor of True-Natural synthetic oranges and a little carbon-dioxide. A simple and delicious soda to sparkle the tongue and enjoy youthful memories.",
                Cost = 300,
                Food = 0,
                Water = 60
            });
            fcsOrangeSoda.Patch();
            Mod.PeeperBarFoods.Add(fcsOrangeSoda.TechType, fcsOrangeSoda.Cost);

            var peeperWhiskey = new FoodSpawnable(new PeeperBarFoodItemData
            {
                Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("PeeperWhiskey", FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                ClassId = "PeeperWhiskey",
                Friendly = "Peeper Whiskey",
                Description = "Always a unique flavor, this special whiskey is created from uniquely local ingredients for the perfect taste of your far-away place. Specially tailored for alcoholic effects on peepers; please do not give Peeper Whiskey to underage peepers.",
                Cost = 425,
                Food = 0,
                Water = 85
            });
            peeperWhiskey.Patch();
            Mod.PeeperBarFoods.Add(peeperWhiskey.TechType, peeperWhiskey.Cost);

            var alterraScotch = new FoodSpawnable(new PeeperBarFoodItemData
            {
                Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("AlterraScotch", FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                ClassId = "AlterraScotch",
                Friendly = "Alterra Scotch",
                Description = "An aged whisky made by a single distillery using only malted barley and water, with a complex and mellow flavor. Perfect for taking the edge off a rough day.  (Dealcoholized for your comfort and convenience and reduced chance of hangover)",
                Cost = 475,
                Food = 0,
                Water = 95
            });
            alterraScotch.Patch();
            Mod.PeeperBarFoods.Add(alterraScotch.TechType, alterraScotch.Cost);


            Mod.PeeperBarFoods.Add(TechType.NutrientBlock,100);
        }
    }

    public struct SoundEntry
    {
        public string Message { get; set; }
        public Sound Sound { get; set; }
    }
}