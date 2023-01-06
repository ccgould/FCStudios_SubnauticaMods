using System.IO;
using System.Reflection;
using BepInEx;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Model.GUI;
using FCS_AlterraHub.Registration;
using FCS_EnergySolutions.AlterraSolarCluster.Buildables;
using FCS_EnergySolutions.Buildable;
using FCS_EnergySolutions.Configuration;
using FCS_EnergySolutions.Mods.AlterraGen.Buildables;
using FCS_EnergySolutions.Mods.JetStreamT242.Buildables;
using FCS_EnergySolutions.Mods.PowerStorage.Buildable;
using FCS_EnergySolutions.Mods.Spawnables;
using FCS_EnergySolutions.Mods.TelepowerPylon.Buildable;
using FCS_EnergySolutions.Mods.UniversalCharger.Buildable;
using FCS_EnergySolutions.Mods.WindSurfer.Buildables;
using FCS_EnergySolutions.Spawnables;
using FCSCommon.Utilities;
using HarmonyLib;
using SMLHelper.Handlers;
using SMLHelper.Utility;
using UnityEngine;

namespace FCS_EnergySolutions
{

    /*
     * If you are trying to build the project after changing it for subnautica check the build settings
     * make sure all build settings line up with the correct engine.
     */



    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class Main : BaseUnityPlugin
    {
        #region [Declarations]

        public const string
            MODNAME = "FCS_EnergySolutions",
            AUTHOR = "FieldCreatorsStudios",
            GUID = AUTHOR + "_" + MODNAME,
            VERSION = "1.0.0.0";
        #endregion

        internal string Version { get; private set; }
        internal static Config Configuration { get; } = OptionsPanelHandler.RegisterModOptions<Config>();

        private void Awake()
        {
            FCSAlterraHubService.PublicAPI.RegisterModPack(Mod.ModPackID, Mod.ModBundleName, Assembly.GetExecutingAssembly());
            FCSAlterraHubService.PublicAPI.RegisterEncyclopediaEntry(Mod.ModPackID);

            AuxPatchers.AdditionalPatching();
            ModelPrefab.Initialize();

            if (Configuration.IsAlterraGenEnabled)
            {
                var alterraGen = new AlterraGenBuildable();
                alterraGen.Patch();

                var bioFuelSpawnable = new BioFuelSpawnable();
                bioFuelSpawnable.Patch();
            }

            if (Configuration.IsAlterraSolarPanelClusterEnabled)
            {
                var alterraSolarCluster = new AlterraSolarClusterBuildable();
                alterraSolarCluster.Patch();
            }

            if (Configuration.IsJetStreamT242Enabled)
            {
                var jetStreamT242 = new JetStreamT242Patcher();
                jetStreamT242.Patch();
            }

            if (Configuration.IsTelepowerPylonEnabled)
            {
                var telepowerPylon = new TelepowerPylonBuildable();
                telepowerPylon.Patch();

                var mk2PylonUpgrade = new TelepowerUpgradeSpawnable("TelepowerMk2Upgrade", "Telepower MK2 Upgrade",
                    "Allows you to upgrade your Telepower Pylon to the MK2 level which allows you to connect to 8 devices",
                    1000000, Color.cyan);
                mk2PylonUpgrade.Patch();

                var mk3PylonUpgrade = new TelepowerUpgradeSpawnable("TelepowerMk3Upgrade", "Telepower MK3 Upgrade",
                    "Allows you to upgrade your Telepower Pylon to the MK3 level which allows you to connect to 10 devices",
                    2000000, Color.green);
                mk3PylonUpgrade.Patch();
            }

            if (Configuration.IsPowerStorageEnabled)
            {
                var powerStorage = new PowerStoragePatcher();
                powerStorage.Patch();
            }
            
            if (Configuration.IsUniversalChargerEnabled)
            {
                var universalCharger = new UniversalChargerPatcher();
                universalCharger.Patch();

                for (int i = 0; i < 10; i++)
                {
                    int slotIndex = i + 1;
                    EquipmentConfiguration.AddNewSlot($"UVPowerCellCharger{slotIndex}", new SlotInformation
                    {
                        EquipmentType = EquipmentType.PowerCellCharger,
                        Position = EquipmentConfiguration.GetSlotPosition(slotIndex)
                    });
                }

                for (int i = 0; i < 10; i++)
                {
                    int slotIndex = i + 1;
                    EquipmentConfiguration.AddNewSlot($"UCBatteryCharger{slotIndex}", new SlotInformation
                    {
                        EquipmentType = EquipmentType.BatteryCharger,
                        Position = EquipmentConfiguration.GetSlotPosition(slotIndex)
                    });
                }
            }

            if (Configuration.IsWindSurferEnabled)
            {
                CraftTreeHandler.AddTabNode(CraftTree.Type.Constructor, "FCSWindSurfer", "Wind Surfer",
                    ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetFolder(), $"{Mod.WindSurferClassName}.png")));

                var windSurferOperator = new WindSurferOperatorBuildable();
                windSurferOperator.Patch();

                var windSurfer = new WindSurferSpawnable();
                windSurfer.Patch();

                var windSurferPlatform = new WindSurferPlatformSpawnable();
                windSurferPlatform.Patch();
            }

            //Register debug commands
            ConsoleCommandsHandler.RegisterConsoleCommands(typeof(DebugCommands));

            //Harmony
            var harmony = new Harmony("com.energrysolutions.fcstudios");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            var constructorOriginal = AccessTools.Method(typeof(ConstructorInput), "OnCraftingBegin");
            var constructorPrefix = new HarmonyMethod(AccessTools.Method(typeof(ConstructorInput_Patch), "Prefix"));
            harmony.Patch(constructorOriginal, constructorPrefix);
            QuickLogger.Info($"Finished patching. Version: {QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly())}");
        }

        public static class ConstructorInput_Patch
        {
            public static void Prefix(TechType techType, ref float duration)
            {
                if (techType == Mod.WindSurferOperatorClassName.ToTechType())
                {
                    duration = 20f; //Takes 20 seconds to build
                    FMODUWE.PlayOneShot("event:/tools/constructor/spawn", Player.main.transform.position, 1f); //Cyclops does this i think
                }
            }
        }
    }
}
