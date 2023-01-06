using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using FCS_AlterraHub.Registration;
using FCS_StorageSolutions.Configuration;
using FCS_StorageSolutions.Mods.AlterraStorage.Buildable;
using FCS_StorageSolutions.Mods.DataStorageSolutions.Buildable;
using FCS_StorageSolutions.Mods.DataStorageSolutions.Spawnable;
using FCS_StorageSolutions.Patches;
using FCSCommon.Utilities;
using HarmonyLib;
using SMLHelper.Handlers;

namespace FCS_StorageSolutions
{
    /*
     * Storage Solutions is a pack of Subnautica Mods that allow you to store items in the game.
     * This mod can be configured to show certain mods before runtime and can only be changed before runtime.
     */
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class Main : BaseUnityPlugin
    {
        #region [Declarations]

        public const string
            MODNAME = "FCS_StorageSolutions",
            AUTHOR = "FieldCreatorsStudios",
            GUID = AUTHOR + "_" + MODNAME,
            VERSION = "1.0.0.0";
        internal static Config Configuration { get; } = OptionsPanelHandler.RegisterModOptions<Config>();
        public static bool IsDockedVehicleStorageAccessInstalled { get; set; }
        #endregion


        private void Awake()
        {
            FCSAlterraHubService.PublicAPI.RegisterModPack(Mod.ModPackID, Mod.ModBundleName, Assembly.GetExecutingAssembly());
            FCSAlterraHubService.PublicAPI.RegisterEncyclopediaEntry(Mod.ModPackID);

            IsDockedVehicleStorageAccessInstalled = BepInEx.Bootstrap.Chainloader.PluginInfos.Values.Any(x => x.Metadata.Name.Equals("DockedVehicleStorageAccess"));


            ModelPrefab.Initialize();

            AuxPatchers.AdditionalPatching();

            if (Configuration.IsRemoteStorageEnabled)
            {
                var alterraStorage = new AlterraStoragePatch();
                alterraStorage.Patch();
            }

            if (Configuration.IsDataStorageSolutionsEnabled)
            {
                var dssserver = new DSSServerSpawnable();
                dssserver.Patch();

                var dssItemDisplay = new DSSItemDisplayPatch();
                dssItemDisplay.Patch();

                var dssTerminal = new DSSTerminalPatch();
                dssTerminal.Patch();
                
                var dssAntenna = new DSSAntennaPatch();
                dssAntenna.Patch();
                
                var wallServerRack = new DSSWallServerRackPatch();
                wallServerRack.Patch();

                var floorServerRack = new DSSFloorServerRackPatch();
                floorServerRack.Patch();

                var dssTransceiver = new DSSTransceiver();
                dssTransceiver.Patch();
            }

            //Register debug commands
            ConsoleCommandsHandler.RegisterConsoleCommands(typeof(DebugCommands));

            //Harmony
            var harmony = new Harmony("com.storagesolutions.fcstudios");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            PatchToolTipFactory(harmony);

            QuickLogger.Info($"Finished Patching");
        }

        private static void PatchToolTipFactory(Harmony harmony)
        {
            var toolTipFactoryType = Type.GetType("TooltipFactory, Assembly-CSharp");

            if (toolTipFactoryType != null)
            {
                QuickLogger.Debug("Got TooltipFactory Type");

                var inventoryItemViewMethodInfo = toolTipFactoryType.GetMethod("InventoryItem");

                if (inventoryItemViewMethodInfo != null)
                {
                    QuickLogger.Info("Got Inventory Item View Method Info");
                    var postfix = typeof(TooltipFactory_Patch).GetMethod("GetToolTip");
                    harmony.Patch(inventoryItemViewMethodInfo, null, new HarmonyMethod(postfix));
                }
            }
        }

    }
}
