using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Objects;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Enums;
using FCS_HomeSolutions.Mods.Cabinets.Buildable;
using FCS_HomeSolutions.Mods.CrewLocker.Buildable;
using FCS_HomeSolutions.Mods.Curtains.Buildable;
using FCS_HomeSolutions.Mods.Elevator.Buildable;
using FCS_HomeSolutions.Mods.FireExtinguisherRefueler.Buildable;
using FCS_HomeSolutions.Mods.HologramPoster.Buildable;
using FCS_HomeSolutions.Mods.JukeBox.Buildable;
using FCS_HomeSolutions.Mods.LedLights.Buildable;
using FCS_HomeSolutions.Mods.Microwave.Buildable;
using FCS_HomeSolutions.Mods.MiniFountainFilter.Buildables;
using FCS_HomeSolutions.Mods.NeonPlanter.Buildable;
using FCS_HomeSolutions.Mods.PaintTool.Spawnable;
using FCS_HomeSolutions.Mods.PeeperLoungeBar.Buildable;
using FCS_HomeSolutions.Mods.QuantumTeleporter.Buildable;
using FCS_HomeSolutions.Mods.SeaBreeze.Buildable;
using FCS_HomeSolutions.Mods.Shower.Buildable;
using FCS_HomeSolutions.Mods.Sink.Buildable;
using FCS_HomeSolutions.Mods.Sofas.Buildable;
using FCS_HomeSolutions.Mods.Stairs.Buildable;
using FCS_HomeSolutions.Mods.Stove.Buildable;
using FCS_HomeSolutions.Mods.Toilet.Buildable;
using FCS_HomeSolutions.Mods.TrashReceptacle.Buildable;
using FCS_HomeSolutions.Mods.TrashRecycler.Buildable;
using FCS_HomeSolutions.Mods.TV.Buildable;
using FCSCommon.Utilities;
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
                var peeperLoungeBarPatch = new PeeperLoungeBarPatch();
                peeperLoungeBarPatch.Patch();
                PeeperLoungeBarPatch.LoadPeeperLoungeTracks();
                PeeperLoungeBarPatch.PatchFood();
            }

            if (Configuration.IsNeonPlanterEnabled)
            {
                //Patch Neon Planter Pot
                var neonPlanter = new NeonPlanterPatch();
                neonPlanter.Patch();
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

            if (Configuration.IsStoveEnabled)
            {
                //Patch Stove
                var stoveBuildable = new StoveBuildable();
                stoveBuildable.Patch();
            }

            if (Configuration.IsShowerEnabled)
            {
                //Patch Shower
                var shower = new ShowerBuildable();
                shower.Patch();
            }

            if (Configuration.IsElevatorEnabled)
            {
                //Patch Elevator
                var elevator = new ElevatorBuildable();
                elevator.Patch();
                AudioUtils.CreateSound(Path.Combine(Mod.GetAssetPath(), "Audio", "LiftSoundEffect.mp3"));
            }


            var sink = new SinkBuildable();
            sink.Patch();


            var jukeBox = new JukeBoxBuildable();
            jukeBox.Patch();

            var jukeboxSpeaker = new JukeBoxSpeakerBuildable();
            jukeboxSpeaker.Patch();

            var stairs = new StairsBuildable();
            stairs.Patch();


            var hologramPoster = new HologramPosterBuildable();
            hologramPoster.Patch();

            PatchComputers();

            PatchSigns();

            PatchSmartTvs();

            PatchRailings();

            PatchShelvesAndTables();

            PatchLights();

            LoadCurtainTemplates();

            PatchCabinets();

            PatchBenches();

            PatchMiscItems();

            var harmony = new Harmony("com.homesolutions.fstudios");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            
            //Register debug commands
            ConsoleCommandsHandler.Main.RegisterConsoleCommands(typeof(DebugCommands));
        }

        private void PatchMiscItems()
        {
            var toiletBuildable = new ToiletBuildable();
            toiletBuildable.Patch();


            var microwave = new CookerPatch("fcsmicrowave", "Microwave", "N/A",
                ModelPrefab.GetPrefabFromGlobal("FCS_Microwave"),
                new Settings
                {
                    KitClassID = "microwave_kit",
                    AllowedInBase = true,
                    AllowedOutside = false,
                    AllowedOnGround = true,
                    AllowedInSub = true,
                    AllowedOnConstructables = true,
                    RotationEnabled = true,
                    Cost = 1500,
                    Center = new Vector3(0f, 0.2938644f, 0.2363691f),
                    Size = new Vector3(0.8795165f, 0.4697664f, 0.6358638f),
                    CategoryForPDA = TechCategory.InteriorModule,
                    GroupForPDA = TechGroup.InteriorModules
                });
            microwave.Patch();

            var curingCabinet = new CookerPatch("fcsCuringCabinet", "Curing Cabinet", "N/A",
                ModelPrefab.GetPrefabFromGlobal("FCS_CuringCabinet"),
                new Settings
                {
                    KitClassID = "curingCabinet_kit",
                    AllowedInBase = true,
                    AllowedOutside = false,
                    AllowedOnGround = true,
                    AllowedInSub = true,
                    AllowedOnConstructables = true,
                    RotationEnabled = true,
                    Cost = 1500,
                    Center = new Vector3(0.009382606f, 0.5641674f, 0.2060485f),
                    Size = new Vector3(0.9137022f, 1.016351f, 0.587903f),
                    CategoryForPDA = TechCategory.InteriorModule,
                    GroupForPDA = TechGroup.InteriorModules
                },CookingMode.Curing);
            curingCabinet.Patch();
        }

        private void PatchComputers()
        {
            var pcCPU = new DecorationEntryPatch("pccpu", "Computer CPU", "N/A",
                FCSAssetBundlesService.PublicAPI.GetPrefabByName("FCS_PCCpu", FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                new Settings
                {
                    KitClassID = "pccpu_kit",
                    AllowedInBase = true,
                    AllowedOutside = false,
                    AllowedOnGround = true,
                    AllowedInSub = true,
                    AllowedOnConstructables = true,
                    RotationEnabled = true,
                    Cost = 4500,
                    Center = new Vector3(0f, 0.4660995f, 0f),
                    Size = new Vector3(0.3240662f, 0.8089027f, 0.6231821f),
                    CategoryForPDA = TechCategory.InteriorModule,
                    GroupForPDA = TechGroup.InteriorModules
                });
            pcCPU.Patch();


            var pcMonitor = new DecorationEntryPatch("pcmonitor", "Computer Monitor", "N/A",
                FCSAssetBundlesService.PublicAPI.GetPrefabByName("FCS_PCmonitor", FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                new Settings
                {
                    KitClassID = "pcmonitor_kit",
                    AllowedInBase = true,
                    AllowedOutside = false,
                    AllowedOnGround = true,
                    AllowedInSub = true,
                    AllowedOnConstructables = true,
                    RotationEnabled = true,
                    Cost = 4500,
                    Center = new Vector3(9.155273E-05f, 0.4331882f, -0.01636505f),
                    Size = new Vector3(1.074585f, 0.6359282f, 0.2370758f),
                    CategoryForPDA = TechCategory.InteriorModule,
                    GroupForPDA = TechGroup.InteriorModules
                });
            pcMonitor.Patch();
        }

        private void PatchBenches()
        {
            var sofa1 = new Sofa1Buildable();
            sofa1.Patch();
            var sofa2 = new Sofa2Buildable();
            sofa2.Patch();
            var sofa3 = new Sofa3Buildable();
            sofa3.Patch();
            var neonBarStool = new NeonBarStoolBuildable();
            neonBarStool.Patch();
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

            var longLight = new LongLEDLight();
            longLight.Patch();

            var shortLight = new ShortLEDLight();
            shortLight.Patch();

            var wallLight = new WallLEDLight();
            wallLight.Patch();
        }

        private void PatchShelvesAndTables()
        {
            if (!Configuration.IsShelvesAndTablesEnabled) return;

            var floorShelf01 = new DecorationEntryPatch("floorShelf01", "Floor Shelf 1", "Don’t put your things on the floor, put them on this lovely shelf!",
                ModelPrefab.GetPrefabFromGlobal("FCS_FloorShelf01"),
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
                ModelPrefab.GetPrefabFromGlobal("FCS_FloorShelf02"),
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
                ModelPrefab.GetPrefabFromGlobal("FCS_FloorShelf03"),
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
                ModelPrefab.GetPrefabFromGlobal("FCS_FloorShelf04"),
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
                ModelPrefab.GetPrefabFromGlobal("FCS_FloorShelf05"),
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
                ModelPrefab.GetPrefabFromGlobal("FCS_FloorShelf06"),
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
                ModelPrefab.GetPrefabFromGlobal("FCS_FloorShelf07"),
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
                ModelPrefab.GetPrefabFromGlobal("FCS_NeonShelf01"),
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
                    Center = new Vector3(-0.002281189f, -0.01851261f, 0.504714f),
                    Size = new Vector3(1.99176f, 0.08865887f, 0.9866596f),
            CategoryForPDA = TechCategory.InteriorModule,
                    GroupForPDA = TechGroup.InteriorModules
                });
            neonShelf01.Patch();

            var neonShelf02 = new DecorationEntryPatch("neonShelf02", "Neon Shelf 02", "A shelf with neon lights. (Paint Tool Recommended)",
                ModelPrefab.GetPrefabFromGlobal("FCS_NeonShelf02"),
                new Settings
                {
                    KitClassID = "neonShelf02_kit",
                    AllowedInBase = true,
                    AllowedOutside = false,
                    AllowedOnGround = false,
                    AllowedInSub = true,
                    AllowedOnWall = true,
                    AllowedOnConstructables = true,
                    RotationEnabled = false,
                    Cost = 27000,
                    Center = new Vector3(0f, -0.01520546f, 0.3414307f),
                    Size = new Vector3(1.367249f, 0.05680838f, 0.6867371f),
            CategoryForPDA = TechCategory.InteriorModule,
                    GroupForPDA = TechGroup.InteriorModules
                });
            neonShelf02.Patch();


            var neonShelf03 = new DecorationEntryPatch("neonShelf03", "Neon Shelf 03", "A shelf with neon lights. (Paint Tool Recommended)",
                ModelPrefab.GetPrefabFromGlobal("FCS_NeonShelf03"),
                new Settings
                {
                    KitClassID = "neonShelf03_kit",
                    AllowedInBase = true,
                    AllowedOutside = false,
                    AllowedOnGround = false,
                    AllowedInSub = true,
                    AllowedOnWall = true,
                    AllowedOnConstructables = true,
                    RotationEnabled = false,
                    Cost = 27000,
                    Center = new Vector3(0f, -0.01649079f, 0.3413544f),
                    Size = new Vector3(0.6748515f, 0.05640584f, 0.6829834f),
                    CategoryForPDA = TechCategory.InteriorModule,
                    GroupForPDA = TechGroup.InteriorModules
                });
            neonShelf03.Patch();


            var neonTable01 = new DecorationEntryPatch("neonTable01", "Neon Table 01", "A table with neon lights. (Paint Tool Recommended)",
                ModelPrefab.GetPrefabFromGlobal("NeonTable01"),
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
                    Center = new Vector3(0f, 0.5825546f, 0f),
                    Size = new Vector3(1.181431f, 0.8781588f, 1.178702f),
                    CategoryForPDA = TechCategory.InteriorModule,
                    GroupForPDA = TechGroup.InteriorModules
                });
            neonTable01.Patch();

            var neonTable02 = new DecorationEntryPatch("neonTable02", "Neon Table 02", "A table with neon lights. (Paint Tool Recommended)",
                ModelPrefab.GetPrefabFromGlobal("NeonTable02"),
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
                    Center = new Vector3(0f, 0.5819359f, 0f),
                    Size = new Vector3(1.174561f, 0.8792629f, 1.163364f),
                    CategoryForPDA = TechCategory.InteriorModule,
                    GroupForPDA = TechGroup.InteriorModules
                });
            neonTable02.Patch();
        }

        private static void PatchRailings()
        {
            if (!Configuration.IsRailingsEnabled) return;

            var ahsrailing = new DecorationEntryPatch("ahsrailing", "Railing", "A railing to create a barrior",
                ModelPrefab.GetPrefabFromGlobal("Railing_Normal"),
                new Settings
                {
                    KitClassID = "ahsrailing_kit",
                    AllowedInBase = true,
                    AllowedOutside = true,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    Cost = 20000,
                    Center = new Vector3(0f, 0.6807203f, 0f),
                    Size = new Vector3(1.460135f, 1.058847f, 0.1297541f),
                    CategoryForPDA = TechCategory.Misc,
                    GroupForPDA = TechGroup.Miscellaneous
                });
            ahsrailing.Patch();

            var ahsrailingglass = new DecorationEntryPatch("ahsrailingglass", "Railing With Glass",
                "A railing to create a barrior",
                ModelPrefab.GetPrefabFromGlobal("Railing_Normal_wGlass"),
                new Settings
                {
                    KitClassID = "ahsrailingglass_kit",
                    AllowedInBase = true,
                    AllowedOutside = true,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    Cost = 20000,
                    Center = new Vector3(0f, 0.6807203f, 0f),
                    Size = new Vector3(1.460135f, 1.058847f, 0.1297541f),
                    CategoryForPDA = TechCategory.Misc,
                    GroupForPDA = TechGroup.Miscellaneous
                });
            ahsrailingglass.Patch();

            var ahsLeftCornerRail = new DecorationEntryPatch("ahsLeftCornerRailing", "Railing Left Corner",
                "A railing to create a barrior",
                ModelPrefab.GetPrefabFromGlobal("Railing_LeftCorner"),
                new Settings
                {
                    KitClassID = "ahsleftcornerrailing_kit",
                    AllowedInBase = true,
                    AllowedOutside = true,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    Cost = 15000,
                    Center = new Vector3(0.2120895f, 0.6853912f, 0.931488f),
                    Size = new Vector3(1.963943f, 1.051393f, 1.979523f),
                    CategoryForPDA = TechCategory.Misc,
                    GroupForPDA = TechGroup.Miscellaneous
                });
            ahsLeftCornerRail.Patch();

            var ahsLeftCornerwGlassRail = new DecorationEntryPatch("ahsLeftCornerwGlassRailing", "Railing Left Corner wGlass",
                "A railing to create a barrior",
                ModelPrefab.GetPrefabFromGlobal("Railing_LeftCorner_wGlass"),
                new Settings
                {
                    KitClassID = "ahsleftcornerwGlassrailing_kit",
                    AllowedInBase = true,
                    AllowedOutside = true,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    Cost = 15000,
                    Center = new Vector3(0.2120895f, 0.6853912f, 0.931488f),
                    Size = new Vector3(1.963943f, 1.051393f, 1.979523f),
                    CategoryForPDA = TechCategory.Misc,
                    GroupForPDA = TechGroup.Miscellaneous
                });
            ahsLeftCornerwGlassRail.Patch();

            var ahsRightCornerRail = new DecorationEntryPatch("ahsRightCornerRailing", "Railing Right Corner",
                "A railing to create a barrior",
                ModelPrefab.GetPrefabFromGlobal("Railing_RightCorner"),
                new Settings
                {
                    KitClassID = "ahsrightcornerrailing_kit",
                    AllowedInBase = true,
                    AllowedOutside = true,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    Cost = 15000,
                    Center = new Vector3(0.2120895f, 0.6853912f, 0.931488f),
                    Size = new Vector3(1.963943f, 1.051393f, 1.979523f),
                    CategoryForPDA = TechCategory.Misc,
                    GroupForPDA = TechGroup.Miscellaneous
                });
            ahsRightCornerRail.Patch();


            var ahsRightCornerwGlassRail = new DecorationEntryPatch("ahsRightCornerwGlassRailing", "Railing Right Corner wGlass",
                "A railing to create a barrior",
                ModelPrefab.GetPrefabFromGlobal("Railing_RightCorner_wGlass"),
                new Settings
                {
                    KitClassID = "ahsrightcornerwGlassrailing_kit",
                    AllowedInBase = true,
                    AllowedOutside = true,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    Cost = 15000,
                    Center = new Vector3(0.2120895f, 0.6853912f, 0.931488f),
                    Size = new Vector3(1.963943f, 1.051393f, 1.979523f),
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
                ModelPrefab.GetPrefabFromGlobal("FCS_TableSmartTV"),
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
                ModelPrefab.GetPrefabFromGlobal("FCS_MountSmartTV"),
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

            var path = Path.Combine(Mod.GetAssetPath(), "CustomImages");
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

            //Tv Stand
            var tvCabinetStand = new TVStandBuildable();
            tvCabinetStand.Patch();

            //Crew Locker
            var crewLocker = new CrewLockerBuildable();
            crewLocker.Patch();
        }

        private void PatchSigns()
        {
            if (!Configuration.IsWallSignsEnabled) return;

            var wallSign = new SignEntryPatch("wallSign", "Wall Sign", "Wall-mounted sign, suitable for use indoors. Requires a wall.",
                ModelPrefab.GetPrefabFromGlobal("FCS_AlterraWallSign"),
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
                ModelPrefab.GetPrefabFromGlobal("FCS_AlterraOutsideSign"),
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
    }
}