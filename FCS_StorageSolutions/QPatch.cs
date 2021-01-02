using System.Reflection;
using FCS_StorageSolutions.Configuration;
using FCS_StorageSolutions.Mods.AlterraStorage.Buildable;
using FCS_StorageSolutions.Mods.DataStorageSolutions.Buildable;
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
        internal static DSSConfig DSSConfiguration { get; } = OptionsPanelHandler.Main.RegisterModOptions<DSSConfig>();
        public static bool IsDockedVehicleStorageAccessInstalled { get; set; }

        [QModPatch]
        public void Patch()
        {
            QuickLogger.Info($"Started patching. Version: {QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly())}");

            IsDockedVehicleStorageAccessInstalled = QModServices.Main.ModPresent("DockedVehicleStorageAccess");


            ModelPrefab.Initialize();

            AuxPatchers.AdditionalPatching();

            var alterraStorage = new AlterraStoragePatch();
            alterraStorage.Patch();

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

            var wallServerRack = new DSSWallServerRackPatch();
            wallServerRack.Patch();

            var floorServerRack = new DSSFloorServerRackPatch();
            floorServerRack.Patch();

            //Register debug commands
            ConsoleCommandsHandler.Main.RegisterConsoleCommands(typeof(DebugCommands));

            //Harmony
            var harmony = new Harmony("com.storagesolutions.fcstudios");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            QuickLogger.Info($"Finished Patching");
        }
    }
}
