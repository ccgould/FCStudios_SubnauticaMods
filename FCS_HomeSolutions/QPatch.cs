using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Objects;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Buildables.OutDoorPlanters;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.MiniFountainFilter.Buildables;
using FCS_HomeSolutions.Mods.AlienChief.Buildables;
using FCS_HomeSolutions.Mods.Cabinets.Buildable;
using FCS_HomeSolutions.Mods.FireExtinguisherRefueler.Buildable;
using FCS_HomeSolutions.Mods.LedLights.Buildable;
using FCS_HomeSolutions.Mods.TV.Buildable;
using FCS_HomeSolutions.QuantumTeleporter.Buildable;
using FCS_HomeSolutions.SeaBreeze.Buildable;
using FCS_HomeSolutions.Spawnables;
using FCS_HomeSolutions.TrashReceptacle.Buildable;
using FCSCommon.Utilities;
using FMODUnity;
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
        internal static TelevisionConfig TelevisionConfiguration { get; } = OptionsPanelHandler.Main.RegisterModOptions<TelevisionConfig>();
        internal static QuantumTeleporterConfig QuantumTeleporterConfiguration { get; } = OptionsPanelHandler.Main.RegisterModOptions<QuantumTeleporterConfig>();

        internal static HoverLiftPadConfig HoverLiftPadConfiguration { get; } =
            OptionsPanelHandler.Main.RegisterModOptions<HoverLiftPadConfig>();

        internal static PaintToolConfig PaintToolConfiguration { get; } =
            OptionsPanelHandler.Main.RegisterModOptions<PaintToolConfig>();

        internal static MiniFountainConfig MiniFountainFilterConfiguration { get; } =
            OptionsPanelHandler.Main.RegisterModOptions<MiniFountainConfig>();

        internal static SeaBreezeConfig SeaBreezeConfiguration { get; } =
            OptionsPanelHandler.Main.RegisterModOptions<SeaBreezeConfig>();

        internal static Dictionary<string, Texture2D> Patterns = new Dictionary<string, Texture2D>();

        internal static Dictionary<Texture2D, Atlas.Sprite> PatternsIcon = new Dictionary<Texture2D, Atlas.Sprite>();

        [QModPatch]
        public void Patch()
        {
            QuickLogger.Info(
                $"Started patching. Version: {QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly())}");

            ModelPrefab.Initialize();

            AuxPatchers.AdditionalPatching();
            
            //Load Additional Colors
            LoadAdditionalColors();

            var ahsSweetWaterBar = new SweetWaterBarPatch("ahsSweetWaterBar", "Sweet Water Bar",
                "All drinks on the house.", ModelPrefab.GetPrefab("SweetWaterBar"), new Settings
                {
                    KitClassID = "ahsSweetWaterBar_kit",
                    AllowedInBase = true,
                    AllowedOutside = true,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    Cost = 45000,
                    Size = new Vector3(3.009064f, 2.474508f, 1.905116f),
                    Center = new Vector3(-0.1594486f, 1.41353f, 0.02790368f)
                });
            ahsSweetWaterBar.Patch();


            //Patch Paint Tool
            var paintToolSpawnable = new PaintToolSpawnable();
            paintToolSpawnable.Patch();

            ////Patch Base Operator
            //var baseOperator = new BaseOperatorPatch();
            //baseOperator.Patch();

            ////Patch hover Lift Operator
            //var hoverLiftPad = new HoverLiftPadPatch();
            //hoverLiftPad.Patch();

            //Patch Smart Planter Pot
            var smartOutDoorPlanter = new OutDoorPlanterPatch(Mod.SmartPlanterPotClassID, Mod.SmartPlanterPotFriendly,
                Mod.SmartPlanterPotDescription, ModelPrefab.SmallOutdoorPot, new Settings
                {
                    KitClassID = Mod.SmartPlanterPotKitClassID,
                    Size = new Vector3(0.7929468f, 0.3463891f, 0.7625999f),
                    Center = new Vector3(0f, 0.2503334f, 0f)
                });

            smartOutDoorPlanter.Patch();

            //Patch Mini Fountain Filter
            var miniFountainFilter = new MiniFountainFilterBuildable();
            miniFountainFilter.Patch();

            //Patch SeaBreeze
            var seaBreeze = new SeaBreezeBuildable();
            seaBreeze.Patch();

            //Patch Paint Can
            var paintCan = new PaintCanSpawnable();
            paintCan.Patch();

            //Fire Extinguisher Refueler
            var fireExtinguisherRefueler = new FireExtinguisherRefuelerBuildable();
            fireExtinguisherRefueler.Patch();

            //Patch Trash Receptacle
            var trashReceptacle = new TrashReceptaclePatch();
            trashReceptacle.Patch();

            //Patch Trash Recycler
            var trashRecycler = new TrashRecyclerPatch();
            trashRecycler.Patch();


            //Patch Curtain
            var curtain = new CurtainPatch();
            curtain.Patch();

            //Patch Quantum Teleporter
            var quantumTeleporter = new QuantumTeleporterBuildable();
            quantumTeleporter.Patch();

            //Patch Alien Chief
            var alienChief = new AlienChefBuildable();
            alienChief.Patch();


            //Patch Alterra Mini Bathroom
            var alterraMiniBathroom = new AlterraMiniBathroomBuildable();
            alterraMiniBathroom.Patch();

            LoadSigns();

            LoadOtherObjects();

            LoadLights();

            LoadCurtainTemplates();

            LoadCabinets();
            
            var harmony = new Harmony("com.homesolutions.fstudios");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            
            //Register debug commands
            ConsoleCommandsHandler.Main.RegisterConsoleCommands(typeof(DebugCommands));
        }

        private void LoadAdditionalColors()
        {
            foreach (AdditionalColor additionalColor in PaintToolConfiguration.AdditionalPaintColors)
            {
                ColorList.AddColor(additionalColor.Color,additionalColor.ColorName);
            }
        }

        private void LoadLights()
        {
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

        private void LoadOtherObjects()
        {
            var ahsrailing = new DecorationEntryPatch("ahsrailing", "Large Railing", "A railing to create a barrior",
                ModelPrefab.GetPrefab("Large_Rail_01"),
                new Settings
                {
                    KitClassID = "ahsrailing_kit",
                    AllowedInBase = true,
                    AllowedOutside = true,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    Cost = 20000,
                    Size = new Vector3(1.963638f, 1.020765f, 0.1433573f),
                    Center = new Vector3(0f, 0.6343491f, 0f)
                });
            ahsrailing.Patch();

            var ahsrailingglass = new DecorationEntryPatch("ahsrailingglass", "Large Railing With Glass",
                "A railing to create a barrior",
                ModelPrefab.GetPrefab("Large_Rail_wGlass_01"),
                new Settings
                {
                    KitClassID = "ahsrailingglass_kit",
                    AllowedInBase = true,
                    AllowedOutside = true,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    Cost = 20000,
                    Size = new Vector3(1.963638f, 1.020765f, 0.1433573f),
                    Center = new Vector3(0f, 0.6343491f, 0f)
                });
            ahsrailingglass.Patch();

            var ahssmallrail = new DecorationEntryPatch("ahssmallrail", "Small Railing",
                "A railing to create a barrior",
                ModelPrefab.GetPrefab("Small_Rail_01"),
                new Settings
                {
                    KitClassID = "ahssmallrailing_kit",
                    AllowedInBase = true,
                    AllowedOutside = true,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    Cost = 15000,
                    Size = new Vector3(1f, 0.822893f, 0.1791649f),
                    Center = new Vector3(-1.365597e-25f, 0.7452481f, -0.004088677f)
                });
            ahssmallrail.Patch();

            var ahssmallrailglass = new DecorationEntryPatch("ahssmallrailglass", "Small Railing With Glass",
                "A railing to create a barrior",
                ModelPrefab.GetPrefab("Small_Rail_wGlass_01"),
                new Settings
                {
                    KitClassID = "ahssmallrailingglass_kit",
                    AllowedInBase = true,
                    AllowedOutside = true,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    Cost = 15000,
                    Size = new Vector3(1f, 0.822893f, 0.1791649f),
                    Center = new Vector3(-1.365597e-25f, 0.7452481f, -0.004088677f)
                });
            ahssmallrailglass.Patch();

            var ahssmallstairplatform = new DecorationEntryPatch("ahssmallstairplatform", "Small Stair Platform",
                "A small set of stairs so you don’t bang your knees.", ModelPrefab.GetPrefab("Small_PlatformDoorStairs"),
                new Settings
                {
                    KitClassID = "ahssmallstairplatform_kit",
                    AllowedInBase = false,
                    AllowedOutside = true,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    Cost = 18000,
                    Size = new Vector3(4.033218f, 2.194448f, 2.34824f),
                    Center = new Vector3(4.449882e-24f, 1.225415f, 0.8919346f)
                });
            ahssmallstairplatform.Patch();

            //var ahssmallrailmesh = new DecorationEntryPatch("ahssmallrailmesh", "Small Railing With Mesh",
            //    "A railing to create a barrior",
            //    ModelPrefab.GetPrefab("Small_Rail_wNeat_01"),
            //    new Settings
            //    {
            //        KitClassID = "ahsSmallRailMesh_kit",
            //        AllowedInBase = true,
            //        AllowedOutside = true,
            //        AllowedOnGround = true,
            //        RotationEnabled = true,
            //        Size = new Vector3(1f, 0.822893f, 0.1791649f),
            //        Center = new Vector3(-1.365597e-25f, 0.7452481f, -0.004088677f)
            //    });
            //ahssmallrailmesh.Patch();

            //var ahslargerailmesh = new DecorationEntryPatch("ahslargerailmesh", "Large Railing With Mesh",
            //    "A railing to create a barrior",
            //    ModelPrefab.GetPrefab("Large_Rail_wNeat_01"),
            //    new Settings
            //    {
            //        KitClassID = "ahsSmallRailMesh_kit",
            //        AllowedInBase = true,
            //        AllowedOutside = true,
            //        AllowedOnGround = true,
            //        RotationEnabled = true,
            //        Size = new Vector3(1.963638f, 1.020765f, 0.1433573f),
            //        Center = new Vector3(0f, 0.6343491f, 0f)
            //    });
            //ahslargerailmesh.Patch();

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
                    Center = new Vector3(0f, 0.9526114f, 0f)
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
                    Center = new Vector3(-0.1762199f, 0.8602326f, 0f)
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
                    Center = new Vector3(-0.1675282f, 0.86191998f, 0f)
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
                    Center = new Vector3(-0.1740894f, 0.4856191f, 0.002290621f)
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
                    Center = new Vector3(2.764177f, 0.8583584f, 0.6468056f)
                });
            floorShelf07.Patch();

            var tableSmartTV = new TVPatch("tableSmartTV", "Smart TV w/ Wall Mount", "Take a little break and watch some TV. Includes Table Mount.",
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

            var mountSmartTV = new TVPatch("mountSmartTV", "Smart TV w/ Stand", "Take a little break and watch some TV. Includes Wall Mount.",
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
                    Center = new Vector3(-2.488494e-05f, -0.01308646f, 0.5065421f)
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
                    Center = new Vector3(-2.488494e-05f, 0.5808856f, 0f)
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
                    Center = new Vector3(-2.488494e-05f, 0.5808856f, 0f)
                });
            neonTable02.Patch();

            var observationTank = new ObservationTankBuildable();
            observationTank.Patch();
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

        private void LoadCabinets()
        {
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

        private void LoadSigns()
        {
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
    }
}