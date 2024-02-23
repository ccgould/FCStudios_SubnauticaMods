using BepInEx;
using BepInEx.Logging;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_EnergySolutions.Configuration;
using FCS_EnergySolutions.ModItems.Buildables.JetStream.Buildables;
using FCS_EnergySolutions.ModItems.Buildables.PowerStorage.Buildable;
using FCS_EnergySolutions.ModItems.Buildables.SolarCluster.Buildable;
using FCS_EnergySolutions.ModItems.Buildables.TelepowerPylon.Buildable;
using FCSCommon.Utilities;
using HarmonyLib;
using Nautilus.Handlers;
using System.Reflection;
using static FCS_EnergySolutions.Configuration.SaveData;

namespace FCS_EnergySolutions;
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.nautilus")]
[BepInDependency("FCS_AlterraHub")]
public class Plugin : BaseUnityPlugin
{
    public new static ManualLogSource Logger { get; private set; }

    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();

    /// <summary>
    /// Configuration of the Energy Solutions Mod (For use in-game menu)
    /// </summary>
    internal static Config Configuration { get; } = OptionsPanelHandler.RegisterModOptions<Config>();

    public static IModSettingsBase ModSettings => new ModSettings();

    internal static EnergrySolutionsSaveData TelepowerPylonSaveData { get; private set; }

    private void Awake()
    {
        // set project-scoped logger instance
        Logger = base.Logger;
        MaterialHelpers.GetIngameObjects();

        //Register mod pack
        FCSModsAPI.PublicAPI.RegisterModPack(PluginInfo.PLUGIN_NAME, Assembly.GetExecutingAssembly(), ModSettings.AssetBundleName, ModSaveManager.Save, ModSaveManager.LoadData);
        FCSModsAPI.PublicAPI.AddStoreCategory(PluginInfo.PLUGIN_GUID, "EnergySolutionsIcon_W", "Energy Solutions", PDAPages.EnergySolutions);
        StartCoroutine(MaterialHelpers.GetGameBaseMaterial(() =>
        {
            QuickLogger.Info($"Started patching [{PluginInfo.PLUGIN_NAME}]. Version: {QuickLogger.GetAssemblyVersion(Assembly)}");
            // Initialize custom prefabs
            InitializePrefabs();
        }));

        // register harmony patches, if there are any
        Harmony.CreateAndPatchAll(Assembly, $"{PluginInfo.PLUGIN_GUID}");
        LanguageHandler.RegisterLocalizationFolder();

       TelepowerPylonSaveData = SaveDataHandler.RegisterSaveDataCache<EnergrySolutionsSaveData>();

       // AlterraHubSaveData.OnFinishedLoading += OnFinishedLoading;



        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");


    }

    private void InitializePrefabs()
    {
        //YeetKnifePrefab.Register();
        FCSModsAPI.PublicAPI.RegisterMod(PluginInfo.PLUGIN_NAME, "SC", new SolarClusterBuildable());
        FCSModsAPI.PublicAPI.RegisterMod(PluginInfo.PLUGIN_NAME, "PS", new PowerStorageBuildable());
        FCSModsAPI.PublicAPI.RegisterMod(PluginInfo.PLUGIN_NAME, "JS", new JetStreamT242Buildable());
        FCSModsAPI.PublicAPI.RegisterMod(PluginInfo.PLUGIN_NAME, "AG", new AlterraGenBuildable());
        FCSModsAPI.PublicAPI.RegisterMod(PluginInfo.PLUGIN_NAME, "TP", new TelepowerPylonBuildable());
        FCSModsAPI.PublicAPI.RegisterMod(PluginInfo.PLUGIN_NAME, "UC", new UniversalChargerBuildable());

    }
}