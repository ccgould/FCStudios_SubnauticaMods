using BepInEx;
using BepInEx.Logging;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Configuation;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.ModItems.Buildables.DroneDepotPort.Buildable;
using FCS_AlterraHub.ModItems.Spawnables.DebitCard.Spawnable;
using FCSCommon.Utilities;
using HarmonyLib;
using Nautilus.Handlers;
using System.Reflection;

namespace FCS_AlterraHub;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.nautilus")]
public class Plugin : BaseUnityPlugin
{
    public new static ManualLogSource Logger { get; private set; }
    /// <summary>
    /// Configuration of the AlterraHub Mod (For use in-game menu)
    /// </summary>
    public static Config Configuration { get; } = OptionsPanelHandler.RegisterModOptions<Config>();

    public static IModSettingsBase ModSettings => new ModSettings();
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();

    private void Awake()
    {
        // set project-scoped logger instance
        Logger = base.Logger;

        LanguageService.AdditionalPatching();


        //Register Ccommands
        RegisterCommands();

        StartCoroutine(MaterialHelpers.GetGameBaseMaterial(() =>
        {
            QuickLogger.Info($"Started patching. Version: {QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly())}");

            // Initialize custom prefabs
            InitializePrefabs();

            QuickLogger.Info($"Finished patching. Version: {QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly())}");
        }));

        // register harmony patches, if there are any
        Harmony.CreateAndPatchAll(Assembly, $"{PluginInfo.PLUGIN_GUID}");
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void InitializePrefabs()
    {
        //YeetKnifePrefab.Register();
        //Register mod pack
        FCSModsAPI.PublicAPI.RegisterModPack(PluginInfo.PLUGIN_NAME, Assembly.GetExecutingAssembly(), ModSettings.AssetBundleName);

        ////Add mod           
        //FCSModsAPI.PublicAPI.RegisterMod(MODNAME,new TestBuildable());

        //Add mod and patch.
        FCSModsAPI.PublicAPI.RegisterMod(PluginInfo.PLUGIN_NAME, "DP", new DroneDepotPortBuildable());

        //Add mod and patch.
        FCSModsAPI.PublicAPI.RegisterMod(PluginInfo.PLUGIN_NAME, "DC", new DebitCardSpawnable());

    }

    private static void RegisterCommands()
    {
        //Register Info commands
        ConsoleCommandsHandler.RegisterConsoleCommands(typeof(Commands));
    }
}