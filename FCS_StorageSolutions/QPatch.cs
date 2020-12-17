using System.Reflection;
using FCS_StorageSolutions.AlterraStorage.Buildable;
using FCS_StorageSolutions.Configuration;
using FCSCommon.Utilities;
using HarmonyLib;
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

        [QModPatch]
        public void Patch()
        {
            QuickLogger.Info($"Started patching. Version: {QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly())}");

            ModelPrefab.Initialize();

            AuxPatchers.AdditionalPatching();

            var alterraStorage = new AlterraStoragePatch();
            alterraStorage.Patch();

            //Register debug commands
            ConsoleCommandsHandler.Main.RegisterConsoleCommands(typeof(DebugCommands));

            //Harmony
            //var harmony = new Harmony("com.storagesolutions.fcstudios");
            //harmony.PatchAll(Assembly.GetExecutingAssembly());

            QuickLogger.Info($"Finished Patching");
        }
    }
}
