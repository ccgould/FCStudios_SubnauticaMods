using System;
using System.Reflection;
using DataStorageSolutions.Patches;
using FCS_StorageSolutions.Configuration;
using FCS_StorageSolutions.Mods.AlterraStorage.Buildable;
using FCS_StorageSolutions.Mods.DataStorageSolutions.Buildable;
using FCS_StorageSolutions.Mods.ItemTransferUnit.Buildable;
using FCS_StorageSolutions.Mods.ItemTransferUnit.Mono;
using FCSCommon.Utilities;
using HarmonyLib;
using QModManager.API;
using QModManager.API.ModLoading;
using SMLHelper.V2.Handlers;

namespace FCS_StorageSolutions
{
    /*
     * Storage Solutions is a pack of Subnautica Mods that allow you to store items in the game.
     * This mod can be configured to show certain mods before runtime and can only be changed before runtime.
     */
    [QModCore]
    public class QPatch
    {
        internal static Config Configuration { get; } = OptionsPanelHandler.Main.RegisterModOptions<Config>();
        public static bool IsDockedVehicleStorageAccessInstalled { get; set; }

        [QModPatch]
        public void Patch()
        {
            QuickLogger.Info($"Started patching. Version: {QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly())}");

            IsDockedVehicleStorageAccessInstalled = QModServices.Main.ModPresent("DockedVehicleStorageAccess");


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

                var dssFormattingStation = new DSSFormattingStationPatch();
                dssFormattingStation.Patch();

                var dssTerminal = new DSSTerminalPatch();
                dssTerminal.Patch();

                var dssAutoCrafter = new DSSAutoCrafterPatch();
                dssAutoCrafter.Patch();

                var dssAntenna = new DSSAntennaPatch();
                dssAntenna.Patch();

                var ItemTransferUnit = new ItemTransferUnitPatch();
                ItemTransferUnit.Patch();

                var wallServerRack = new DSSWallServerRackPatch();
                wallServerRack.Patch();

                var floorServerRack = new DSSFloorServerRackPatch();
                floorServerRack.Patch();
            }

            //Register debug commands
            ConsoleCommandsHandler.Main.RegisterConsoleCommands(typeof(DebugCommands));

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
