using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FCS_HomeSolutions.BaseOperator.Buildable;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Buildables.OutDoorPlanters;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Curtains.Mono;
using FCS_HomeSolutions.MiniFountainFilter.Buildables;
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
        internal static HoverLiftPadConfig HoverLiftPadConfiguration { get; } = OptionsPanelHandler.Main.RegisterModOptions<HoverLiftPadConfig>();
        internal static MiniFountainConfig MiniFountainFilterConfiguration { get; } = OptionsPanelHandler.Main.RegisterModOptions<MiniFountainConfig>();
        internal static SeaBreezeConfig SeaBreezeConfiguration { get; } = OptionsPanelHandler.Main.RegisterModOptions<SeaBreezeConfig>();
        internal static QuantumTeleporterConfig QuantumTeleporterConfiguration { get; } = OptionsPanelHandler.Main.RegisterModOptions<QuantumTeleporterConfig>();

        internal static Dictionary<string,Texture2D> Patterns = new Dictionary<string, Texture2D>();
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
                    Size = new Vector3(3.075155f, 2.793444f, 1.905103f),
                    Center = new Vector3(-0.1215184f, 1.516885f, -0.008380651f)
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
            var smartOutDoorPlanter = new OutDoorPlanterPatch(Mod.SmartPlanterPotClassID, Mod.SmartPlanterPotFriendly, Mod.SmartPlanterPotDescription, ModelPrefab.SmallOutdoorPot, new Settings
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
            
            LoadOtherObjects();

            LoadCurtainTemplates();

            var harmony = new Harmony("com.homesolutions.fstudios");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            
            //Register debug commands
            ConsoleCommandsHandler.Main.RegisterConsoleCommands(typeof(DebugCommands));
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

            var ahsrailingglass = new DecorationEntryPatch("ahsrailingglass", "Large Railing With Glass", "A railing to create a barrior",
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

            var ahssmallrail = new DecorationEntryPatch("ahssmallrail", "Small Railing", "A railing to create a barrior",
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

            var ahssmallrailglass = new DecorationEntryPatch("ahssmallrailglass", "Small Railing With Glass", "A railing to create a barrior",
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

            var ahssmallrailmesh = new DecorationEntryPatch("ahssmallrailmesh", "Small Railing With Mesh", "A railing to create a barrior",
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

            var ahslargerailmesh = new DecorationEntryPatch("ahslargerailmesh", "Large Railing With Mesh", "A railing to create a barrior",
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

        public static Sprite ConvertTextureToSprite(Texture2D texture, float PixelsPerUnit = 100.0f, SpriteMeshType spriteType = SpriteMeshType.Tight)
        {
            // Converts a Texture2D to a sprite, assign this texture to a new sprite and return its reference

            Sprite NewSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), PixelsPerUnit, 0, spriteType);

            return NewSprite;
        }
    }
}
