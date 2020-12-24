using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FCS_HomeSolutions.BaseOperator.Buildable;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Buildables.OutDoorPlanters;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Curtains.Mono;
using FCS_HomeSolutions.MiniFountainFilter.Buildables;
using FCS_HomeSolutions.Mods.AlienChief.Buildables;
using FCS_HomeSolutions.Mods.Cabinets.Buildable;
using FCS_HomeSolutions.Mods.LedLights.Buildable;
using FCS_HomeSolutions.QuantumTeleporter.Buildable;
using FCS_HomeSolutions.SeaBreeze.Buildable;
using FCS_HomeSolutions.Spawnables;
using FCS_HomeSolutions.TrashReceptacle.Buildable;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using HarmonyLib;
using QModManager.API.ModLoading;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_HomeSolutions
{
    /*
     * Alterra Home Solutions mod pack adds objects to subnautica that deals with bases and decorations
    */

    [QModCore]
    public class QPatch
    {
        internal static Config Configuration { get; } = OptionsPanelHandler.Main.RegisterModOptions<Config>();

        internal static HoverLiftPadConfig HoverLiftPadConfiguration { get; } =
            OptionsPanelHandler.Main.RegisterModOptions<HoverLiftPadConfig>();

        internal static PaintToolConfig PaintToolConfiguration { get; } =
            OptionsPanelHandler.Main.RegisterModOptions<PaintToolConfig>();

        internal static MiniFountainConfig MiniFountainFilterConfiguration { get; } =
            OptionsPanelHandler.Main.RegisterModOptions<MiniFountainConfig>();

        internal static SeaBreezeConfig SeaBreezeConfiguration { get; } =
            OptionsPanelHandler.Main.RegisterModOptions<SeaBreezeConfig>();

        internal static QuantumTeleporterConfig QuantumTeleporterConfiguration { get; } =
            OptionsPanelHandler.Main.RegisterModOptions<QuantumTeleporterConfig>();

        internal static Dictionary<string, Texture2D> Patterns = new Dictionary<string, Texture2D>();
        internal static Dictionary<Texture2D, Atlas.Sprite> PatternsIcon = new Dictionary<Texture2D, Atlas.Sprite>();

        [QModPatch]
        public void Patch()
        {
            QuickLogger.Info(
                $"Started patching. Version: {QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly())}");

            ModelPrefab.Initialize();

            AuxPatchers.AdditionalPatching();

            var ahsSweetWaterBar = new SweetWaterBarPatch("ahsSweetWaterBar", "Sweet Water Bar",
                "All drinks on the house.", ModelPrefab.GetPrefab("SweetWaterBar"), new Settings
                {
                    KitClassID = "ahsSweetWaterBar_kit",
                    AllowedInBase = true,
                    AllowedOutside = true,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    Size = new Vector3(3.009064f, 2.474508f, 1.905116f),
                    Center = new Vector3(-0.1594486f, 1.41353f, 0.02790368f)
                });
            ahsSweetWaterBar.Patch();

            //Patch Paint Tool
            var paintToolSpawnable = new PaintToolSpawnable();
            paintToolSpawnable.Patch();

            //Patch Base Operator
            var baseOperator = new BaseOperatorPatch();
            baseOperator.Patch();

            //Patch hover Lift Operator
            var hoverLiftPad = new HoverLiftPadPatch();
            hoverLiftPad.Patch();

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

            LoadOtherObjects();

            LoadLights();

            LoadCurtainTemplates();

            LoadCabinets();

            var harmony = new Harmony("com.homesolutions.fstudios");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            //Register debug commands
            ConsoleCommandsHandler.Main.RegisterConsoleCommands(typeof(DebugCommands));
        }

        private void LoadLights()
        {
            var longLight = new LedLightPatch(new LedLightData
            {
                classId = "LedLightStickLong",
                description = "A long led light stick for external use on your base exterior with color changing.",
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
                description = "A long led light stick for external use on your base exterior with color changing.",
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
                description =
                    "A wall mountable led light strip for internal use on your base exterior with color changing.",
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
                    Size = new Vector3(1f, 0.822893f, 0.1791649f),
                    Center = new Vector3(-1.365597e-25f, 0.7452481f, -0.004088677f)
                });
            ahssmallrailglass.Patch();

            var ahssmallstairplatform = new DecorationEntryPatch("ahssmallstairplatform", "Small Stair Platform",
                "A stairs for your personal needs.", ModelPrefab.GetPrefab("Small_PlatformDoorStairs"),
                new Settings
                {
                    KitClassID = "ahssmallstairplatform_kit",
                    AllowedInBase = false,
                    AllowedOutside = true,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    Size = new Vector3(4.033218f, 2.194448f, 2.34824f),
                    Center = new Vector3(4.449882e-24f, 1.225415f, 0.8919346f)
                });
            ahssmallstairplatform.Patch();

            var ahssmallrailmesh = new DecorationEntryPatch("ahssmallrailmesh", "Small Railing With Mesh",
                "A railing to create a barrior",
                ModelPrefab.GetPrefab("Small_Rail_wNeat_01"),
                new Settings
                {
                    KitClassID = "ahsSmallRailMesh_kit",
                    AllowedInBase = true,
                    AllowedOutside = true,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    Size = new Vector3(1f, 0.822893f, 0.1791649f),
                    Center = new Vector3(-1.365597e-25f, 0.7452481f, -0.004088677f)
                });
            ahssmallrailmesh.Patch();

            var ahslargerailmesh = new DecorationEntryPatch("ahslargerailmesh", "Large Railing With Mesh",
                "A railing to create a barrior",
                ModelPrefab.GetPrefab("Large_Rail_wNeat_01"),
                new Settings
                {
                    KitClassID = "ahsSmallRailMesh_kit",
                    AllowedInBase = true,
                    AllowedOutside = true,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    Size = new Vector3(1.963638f, 1.020765f, 0.1433573f),
                    Center = new Vector3(0f, 0.6343491f, 0f)
                });
            ahslargerailmesh.Patch();

            var floorShelf01 = new DecorationEntryPatch("floorShelf01", "Floor Shelf 1", "A neat shelf",
                ModelPrefab.GetPrefab("FloorShelf01"),
                new Settings
                {
                    KitClassID = "floorShelf01_kit",
                    AllowedInBase = true,
                    AllowedOutside = false,
                    AllowedOnGround = true,
                    AllowedOnConstructables = true,
                    RotationEnabled = true,
                    Center = new Vector3(0f, 0.4615f, 0f),
                    Size = new Vector3(0.6667f, 0.9036641f, 0.6438f),
                });
            floorShelf01.Patch();

            var floorShelf02 = new DecorationEntryPatch("floorShelf02", "Floor Shelf 2", "A neat shelf",
                ModelPrefab.GetPrefab("FloorShelf02"),
                new Settings
                {
                    KitClassID = "floorShelf02_kit",
                    AllowedInBase = true,
                    AllowedOutside = false,
                    AllowedOnGround = true,
                    RotationEnabled = true,
                    Size = new Vector3(0.6780548f, 2.636386f, 0.6340148f),
                    Center = new Vector3(69.55f, 1.381544f, 0f)
                });
            floorShelf02.Patch();

            var floorShelf03 = new DecorationEntryPatch("floorShelf03", "Floor Shelf 3", "A neat shelf",
                ModelPrefab.GetPrefab("FloorShelf03"),
                new Settings
                {
                    KitClassID = "floorShelf03_kit",
                    AllowedInBase = true,
                    AllowedOutside = false,
                    AllowedOnGround = true,
                    AllowedOnConstructables = true,
                    RotationEnabled = true,
                    Size = new Vector3(0.6776733f, 1.692158f, 0.6340148f),
                    Center = new Vector3(75.744f, 0.9614337f, 0f)
                });
            floorShelf03.Patch();


            var floorShelf04 = new DecorationEntryPatch("floorShelf04", "Floor Shelf 4", "A neat shelf",
                ModelPrefab.GetPrefab("FloorShelf04"),
                new Settings
                {
                    KitClassID = "floorShelf04_kit",
                    AllowedInBase = true,
                    AllowedOutside = false,
                    AllowedOnGround = true,
                    AllowedOnConstructables = true,
                    RotationEnabled = true,
                    Size = new Vector3(2.814087f, 1.54613f, 0.6510811f),
                    Center = new Vector3(77.73706f, 0.9081734f, -0.005623609f)
                });
            floorShelf04.Patch();

            var floorShelf05 = new DecorationEntryPatch("floorShelf05", "Floor Shelf 5", "A neat shelf",
                ModelPrefab.GetPrefab("FloorShelf05"),
                new Settings
                {
                    KitClassID = "floorShelf05_kit",
                    AllowedInBase = true,
                    AllowedOutside = false,
                    AllowedOnGround = true,
                    AllowedOnConstructables = true,
                    RotationEnabled = true,
                    Size = new Vector3(2.820679f, 1.546359f, 0.6349438f),
                    Center = new Vector3(80.90942f, 0.9178838f, -0.01041579f)
                });
            floorShelf05.Patch();

            var floorShelf06 = new DecorationEntryPatch("floorShelf06", "Floor Shelf 6", "A neat shelf",
                ModelPrefab.GetPrefab("FloorShelf06"),
                new Settings
                {
                    KitClassID = "floorShelf06_kit",
                    AllowedInBase = true,
                    AllowedOutside = false,
                    AllowedOnGround = true,
                    AllowedOnConstructables = true,
                    RotationEnabled = true,
                    Size = new Vector3(2.781815f, 0.8054426f, 0.6553071f),
                    Center = new Vector3(84.07667f, 0.5206897f, -0.01041579f)
                });
            floorShelf06.Patch();


            var floorShelf07 = new DecorationEntryPatch("floorShelf07", "Floor Shelf 7", "A neat shelf",
                ModelPrefab.GetPrefab("FloorShelf07"),
                new Settings
                {
                    KitClassID = "floorShelf07_kit",
                    AllowedInBase = true,
                    AllowedOutside = false,
                    AllowedOnGround = true,
                    AllowedOnConstructables = true,
                    RotationEnabled = true,
                    Size = new Vector3(2.764771f, 0.7996328f, 0.6282197f),
                    Center = new Vector3(73.80698f, 0.5240374f, -0.01041579f)
                });
            floorShelf07.Patch();

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

        public static Sprite ConvertTextureToSprite(Texture2D texture, float PixelsPerUnit = 100.0f,
            SpriteMeshType spriteType = SpriteMeshType.Tight)
        {
            // Converts a Texture2D to a sprite, assign this texture to a new sprite and return its reference

            Sprite NewSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0),
                PixelsPerUnit, 0, spriteType);

            return NewSprite;
        }
    }
}